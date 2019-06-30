using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Data;
using System.ComponentModel;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using Sap.Data.Hana;

using ArcGIS.Core.Geometry;

namespace SapHanaAddIn
{
    internal class TableViewerPanelViewModel : DockPane, INotifyPropertyChanged
    {
        public const string _dockPaneID = "SapHanaAddIn_TableViewerPanel";

        //locks to update collections to avoid thread collisions
        private Object _schemasLock = new Object();
        private Object _tablesLock = new Object();
        private Object _resultsLock = new Object();

        protected TableViewerPanelViewModel()
        {
        }

        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
            {
                return;
            }
            else
            {
                pane.Activate();
            }
        }

        protected override Task InitializeAsync()
        {
            // Initialize instance...
            return Task.FromResult(0);
        }
        protected override Task UninitializeAsync()
        {
            // Uninitialize instance...
            return Task.FromResult(0);
        }

        #region Tasks
        public async Task RefreshSchemasCallback()
        {
            await QueuedTask.Run(() =>
            {

                HanaCommand cmd = new HanaCommand("select TOP 1000 * from schemas", Globals.hanaConn);
                HanaDataReader dr = cmd.ExecuteReader();
                ObservableCollection<string> temp = new ObservableCollection<string>();
                while (dr.Read())
                {
                    Schema ss = new Schema { SchemaName = dr.GetString(0) };
                    temp.Add(ss.SchemaName);
                }
                dr.Close();
                lock (_schemasLock)
                {
                    SchemaColl = temp;
                }

            });
        }
        public async Task RefreshTablesCallback()
        {
            await QueuedTask.Run(() =>
            {
                HanaCommand cmd = new HanaCommand("SELECT TOP 1000 table_name FROM sys.tables where schema_name = '" + _currentSchema + "'", Globals.hanaConn);
                HanaDataReader dr = cmd.ExecuteReader();
                ObservableCollection<string> temp = new ObservableCollection<string>();
                while (dr.Read())
                {
                    temp.Add(dr.GetString(0));
                }
                dr.Close();
                lock (_tablesLock)
                {
                    Tables = temp;
                }
                
            });
        }
        public async Task TableSelectedCallback()
        {
            await QueuedTask.Run(() =>
            {
                //try
                //{
                //clear stuff out
                if (Results != null)
                {
                    lock (_resultsLock)
                    { Results = null; }
                }

                //get field names
                HanaCommand cmd = new HanaCommand("select COLUMN_NAME from SYS.TABLE_COLUMNS where schema_name like '" + _currentSchema + "' and table_name = '" + _currentTable + "'", Globals.hanaConn);
                HanaDataReader dr = cmd.ExecuteReader();
                List<string> colls = new List<string>();
                while (dr.Read())
                {
                    //fix bug with slash in column namestring
                    if (dr.GetString(0).IndexOfAny(new char[] { ' ', '/', '\\' }) != -1)
                    {
                        var s = '"' + dr.GetString(0) + '"';
                        colls.Add(s);
                    }
                    else
                    { colls.Add(dr.GetString(0)); }
                }
                dr.Close();

                //set default querytext string for the UI txt box 
                _querytext.SelectString = "SELECT TOP 1000 " + string.Join(", ", colls.ToArray()) + " FROM \"" + _currentSchema + "\".\"" + _currentTable + "\"";

                //find spatial column
                SpatialCol.SelectString = "";
                cmd.CommandText = "select COLUMN_NAME from SYS.TABLE_COLUMNS where schema_name like '" + _currentSchema + "' and table_name = '" + _currentTable + "' and data_type_id = 29812";
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        _spatialCol.SelectString = dr.GetString(0);
                    }
                }
                else
                {
                    _spatialCol.SelectString = "none";
                }
                dr.Close();

                //find objectid
                ObjidCol.SelectString = "";
                cmd.CommandText = "select COLUMN_NAME from SYS.TABLE_COLUMNS where schema_name like '" + _currentSchema + "' and table_name = '" + _currentTable + "' and COLUMN_NAME = 'OBJECTID'";
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        _objidCol.SelectString = dr.GetString(0);
                    }
                }
                else
                {
                    _objidCol.SelectString = "none";
                }
                dr.Close();

                //}
                //catch (Exception ex)
                //{
                //    FrameworkApplication.State.Deactivate("condition_state_isconnected");
                //    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Error in SetPropertiesForTable. " + ex.Message);
                //}

            });
        }
        public async Task ExecuteSqlCallback()
        { 
            await QueuedTask.Run(() =>
            {
                //check for valid connection
                if (Globals.hanaConn == null || Globals.hanaConn.State != ConnectionState.Open)
                {
                    //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Connect to a database first.", "Not connected");
                    return;
                }

                //string txtSQLStatement = txtQueryText.Text;
                string ts = _querytext.SelectString;

                if (ts.Trim().Length < 1)
                {
                    //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Please enter the command text.", "Empty command text");
                    return;
                }

                string qtest = "";
                string sc = _spatialCol.SelectString;
                if (sc != "none" && sc != "")
                {
                    qtest = ts.Replace(sc, sc + ".st_aswkt()" + " as " + sc);
                }
                else
                {
                    qtest = ts;
                }

                HanaCommand cmd = new HanaCommand(qtest, Globals.hanaConn);
                HanaDataReader dr = null;
                dr = cmd.ExecuteReader();
                DataTable dt = new DataTable();
                if (dr != null)
                {
                    dt.Load(dr);
                }
                dr.Close();
                
                lock (_resultsLock)
                {
                    Results = dt.DefaultView;
                }

                ActRecCount.SelectString = dt.DefaultView.Count.ToString();
            });
    }
        public async Task AddToTOCCallback()
        {
            await QueuedTask.Run(() =>
            {

                //This is for reading the odbc data sources from the machine registery and is not currently being used.
                //// Opening a Non-Versioned db instance.
                //RegistryKey reg = (Registry.LocalMachine).OpenSubKey("Software");
                //reg = reg.OpenSubKey("ODBC");
                //reg = reg.OpenSubKey("ODBC.INI");
                //reg = reg.OpenSubKey("ODBC Data Sources");
                //string instance = "";
                //foreach (string item in reg.GetValueNames())
                //{
                //    instance = item;
                //    string vvv = reg.GetValue(item).ToString();
                //}
               
                ArcGIS.Desktop.Framework.Contracts.ComboBoxItem item2 = (ArcGIS.Desktop.Framework.Contracts.ComboBoxItem)cboEnv.cboBox.SelectedItem;
                    ConnectionItem connitem = item2.Icon as ConnectionItem;
                    string tst2 = new System.Net.NetworkCredential(string.Empty, connitem.pass).Password;
                    // Provided that a login called gdb has been created and corresponding schema has been created with the required permissions.
                    DatabaseConnectionProperties connectionProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.Hana)
                    {
                        AuthenticationMode = AuthenticationMode.DBMS,
                        Instance = cboEnv.cboBox.Text, //@"sapqe2hana",
                        User = connitem.userid,
                        Password = tst2,
                        Version = "dbo.DEFAULT"
                    };

                    ////create a query layer
                    //using (Geodatabase geodatabase = new Geodatabase(connectionProperties))
                    //{
                    //    Connector pCon = geodatabase.GetConnector();
                    //    pCon.ToString();
                    //}

                    string ts = _querytext.SelectString;

                    if (ts.Trim().Length < 1)
                    {
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Please enter the command text.", "Empty command text");
                        return;
                    }

                    string qtest = "";
                    string sc = _spatialCol.SelectString;
                    if (sc != "none" && sc != "")
                    {
                        qtest = ts.Replace(sc, sc + ".st_aswkt()" + " as " + sc);
                    }
                    else
                    {
                        qtest = ts;
                    }

                using (Database db = new Database(connectionProperties))
                {

                    //create table name for the layer
                    Random r = new Random();
                    int n = r.Next();
                    string s = n.ToString();
                    s = s.Substring(s.Length - 4);
                    string lyrname = _currentTable + "_" + s;

                    //create query layer
                    ArcGIS.Core.Data.QueryDescription qds = db.GetQueryDescription(qtest, lyrname);
                    ArcGIS.Core.Data.QueryDescription qds2 = null;
                    string oflds = qds.GetObjectIDFields();
                    string oc = "";
                    string fakeObjSql = "none";
                    string qtest2 = "none";

                    //determine objectid
                    // Pro requires a pk
                    // can't assume the field name OBJECTID in HANA is a valid field type to use for objectid according to Esri...
                    if (oflds.Length > 0)
                    {
                        oc = oflds.ToString().Split(',')[0];
                        qds.SetObjectIDFields(oc);
                    }
                    else
                    {
                        //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("This table does not have a valid key field in the database.  Pro requires at least one field that can be used as an Objectid field. The field must be not null, contain unique values, and be one of the following data types: Integer, string, GUID or Date.  An attempt will be made to create a field for you.");
                        //construct objectid as it is a required column in Pro
                        //row_number() over(partition by " + tempid + ") as OBJECTID";))
                        //string fakeObjCol = objidCol.Split('-')[1];
                        var lst = qds.GetFields();
                        if (lst != null)
                        {
                            if (lst.Count > 0)
                            {
                                oc = lst[0].Name;  //use this as a dummy field for the sql
                            }
                        }
                        fakeObjSql = "row_number()" + " over(partition by " + oc + ")" + " as OBJID";
                        qtest2 = qtest.Insert(ts.IndexOf("FROM") - 1, ", " + fakeObjSql);
                        qds2 = db.GetQueryDescription(qtest2, lyrname);
                    }

                    ArcGIS.Core.Data.QueryDescription qdsfinal = null;
                    if (fakeObjSql == "none")
                    { qdsfinal = db.GetQueryDescription(qtest, lyrname); }
                    else
                    { qdsfinal = db.GetQueryDescription(qtest2, lyrname); }

                    ArcGIS.Core.Data.Table pTab = db.OpenTable(qdsfinal);
                    var serverConnection = new CIMInternetServerConnection { URL = "Fill in the URL of the WMS service" };
                    var connt = new CIMWMTSServiceConnection { ServerConnection = serverConnection, };

                    if (qdsfinal.GetShapeColumnName() != null)
                    {
                        FeatureLayer pFL = (FeatureLayer)LayerFactory.Instance.CreateLayer(pTab.GetDataConnection(), MapView.Active.Map, layerName: lyrname);
                        pFL.Select(null, SelectionCombinationMethod.New);
                        MapView.Active.ZoomToSelected();
                    }
                    else
                    {
                        StandaloneTable pFL = (StandaloneTable)StandaloneTableFactory.Instance.CreateStandaloneTable(pTab.GetDataConnection(), MapView.Active.Map, tableName: lyrname);
                    }
                }

            });


        }
        public async Task SetTotalRecordsCount()
        {
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                try
                {
                    string qtst = "SELECT COUNT(*) FROM  " + _currentSchema + "." + _currentTable;
                    HanaCommand cmd2 = new HanaCommand(qtst, Globals.hanaConn);
                    HanaDataReader dr2 = null;
                    System.Windows.Forms.DataGrid dgResults2 = new System.Windows.Forms.DataGrid();
                    dr2 = cmd2.ExecuteReader();

                    if (dr2 != null)
                    {
                        DataTable dt2 = new DataTable();

                        dt2.Load(dr2);
                        if (dr2 != null)
                        {
                            if (dt2.Rows.Count > 0)
                            {
                                TotRecCount.SelectString = dt2.Rows.Count.ToString();
                            }
                            else if (dt2.Rows.Count == 0)
                            {
                                TotRecCount.SelectString = "0";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Error TotRecordCount. " + ex.Message);
                }
            });
        }
        #endregion

        #region Dockpane Properties
        private string _heading = "Search with SQL";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        private string _heading2 = "Results";
        public string Heading2
        {
            get { return _heading2; }
            set
            {
                SetProperty(ref _heading2, value, () => Heading2);
            }
        }

        private ObservableCollection<string> _schemacoll = new ObservableCollection<string>();
        public ObservableCollection<string> SchemaColl
        {
            get
            {
                return _schemacoll;
            }
            set
            {
                _schemacoll = value;
                NotifyPropertyChanged("SchemaColl");
            }
        }

        private ObservableCollection<string> _tables = new ObservableCollection<string>();
        public ObservableCollection<string> Tables
        {
            get
            {
                return _tables;
            }
            set
            {
                _tables = value;
                NotifyPropertyChanged("Tables");
            }
        }

        private string _currentSchema;
        public string CurrentSchema
        {
            get { return _currentSchema; }
            set
            {
                _currentSchema = value;
                NotifyPropertyChanged("CurrentSchema");
            }
        }

        private string _currentTable;
        public string CurrentTable
        {
            get { return _currentTable; }
            set
            {
                _currentTable = value;
                NotifyPropertyChanged("CurrentTable");
            }
        }

        private DataView _results;
        public DataView Results
        {
            get
            {
                return _results;
            }
            private set
            {
                _results = value;
                NotifyPropertyChanged("Results");
            }
        }

        private SelectionString _querytext;
        public SelectionString QueryTxt
        {
            get
            {
                if (_querytext == null)
                {
                    _querytext = new SelectionString();
                }
                return _querytext;
            }
            set
            {
                _querytext = value;
                NotifyPropertyChanged("QueryTxt");
            }
        }

        private SelectionString _spatialCol;
        public SelectionString SpatialCol
        {
            get
            {
                if (_spatialCol == null)
                {
                    _spatialCol = new SelectionString();
                }
                return _spatialCol;
            }
            set
            {
                _spatialCol = value;
                NotifyPropertyChanged("SpatialCol");
            }
        }

        private SelectionString _objidCol;
        public SelectionString ObjidCol
        {
            get
            {
                if (_objidCol == null)
                {
                    _objidCol = new SelectionString();
                }
                return _objidCol;
            }
            set
            {
                _objidCol = value;
                NotifyPropertyChanged("ObjidCol");
            }
        }

        private SelectionString _totrecCount;
        public SelectionString TotRecCount
        {
            get
            {
                if (_totrecCount == null)
                {
                    _totrecCount = new SelectionString();
                }
                return _totrecCount;
            }
            set
            {
                _totrecCount = value;
                NotifyPropertyChanged("TotRecCount");
            }
        }

        private SelectionString _actrecCount;
        public SelectionString ActRecCount
        {
            get
            {
                if (_actrecCount == null)
                {
                    _actrecCount = new SelectionString();
                }
                return _actrecCount;
            }
            set
            {
                _actrecCount = value;
                NotifyPropertyChanged("ActRecCount");
            }
        }
        #endregion

    }

    #region btnSqlExplorer
    public class btnSqlExplorer : Button
    {
        protected override void OnClick()
        {
            TableViewerPanelViewModel.Show();
        }
    }
    #endregion

    #region cboEnv Class
    public class cboEnv : ArcGIS.Desktop.Framework.Contracts.ComboBox
    {
        private static cboEnv comboBox;
        public static cboEnv cboBox
        {
            get
            { return comboBox; }
            set
            {
                comboBox = value;
            }
        }
        public cboEnv()
        {
            cboBox = this;
        }

        public void GetConnectionItems()
        {
            Clear();
            foreach (ConnectionItem item in HanaConfigModule.Current.ConnectionItems)
            {
                this.Add(new ArcGIS.Desktop.Framework.Contracts.ComboBoxItem(item.name, item));
            }
            if (this.ItemCollection.Count == 0)
            {
                //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("There were no HANA connections found.  Please add connections by going to Pro's backstage, then Options, then Hana Properties.");
            }
        }
        public void OpenConnForEnv()
        {
            //try
            //{
                //FrameworkApplication.State.Activate("condition_state_hasProps");

                //if (cboEnv.cboBox.SelectedItem == null)
                //{
                //    FrameworkApplication.State.Deactivate("condition_state_hasProps");
                //    return;
                //}

                //recreate connection each time cboenv changes 
                if (Globals.hanaConn == null)
                {
                    Globals.hanaConn = new HanaConnection();
                }
                else
                {
                    Globals.hanaConn.Close();
                    Globals.hanaConn = null;
                    Globals.hanaConn = new HanaConnection();
                }

                ComboBoxItem itm = (ComboBoxItem)cboEnv.cboBox.SelectedItem;
                string txtitm = itm.Text;
                ConnectionItem connitem = itm.Icon as ConnectionItem;
                Globals.hanaConn.ConnectionString = "Server=" + connitem.server;
                System.Security.SecureString ss = new System.Security.SecureString();
                ss = connitem.pass;
                ss.MakeReadOnly();
                HanaCredential hcr = new HanaCredential(connitem.userid, ss);
                Globals.hanaConn.Credential = hcr;
                Globals.hanaConn.Open();

                IPlugInWrapper wrapper = FrameworkApplication.GetPlugInWrapper("lblHasConn");

                if (wrapper != null)
                {
                    wrapper.Caption = "Connected";
                }
                IPlugInWrapper wrapper2 = FrameworkApplication.GetPlugInWrapper("btnConnect");
                if (wrapper2 != null)
                {
                    wrapper2.Caption = "Connected to " + txtitm;
                }
                IPlugInWrapper wrapper3 = FrameworkApplication.GetPlugInWrapper("lblHasConnTrue");
                if (wrapper3 != null)
                {
                    wrapper3.Caption = "Connected to " + txtitm;
                    wrapper3.Checked = false;
                }
            //}
            //catch (Exception ex)
            //{
            //    FrameworkApplication.State.Deactivate("condition_state_isconnected");

            //    IPlugInWrapper wrapper = FrameworkApplication.GetPlugInWrapper("lblHasConn");
            //    if (wrapper != null)
            //    {
            //        wrapper.Caption = "Not Connected";
            //    }
            //    IPlugInWrapper wrapper2 = FrameworkApplication.GetPlugInWrapper("btnConnect");
            //    if (wrapper2 != null)
            //    {
            //        wrapper2.Caption = "Not Connected";
            //    }
            //    IPlugInWrapper wrapper3 = FrameworkApplication.GetPlugInWrapper("lblHasConnTrue");
            //    if (wrapper3 != null)
            //    {
            //        wrapper3.Caption = "Not Connected";
            //        wrapper3.Checked = false;
            //    }
                
            //    //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Error in OnSelectionChanged. " + ex.Message);
            //}
        }

        protected override async void OnSelectionChange(ArcGIS.Desktop.Framework.Contracts.ComboBoxItem item)
        {
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                OpenConnForEnv();

                FrameworkApplication.State.Activate("condition_state_isconnected");
            });
        }
        protected override async void OnDropDownOpened()
        {
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                GetConnectionItems();
            });
        }


    }
    #endregion

    #region ICommand Class to relay NotifyPropertyChanged
    public class CommandHandler : ICommand
    {
        private Action _action;
        private bool _canExecute;
        public CommandHandler(Action action, bool canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    #endregion
}

