using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using Sap.Data.Hana;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Data;
using System.ComponentModel;
using ArcGIS.Desktop.Framework.Events;


namespace SapHanaAddIn
{
    internal class TableViewerPanelViewModel : DockPane, INotifyPropertyChanged
    {
        public const string _dockPaneID = "SapHanaAddIn_TableViewerPanel";
        private object _schemasLock = new object();
        private object _tablesLock = new object();
        private object _resultsLock = new object();
       
        protected TableViewerPanelViewModel()
        {
            //See blog http://10rem.net/blog/2012/01/20/wpf-45-cross-thread-collection-synchronization-redux
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(Schemas, _schemasLock);
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(Tables, _tablesLock);

            try
            {
                if (Globals.hanaConn == null || Globals.hanaConn.State == System.Data.ConnectionState.Closed)
                {
                    MessageBox.Show("Select an environment first.", "No environment is selected.");
                    return;
                }
                else
                {
                    HanaCommand cmd = new HanaCommand("select * from schemas", Globals.hanaConn);
                    HanaDataReader dr = cmd.ExecuteReader();
                    _schemas.Clear();
                    while (dr.Read())
                    {
                        _schemas.Add(dr.GetString(0));
                    }
                    dr.Close();
                    //_schemas = Globals.GSchemas;

                }

                
            }
            catch (HanaException ex)
            {
                MessageBox.Show(ex.Errors[0].Source + " : " + ex.Errors[0].Message + " (" +
                 ex.Errors[0].NativeError.ToString() + ")",
                 "Failed to initialize table drop down.");
            }
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
            ////on first show set the schemas from the cboEnv setting of global var
            //if (Globals.GSchemas.Count > 0)
            //{
            //    //lock (_schemasLock)
            //    //{ 
            //    Schemas = Globals.GSchemas;
            //    //}
            //}

            return base.InitializeAsync();
        }

        public async void RefreshSchemas()
        {
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                try
                    {
                        //set schemas for dropdown
                        HanaCommand cmd = new HanaCommand("select TOP 1000 * from schemas", Globals.hanaConn);
                        HanaDataReader dr = cmd.ExecuteReader();
                        lock (_schemasLock)
                        {
                            this.Schemas.Clear();
                            while (dr.Read())
                            {
                                Schemas.Add(dr.GetString(0));
                            }
                        }
                        dr.Close();
                    }
                    catch (Exception ex)
                    {
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Error Adding Schemas. " + ex.Message);
                    }
            });
        }
        public async void RefreshTables()
        {
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                using (new WaitCursor())
                {
                try
                {
                    //set schemas for dropdown
                    HanaCommand cmd = new HanaCommand("SELECT table_name FROM sys.tables where schema_name = '" + _currentSchema + "'", Globals.hanaConn);
                    HanaDataReader dr = cmd.ExecuteReader();
                    lock (_tablesLock)
                    {
                        this.Tables.Clear();
                        while (dr.Read())
                        {
                            Tables.Add(dr.GetString(0));
                        }
                    }
                    dr.Close();
                }
                catch (Exception ex)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Error Adding Tables. " + ex.Message);
                }
                }  //end waitcursor
            });
        }
        public async void SetPropertiesForTable()
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
            using (new WaitCursor())
            {
                try
                {
                    //clear stuff out
                    _results = null;
                    _querytext.SelectString = "";
                    _objidCol.SelectString = "";
                    _spatialCol.SelectString = "";
                    //_actrecCount.SelectString = "";
                    _totrecCount.SelectString = "";

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

                    //set initial querytext string
                    _querytext.SelectString = "";
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

                }
                catch (Exception ex)
                {
                    FrameworkApplication.State.Deactivate("condition_state_hasProps");
                    FrameworkApplication.State.Deactivate("condition_state_isconnected");
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Error in SetPropertiesForTable. " + ex.Message);
                }
                }  //end waitcursor
            });

                Mouse.OverrideCursor = null;
        }

        //private void RefreshProperties()
        //{   
        //    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        //    ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
        //    {
        //        try
        //        {
        //            //if dockpane was already open then cleanup.  Can't delete/reinstantiate an instance of an esri dockpane.     
        //            bool dpexists = FrameworkApplication.DockPaneManager.IsDockPaneCreated("SapHanaAddIn_TableViewerPanel");
        //            if (dpexists)
        //            {
        //                TableViewerPanelViewModel vm = FrameworkApplication.DockPaneManager.Find("SapHanaAddIn_TableViewerPanel") as TableViewerPanelViewModel;

        //                if (vm.CurrentTable != null) { vm.CurrentTable = ""; }
        //                if (vm.CurrentSchema != null) { vm.CurrentSchema = ""; }
        //                if (vm.QueryTxt != null) { vm.QueryTxt.SelectString = ""; }
        //                if (vm.ObjidCol != null) { vm.ObjidCol.SelectString = ""; }
        //                if (vm.SpatialCol != null) { vm.SpatialCol.SelectString = ""; }
        //                if (vm.ActRecCount != null) { vm.ActRecCount.SelectString = ""; }
        //                if (vm.TotRecCount != null) { vm.TotRecCount.SelectString = ""; }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            FrameworkApplication.State.Deactivate("condition_state_hasProps");
        //            FrameworkApplication.State.Deactivate("condition_state_isconnected");
        //            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.Message);
        //        }
        //    });
        //    Mouse.OverrideCursor = null;
        //}
        public async void GetActualResultCount()
        {
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                try
                {
                    //get actual count
                    //TotRecCount.SelectString = "";
                }
                catch (Exception ex)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Error Adding Tables. " + ex.Message);
                }
            });
        }

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

        private ObservableCollection<string> _schemas = new ObservableCollection<string>();
        public ObservableCollection<string> Schemas
        {
            get
            {
                return _schemas;
            }
        }

        private ObservableCollection<string> _tables = new ObservableCollection<string>();
        public ObservableCollection<string> Tables
        {
            get
            {
                return _tables;
            }
        }

        private string _currentSchema;
        public string CurrentSchema
        {
            get { return _currentSchema; }
            set
            {
                _currentSchema = value;
                //Thread.Sleep(500);
                RefreshTables();
            }
        }

        private string _currentTable;
        public string CurrentTable
        {
            get { return _currentTable; }
            set
            {
                _currentTable = value;
                SetPropertiesForTable();
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
                if (value.SelectString != "")
                {
                    _canExecute = true;
                }
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
                if (value.SelectString != "")
                {
                    _canExecute = true;
                }
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
                if (value.SelectString != "")
                {
                    _canExecute = true;
                }
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
                //get total record count
                string qtst = "SELECT COUNT(*) FROM  " + _currentSchema + "." + _currentTable;
                HanaCommand cmd2 = new HanaCommand(qtst, Globals.hanaConn);
                HanaDataReader dr2 = null;
                System.Windows.Forms.DataGrid dgResults2 = new System.Windows.Forms.DataGrid();
                dr2 = cmd2.ExecuteReader();
                string rc = "";
                if (dr2 != null)
                {
                    DataTable dt2 = new DataTable();
                    dt2.Load(dr2);
                    if (dr2 != null)
                    {
                        if (dt2.Rows.Count > 0)
                        {
                            rc = dt2.Rows.Count.ToString();
                        }
                    }
                }

                _totrecCount = value;
                if (value.SelectString != "")
                {
                    _canExecute = true;
                }
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
                
                if (value.SelectString != "")
                {
                    _canExecute = true;
                }

                _actrecCount = value;
            }
        }
        #endregion

        private ICommand _executeselect;
        public ICommand ExecuteSelect
        {
            get
            {
                return _executeselect ?? (_executeselect = new CommandHandler(() => MyAction(), _canExecute));
            }
        }
        private bool _canExecute = true;
        public void MyAction()
        {

            if (Globals.hanaConn == null || Globals.hanaConn.State != ConnectionState.Open)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Connect to a database first.", "Not connected");
                return;
            }
            string txtSQLStatement = _querytext.SelectString;
            if (txtSQLStatement.Trim().Length < 1)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Please enter the command text.", "Empty command text");
                return;
            }

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            string qtest = txtSQLStatement;

            //get reccords
            if (_spatialCol.SelectString != "" && _spatialCol.SelectString != "none")
            {
                if (txtSQLStatement.IndexOf(_spatialCol.SelectString) > 0)
                {
                    qtest = txtSQLStatement.Replace(_spatialCol.SelectString, _spatialCol.SelectString + ".st_aswkt()" + " as " + _spatialCol.SelectString);
                }
                else
                {
                    qtest = txtSQLStatement;
                }
            }

            HanaCommand cmd = new HanaCommand(qtest, Globals.hanaConn);
            HanaDataReader dr = null;
            System.Windows.Forms.DataGrid dgResults = new System.Windows.Forms.DataGrid();
            try
            {
                dgResults.DataSource = null;

                dr = cmd.ExecuteReader();
                dgResults.DataSource = null;
                dgResults.Refresh();

                if (dr != null && dr.HasRows)
                {
                    dgResults.DataSource = dr;
                    DataTable dt = new DataTable();
                    dt.Load(dr);
                    _results = dt.DefaultView;
                    if (_results.Count > 0)
                    {
                        _actrecCount.SelectString = _results.Count.ToString();
                    }
                    dr.Close();
                }

                Mouse.OverrideCursor = null;
            }
            catch (Exception ex)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.Message, "MyAction failed");
                if (dr != null)
                {
                    dr.Close();
                }

                Mouse.OverrideCursor = null;
            }
            Mouse.OverrideCursor = null;
        }
    }

    public class btnSqlExplorer : Button
    {
        protected override void OnClick()
        {
            TableViewerPanelViewModel.Show();
        }
    }

#region cboEnv Class
public class cboEnv : ComboBox
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
    protected override async void OnSelectionChange(ComboBoxItem item)
    {
        Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
        {
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                FrameworkApplication.State.Activate("condition_state_hasProps");

                if (cboEnv.cboBox.SelectedItem == null)
                {
                    FrameworkApplication.State.Deactivate("condition_state_hasProps");
                    return;
                }

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

                FrameworkApplication.State.Activate("condition_state_isconnected");
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

                ////cleanup if dockpane was already open.  Can't delete/reinstantiate an instance of an esri dockpane.
                ////var panes = ProApp.Panes.FindLayoutPanes(findLyt);
                ////foreach (Pane pane in panes)
                ////{
                ////    ProApp.Panes.CloseLayoutPanes(findLyt.URI);  //Close the pane
                ////}
                bool dpexists = FrameworkApplication.DockPaneManager.IsDockPaneCreated("SapHanaAddIn_TableViewerPanel");
                if (dpexists)
                {
                    TableViewerPanelViewModel vm = FrameworkApplication.DockPaneManager.Find("SapHanaAddIn_TableViewerPanel") as TableViewerPanelViewModel;
                    vm.RefreshSchemas();
                }
                //}
                //else
                //{
                //    //need to pass schemalist to global to hold until the dp is instatintiated  dp.Show
                //    Globals.GSchemas = oc;
                //}
                //Mouse.OverrideCursor = null;
            }
            catch (Exception ex)
            {
                FrameworkApplication.State.Deactivate("condition_state_hasProps");
                FrameworkApplication.State.Deactivate("condition_state_isconnected");

                IPlugInWrapper wrapper = FrameworkApplication.GetPlugInWrapper("lblHasConn");
                if (wrapper != null)
                {
                    wrapper.Caption = "Not Connected";
                }
                IPlugInWrapper wrapper2 = FrameworkApplication.GetPlugInWrapper("btnConnect");
                if (wrapper2 != null)
                {
                    wrapper2.Caption = "Not Connected";
                }
                IPlugInWrapper wrapper3 = FrameworkApplication.GetPlugInWrapper("lblHasConnTrue");
                if (wrapper3 != null)
                {
                    wrapper3.Caption = "Not Connected";
                    wrapper3.Checked = false;
                }
                Mouse.OverrideCursor = null;
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Error in OnSelectionChanged. " + ex.Message);
            }

        });

    }
    protected override async void OnDropDownOpened()
    {
        await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
        {
            try
            {
                base.OnDropDownOpened();

                Clear();
                foreach (ConnectionItem item in HanaConfigModule.Current.ConnectionItems)
                {
                    this.Add(new ComboBoxItem(item.name, item));
                }
                if (this.ItemCollection.Count == 0)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("There were no HANA connections found.  Please add connections by going to Pro's backstage, then Options, then Hana Properties.");

                }
            }
            catch (Exception ex)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Error in OnDropDownOpened. " + ex.Message);
            }
        });
        Mouse.OverrideCursor = null;
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

