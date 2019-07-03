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
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Core.Geometry;
using System.Threading;

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
            try
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
            catch (Exception ex)
            {
                MessageBox.Show("Error in RefreshSchemas:  " + ex.ToString(), "Error");
            }
        }
        public async Task RefreshTablesCallback()
        {
            try
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
            catch (Exception ex)
            {
                MessageBox.Show("Error in RefreshTables:  " + ex.ToString(), "Error");
            }
        }
        public async Task TableSelectedCallback()
        {
            try { 
            await QueuedTask.Run(() =>
            {
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

            });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in TableSelected:  " + ex.ToString(), "Error");
            }
        }
        public async Task ExecuteSqlCallback()
        { 
            try { 
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
            catch (Exception ex)
            {
                MessageBox.Show("Error in ExecuteSql:  " + ex.ToString(), "Error");
            }
        }
        public async Task AddToTOCCallback(CancelableProgressorSource cpd)
        {
            try
            {
                await QueuedTask.Run(() =>
                {
                    
                    if (MapView.Active == null)
                {
                    _strMessage.SelectString = "No active map view.";
                    cpd.Message = "There is no active map view.  Please select a map and try again.";
                    return;
                }



                //===This is for reading the odbc data sources from the machine registery and is not currently being used.
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

                //setup connection properties
                ArcGIS.Desktop.Framework.Contracts.ComboBoxItem item2 = (ArcGIS.Desktop.Framework.Contracts.ComboBoxItem)cboEnv.cboBox.SelectedItem;
                ConnectionItem connitem = item2.Icon as ConnectionItem;
                string tst2 = new System.Net.NetworkCredential(string.Empty, connitem.pass).Password;
                DatabaseConnectionProperties connectionProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.Hana)
                {
                    AuthenticationMode = AuthenticationMode.DBMS,
                    Instance = cboEnv.cboBox.Text, //@"sapqe2hana",
                    User = connitem.userid,
                    Password = tst2,
                    Version = "dbo.DEFAULT"
                };

                //==================had to break code up more with using statements cause kept having thread probs

                //create table name for the layer
                Random r = new Random();
                int n = r.Next();
                string s = n.ToString();
                s = s.Substring(s.Length - 4);
                string lyrname = _currentTable + "_" + s;

                //get all the variables needed from qds first
                string qtest = _querytext.SelectString.Trim();
                string oflds = "";
                IReadOnlyList<Field> lst;
                string db_sc = SpatialCol.SelectString;
                string esri_sc = "";
                string db_oc = ObjidCol.SelectString;
                string esri_oc = "";
                GeometryType st = GeometryType.Unknown;
                SpatialReference sr = null;
                string sri = "";

                using (Database db = new Database(connectionProperties))
                {
                    if (qtest.Length < 1)
                    {
                        _strMessage.SelectString = "Empty Sql.";
                        return;
                    }
                    else
                    {
                        QueryDescription qds = db.GetQueryDescription(qtest, lyrname);
                        lst = qds.GetFields();
                        oflds = qds.GetObjectIDFields();
                        esri_sc = qds.GetShapeColumnName();
                        esri_oc = qds.GetObjectIDColumnName();
                        st = qds.GetShapeType();
                        sr = qds.GetSpatialReference();
                        sri = qds.GetSRID();
                    }
                }

                if (lst == null)   //table has no fields
                    return;

                //now do work with variables from qds
                int objfieldcount = oflds.Split(',').Length - 1;
                string objForSet = "";
                bool hasObj = false;
                string fakeObjSql = "";
                bool hasShape = false;
                //bool hasSlashes = false;

                //==============determine field to use for objectid - pro requires a pk - can't assume the field name OBJECTID in HANA is a valid field type to use for objectid according to Esri...
                if (objfieldcount == 0)
                {
                    //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("This table does not have a valid key field in the database.  Pro requires at least one field that can be used as an Objectid field. The field must be not null, contain unique values, and be one of the following data types: Integer, string, GUID or Date.  An attempt will be made to create a field for you.");
                    _strMessage.SelectString = "Table missing key.";
                    //does db think there is a pk?
                    if (db_oc != null)
                    {
                        //trick esri into use this?
                        //objForSet = db_oc;
                        //hasObj = true;
                        //ObjidCol.SelectString = db_oc;

                        objForSet = esri_oc;
                        ObjidCol.SelectString = "default";
                    }
                    else
                    {
                        //last try to construct objectid as it is a required column in Pro
                        //row_number() over(partition by " + tempid + ") as OBJECTID";))
                        //string fakeObjCol = objidCol.Split('-')[1];
                        if (lst != null && lst.Count > 0)
                        {
                            string oc = lst[0].Name;  //use this as a dummy field to put the partition statement in (as long as it is not named "OBJECTID" esri crashes)

                            //if (esri_oc == "OBJECTID")
                            //{
                            //    //drop OBJECTID from field list
                            //    qtest0 = qtest0.Replace("OBJECTID,", "");
                            //}
                            //else
                            //{ }

                            fakeObjSql = ", row_number()" + " over(partition by " + oc + ")" + " as QID ";
                            objForSet = oc;
                            ObjidCol.SelectString = "default";
                            _strMessage.SelectString = "Attempted to create key.";
                            cpd.Message = "This data does snot have a key field to use for OBJECTID.  An attempt will be made to create one.";
                            }
                    }
                }
                if (objfieldcount == 1)
                {
                    esri_oc = oflds.ToString().Split(',')[0];
                    objForSet = esri_oc;              // prepare for SetObjectidfeilds
                    ObjidCol.SelectString = "ESRI_OID";  //updateprop
                    hasObj = true;
                        cpd.Message = "This data has a valid key field to use for OBJECTID.";
                    }
                if (objfieldcount > 1)
                {
                    //fix backslash slash bug
                    //string fldLst = "";
                    //int cnt = 0;

                    //foreach (Field fld in lst)
                    //{
                    //    //fix bug with slash in column namestring
                    //    if (fld.Name.IndexOfAny(new char[] { ' ', '/', '\\' }) != -1)
                    //    {
                    //        var f = '"' + fld.Name + '"';
                    //        if (cnt == 0)
                    //        {
                    //            fldLst = fldLst + f;  //first field don't put a comma
                    //        }
                    //        else
                    //        {
                    //            fldLst = fldLst + "," + f;
                    //        }
                    //        hasSlashes = true;
                    //    }
                    //    else
                    //    {
                    //        if (cnt == 0)
                    //        {
                    //            fldLst = fldLst + fld.Name;
                    //        }
                    //        else
                    //        {
                    //            fldLst = fldLst + "," + fld.Name;
                    //        }
                    //    }

                    //    cnt = cnt + 1;
                    //}
                    //objForSet = fldLst;   // prepare for SetObjectidfeilds

                    //just use first field in lst as objid
                    objForSet = oflds.ToString().Split(',')[0];
                    ObjidCol.SelectString = "multi";    //updateprop
                    hasObj = true;
                        cpd.Message = "This data has a valid key field to use for OBJECTID.";
                    }

                //==================determine is shape field
                if (esri_sc != null && esri_sc != "")
                {
                    //determine shape type
                    //TODO 

                    //determine if it should be converted to .st_aswkt
                    //TODO

                    SpatialCol.SelectString = esri_sc;
                    hasShape = true;
                    cpd.Message = "This data has a valid shapefield";

                    }
                    else
                {
                    // //if (db_sc != "none")   // (data_type_id = 29812)
                    //TODO - why does db think there is an ST_GEOMETRY and esri doesn't?
                    SpatialCol.SelectString = "none";
                        cpd.Message = "This data does  not have a valid shapefield";
                    }

                //==============fix the sql statement based on new knowledge
                string qtest1 = "";
                string qtest2 = "";
                //add conversion for objid to ssql
                if (hasObj == false)
                {
                    int inx = qtest.IndexOf("FROM") - 1;
                    qtest1 = qtest.Insert(inx, fakeObjSql);
                }
                else
                {
                    qtest1 = qtest;
                }
                //add conversion for shape to sql
                if (hasShape)
                {
                    qtest2 = qtest1; // qtest1.Replace(esri_sc, esri_sc + ".st_aswkt()" + " as " + esri_sc);
                }
                else
                {
                    qtest2 = qtest1;
                }

                //=============finally add to map
                Database db2 = new Database(connectionProperties);
                QueryDescription qdsfinal = db2.GetQueryDescription(qtest2, lyrname);  //reapply with new sql statement
                if (objForSet != "none")
                {
                    qdsfinal.SetObjectIDFields(objForSet);
                }
                //qdsfinal.SetShapeType(GeometryType.Point);
                //qdsfinal.SetSpatialReference;
                //qdsfinal.SetSRID
                Table pTab2 = db2.OpenTable(qdsfinal);

                if (hasShape)
                {
                    // Add a new layer to the map
                    FeatureLayer pFL = (FeatureLayer)LayerFactory.Instance.CreateLayer(pTab2.GetDataConnection(), MapView.Active.Map, layerName: lyrname);
                    pFL.Select(null, SelectionCombinationMethod.New);
                    MapView.Active.ZoomToSelected();
                    pFL.ClearSelection();

                    _strMessage.SelectString = "Layer added.";
                     cpd.Message = "Layer added successsful to map.";

                    }
                    else
                {

                    //if (objForSet != "none")
                    //{
                    //    if (hasSlashes)
                    //    { qdsfinal.SetObjectIDFields(objForSet); }
                    //}

                    StandaloneTable pFL = (StandaloneTable)StandaloneTableFactory.Instance.CreateStandaloneTable(pTab2.GetDataConnection(), MapView.Active.Map, tableName: lyrname);
                    _strMessage.SelectString = "Table added.";
                     cpd.Message = "Standalone table added successsful to map.";
                    }
                    cpd.Dispose();
                });
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in AddToMap:  " + ex.ToString(), "Error");
            }

        }
        public async Task SetTotalRecordsCount()
        {
            try {

                await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
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
              
            });

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in SetTotalRecordsCount:  " + ex.ToString(), "Error");
            }
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

        private SelectionString _strMessage;
        public SelectionString StrMessage
        {
            get
            {
                if (_strMessage == null)
                {
                    _strMessage = new SelectionString();
                }
                return _strMessage;
            }
            set
            {
                _strMessage = value;
                NotifyPropertyChanged("StrMessage");
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

