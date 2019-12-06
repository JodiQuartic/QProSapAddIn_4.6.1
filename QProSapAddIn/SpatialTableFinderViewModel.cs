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
using Oracle.ManagedDataAccess.Client;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Core.Geometry;
using System.Threading;

namespace QProSapAddIn
{
    internal class SpatialTableFinderViewModel : DockPane, INotifyPropertyChanged
    {
        public const string _dockPaneID2 = "QProSapAddIn_SpatialTableFinder";

        //locks to update collections to avoid thread collisions
        private Object _aLock = new Object();

        protected SpatialTableFinderViewModel()
        {
        }

        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID2);
            if (pane == null)
            {
                return;
            }
            else
            {
                string dateInString = "02.01.2020";

                DateTime startDate = DateTime.Parse(dateInString);
                DateTime expiryDate = startDate.AddDays(29);
                if (DateTime.Now > expiryDate)
                {
                    MessageBox.Show("This Trial has expired.  Thank you for trying Q Pro for SAP. " + Environment.NewLine + Environment.NewLine + "We would appreciate any feedback you may have.  Please report bugs and/or enhancements to admin@quarticsolutions.com ." + Environment.NewLine + Environment.NewLine + "You can uninstall this addin by using ArcGIS Pro's AddIn Manager." + Environment.NewLine + Environment.NewLine + "If you are interested in purchasing Q Pro for SAP please contact Quartic Solutions at info@quarticsolutions.com or visit our website www.quarticsolutions.com");
                }
                else
                {
                    pane.Activate();
                    
                }

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
        public async Task GetSpatTabsCallback()
        {
            try
            {
                await QueuedTask.Run(() =>
                {
                    //string queryStr = QueryText.SelectString;
                    string queryStr = "select * from sde.st_GEOMETRY_COLUMNS ORDER BY TABLE_NAME ";

                    if (Globals.DBConn.State != ConnectionState.Open)
                        Globals.DBConn.Open();
                    if (Globals.DBConn == null || Globals.DBConn.State != ConnectionState.Open)
                    {
                        MessageBox.Show("There is no open database connection. Please connect to database first. ");
                        lock (_aLock)
                        {
                            BtnMapVis = false;
                        }
                        return;
                    }

                    DataTable dt = new DataTable();
                    int cnt = 0;
                    if (Globals.RdbmsType == "Hana")
                    {
                      
                    }
                    if (Globals.RdbmsType == "Oracle")
                    {
                        DataSet dataSet = new DataSet();
                        OracleCommand cmd = new OracleCommand(queryStr, Globals.DBConn);
                        using (OracleDataAdapter dataAdapter = new OracleDataAdapter())
                        {
                            dataAdapter.SelectCommand = cmd;
                            dataAdapter.Fill(dataSet);
                            dt = dataSet.Tables[0];
                            cnt = dt.Rows.Count;
                            _tabCount.SelectString = cnt.ToString();

                        }
                        if (cnt == 0)
                        {
                            lock (_aLock)
                            {
                                BtnMapVis = false;
                            }
                            lock (_aLock)
                            {
                                BtnGetSpatVis = true;
                            }

                            MessageBox.Show("There are no rows in the selected table. ");

                            return;
                        }
                        else
                        {
                            lock (_aLock)
                            {
                                Results = dt.DefaultView;
                                
                            }
                            lock (_aLock)
                            {
                                BtnMapVis = true;
                            }
                            lock (_aLock)
                            {
                                BtnGetSpatVis = false;
                            }
                        }

                        if (Globals.DBConn.State != ConnectionState.Closed)
                            Globals.DBConn.Close();
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("The sql statement was not able to execute. " + ex.Message.ToString() + " Please review the sql statement.");
                lock (_aLock)
                {
                    BtnMapVis = false;
                }
                lock (_aLock)
                {
                    BtnGetSpatVis = true;
                }
                if (Globals.DBConn.State != ConnectionState.Closed)
                    Globals.DBConn.Close();
            }
        }
        public async Task AddToMapCallback2(CancelableProgressorSource cpd)
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

                    //create table name for the toc layer
                    Random r = new Random();
                    int n = r.Next();
                    string s = n.ToString();
                    s = s.Substring(s.Length - 4);
                    string lyrname = _currentTable + "_" + s;

                    //open an Esri database connection type to sap
                    ComboBoxItem item2 = (ComboBoxItem)cboEnv.cboBox.SelectedItem;
                    ConnectionItem connitem = item2.Icon as ConnectionItem;
                    string tst2 = new System.Net.NetworkCredential(string.Empty, connitem.pass).Password;
                    string serverport = connitem.server.ToString();
                    DatabaseConnectionProperties connectionProperties;

                    if (Globals.RdbmsType == "Hana")
                    {
                        string inst = serverport.Substring(0, serverport.IndexOf(":"));
                        connectionProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.Hana)
                        {
                            AuthenticationMode = AuthenticationMode.DBMS,
                            Instance = inst,  //cboEnv.cboBox.Text, //@"sapqe2hana",
                            User = connitem.userid,
                            Password = tst2,
                            Version = "dbo.DEFAULT"
                        };
                    }
                    else if (Globals.RdbmsType == "Oracle")
                    {
                        connectionProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.Oracle)
                        {
                            AuthenticationMode = AuthenticationMode.DBMS,
                            Instance = serverport,  //cboEnv.cboBox.Text, //@"sapqe2hana",
                            User = connitem.userid,
                            Password = tst2,
                            Version = "sde.DEFAULT"
                        };
                    }

                    //=====================add to map
                    QueryText.SelectString = "SELECT * FROM \"" + _currentSchema + "\".\"" + _currentTable + "\"";

                    using (Database db = new Database(connectionProperties))
                    {
                        QueryDescription qds = db.GetQueryDescription(_querytext.SelectString, lyrname);

                        Table pTab2 = db.OpenTable(qds);

                        if (_spatialCol.SelectString != "" && _objidCol.SelectString != "none")
                        {
                            _strMessage.SelectString = "Adding Layer";
                            // Add a new layer to the map
                            FeatureLayer pFL = (FeatureLayer)LayerFactory.Instance.CreateLayer(pTab2.GetDataConnection(), MapView.Active.Map, layerName: lyrname);
                            pFL.Select(null, SelectionCombinationMethod.New);
                            MapView.Active.ZoomToSelected();
                            pFL.ClearSelection();
                            FeatureClass featureClass = pFL.GetFeatureClass();
                            

                            _strMessage.SelectString = "Layer added.";
                            cpd.Message = "Layer added.";
                            MessageBox.Show("Layer added successsful to map.");

                        }
                        else
                        {
                            StandaloneTable pFL = (StandaloneTable)StandaloneTableFactory.Instance.CreateStandaloneTable(pTab2.GetDataConnection(), MapView.Active.Map, tableName: lyrname);
                            _strMessage.SelectString = "Table added.";
                            

                            cpd.Message = "Standalone table added successsfuly to map.";
                            MessageBox.Show("Standalone table added successsfuly to map.");
                        }
                    }
                    cpd.Dispose();

                });

            }
            catch (Exception ex)
            {
                lock (_aLock)
                { BtnMapVis = false; }

                MessageBox.Show("The table could not be added to ArcGIS Pro.  " + ex.Message.ToString());
                if (Globals.DBConn.State != ConnectionState.Closed)
                    Globals.DBConn.Close();
               
                cpd.Message = "Table not added.";
                cpd.Dispose();
            }
        }

        private string _heading = "Spatial Tables";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
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
        public SelectionString QueryText
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
                NotifyPropertyChanged("QueryText");
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
        private DataRowView _selrow;
        public DataRowView SelRow
        {
            get
            {
                return _selrow;
            }
            set
            {
                _selrow = value;
                NotifyPropertyChanged("SelRow");
            }
        }

        private bool _btnGetSpatVis = true;
        public bool BtnGetSpatVis
        {
            get { return _btnGetSpatVis; }
            set
            {
                SetProperty(ref _btnGetSpatVis, value, () => BtnGetSpatVis);
                NotifyPropertyChanged("BtnGetSpatVis");
            }
        }

        private bool _btnMapVis = false;
        public bool BtnMapVis
        {
            get { return _btnMapVis; }
            set
            {
                SetProperty(ref _btnMapVis, value, () => BtnMapVis);
                NotifyPropertyChanged("BtnMapVis");
            }
        }

        private SelectionString _tabCount;
        public SelectionString TabCount
        {
            get
            {
                if (_tabCount == null)
                {
                    _tabCount = new SelectionString();
                }
                return _tabCount;
            }
            set
            {
                _tabCount = value;
                NotifyPropertyChanged("TabCount");
            }
        }


    }

    public class btnFindSpatial : Button
    {
        protected override void OnClick()
        {
            SpatialTableFinderViewModel.Show();

        }
    }

    #endregion



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
}
   





