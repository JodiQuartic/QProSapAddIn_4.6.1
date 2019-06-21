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
using System.Security;

namespace SapHanaAddIn
{
    internal class TableViewerPanelViewModel : DockPane, INotifyPropertyChanged
    {
        public const string _dockPaneID = "SapHanaAddIn_TableViewerPanel";
        protected TableViewerPanelViewModel()
        {
            //HTables = new ObservableCollection<HanaTables>();
            try
            {
                //dispose of panel if it exists, so dropdowns all get reset.
                //DockPane pne = FrameworkApplication.DockPaneManager.Find("SapHanaAddIn_TableViewerPanel");
                //if (pne != null)
                // {
                //TableViewerPanelViewModel vm = FrameworkApplication.DockPaneManager.Find("SapHanaAddIn_TableViewerPanel") as TableViewerPanelViewModel;
                //vm = null;
                //pne = null;
                //}

                //HanaPropertiesViewModel ass = new HanaPropertiesViewModel();
                //HanaPropertiesView dd = new HanaPropertiesView();
                //string fff = ass.ModuleSetting2;
                //HanaConfigModule sds = HanaConfigModule.Current;
                //ComboBoxItem itm = (ComboBoxItem)cboEnv.cboBox.SelectedItem;
                //string sss = itm.Text;
                //ConnectionItem connitem = itm.Icon as ConnectionItem;
                //Globals.hanaConn.ConnectionString = "Server=" + connitem.server;
                //Globals.isHanaConn = true;
                //SecureString ss = new SecureString();
                //ss = connitem.pass;
                //ss.MakeReadOnly();
                //HanaCredential hcr = new HanaCredential(connitem.userid, ss);
                //Globals.hanaConn.Credential = hcr;
                //Globals.hanaConn.Open();

                //set schemas for dropdown
                HanaCommand cmd = new HanaCommand("select TOP 2000 * from schemas", Globals.hanaConn);
                HanaDataReader dr = cmd.ExecuteReader();
                this._schemas.Clear();
                while (dr.Read())
                {
                    _schemas.Add(dr.GetString(0));
                }
                dr.Close();

            }
            catch (Exception ex)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.Message, "Failed to initialize table drop down.");
            }

        }
        protected override Task InitializeAsync()
        {
            return base.InitializeAsync();
        }

        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }
        protected override void OnHidden()
        {
            //clear stuff out
            //_tables.Clear();
            //_schemas.Clear();
            //_currenttable = "";
            //_currentSchema = "";
            //_results = null;
            //_querytext.SelectString = "";
            //_objidCol.SelectString = "";
            //_spatialCol.SelectString = "";
        }

        private ObservableCollection<string> _schemas = new ObservableCollection<string>();
        public ObservableCollection<string> Schemas
        {
            get
            {
                return _schemas;
            }
            set
            {
                //HanaCommand cmd = new HanaCommand("select * from schemas", Globals.hanaConn);
                //HanaDataReader dr = cmd.ExecuteReader();
                //this._schemas.Clear();
                //while (dr.Read())
                //{
                //    _schemas.Add(dr.GetString(0));
                //}
                //dr.Close();
                //Task rs = retrieveSchemas();
                _schemas = value;
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
                ////_tables = value;
                //HanaCommand cmd = new HanaCommand("SELECT schema_name,table_name FROM sys.tables where schema_name = '" + value + "'", Globals.hanaConn);
                //HanaDataReader dr = cmd.ExecuteReader();
                ////comboBoxTables.Items.Clear();
                //_tables.Clear();
                //while (dr.Read())
                //{
                //    _tables.Add(dr.GetString(1));
                //}
                //dr.Close();
                _tables = value;
            }
        }
        private string _currentSchema;
        public string CurrentSchema
        {
            get { return _currentSchema; }
            set
            {
                _currentSchema = value;
                RaisePropertyChanged(CurrentSchema);
            }
        }

        private void RaisePropertyChanged(string currentSchema)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            
            //clear stuff out
            _tables.Clear();
            _currenttable = "";
            _results = null;
            _querytext.SelectString = "";
            _objidCol.SelectString = "";
            _spatialCol.SelectString = "";
            //_actrecCount.SelectString = "";
            _totrecCount.SelectString = "";


            //set table list
            HanaCommand cmd = new HanaCommand("SELECT TOP 2000 table_name FROM sys.tables where schema_name = '" + currentSchema + "'", Globals.hanaConn);
            HanaDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                _tables.Add(dr.GetString(0));
            }
            dr.Close();
            Mouse.OverrideCursor = null;
        }

        private string _currenttable;
        public string CurrentTable
        {
            get { return _currenttable; }
            set
            {
                //clear stuff out
                _results = null;
                _querytext.SelectString = "";
                _objidCol.SelectString = "";
                _spatialCol.SelectString = "";
                //_actrecCount.SelectString = "";
                _totrecCount.SelectString = "";

                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                HanaCommand cmd = new HanaCommand("select COLUMN_NAME from SYS.TABLE_COLUMNS where schema_name like '" + _currentSchema + "' and table_name = '" + value + "'", Globals.hanaConn);
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
                _querytext.SelectString = "SELECT TOP 2000 " + string.Join(", ", colls.ToArray()) + " FROM \"" + _currentSchema + "\".\"" + value + "\"";

                //find spatial column
                SpatialCol.SelectString = "";
                cmd.CommandText = "select COLUMN_NAME from SYS.TABLE_COLUMNS where schema_name like '" + _currentSchema + "' and table_name = '" + value + "' and data_type_id = 29812";
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        _spatialCol.SelectString = dr.GetString(0);
                    }
                }
                else
                { _spatialCol.SelectString = "none"; }
                dr.Close();

                //find objectid
                cmd.CommandText = "select COLUMN_NAME from SYS.TABLE_COLUMNS where schema_name like '" + _currentSchema + "' and table_name = '" + value + "' and COLUMN_NAME = 'OBJECTID'";
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        _objidCol.SelectString = dr.GetString(0);
                    }
                }
                else
                { _objidCol.SelectString = "none"; }
                dr.Close();

                //get actual count
                TotRecCount.SelectString = "";
                cmd.CommandText = "SELECT COUNT(*) FROM \"" + _currentSchema + "\".\"" + value + "\"";
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        _totrecCount.SelectString = dr.GetString(0);
                    }
                }
                else
                { _totrecCount.SelectString= ""; }
                dr.Close();

                Mouse.OverrideCursor = null;
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
                string qtst = "SELECT COUNT(*) FROM  " + _currentSchema + "." + _currenttable;
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
                _actrecCount = value;
                if (value.SelectString != "")
                {
                    _canExecute = true;
                }
            }
        }
        private string _tblName;
        public string TblName
        {
            get { return _tblName; }
            set
            {
                _tblName = value;
                //NotifyPropertyChanged("TblName");
            }
        }
        
        private TableViewerPanelViewModel _selectedRow;
        public TableViewerPanelViewModel SelectedRow
        {
            get { return _selectedRow; }
            set { _selectedRow = value; }
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

        private SelectionString _querytext  ;
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
                MessageBox.Show("Connect to a database first.", "Not connected");
                return;
            }
            string txtSQLStatement = _querytext.SelectString;
            if (txtSQLStatement.Trim().Length < 1)
            {
                MessageBox.Show("Please enter the command text.", "Empty command text");
                return;
            }

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            string qtest =txtSQLStatement;

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
                    if ( _results.Count>0)
                    {
                        _actrecCount.SelectString = _results.Count.ToString();
                    }
                    dr.Close();
                }

                Mouse.OverrideCursor = null;
            }
            catch (Exception ex)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.Message,  "MyAction failed");
                if (dr != null)
                {
                    dr.Close();
                }

                Mouse.OverrideCursor = null;
            }
            Mouse.OverrideCursor = null;
        }

    }
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
    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class TableViewerPanel_ShowButton : System.Windows.Controls.Button
    {
        protected override void OnClick()
        {
            TableViewerPanelViewModel.Show();
        }
    }

   
}
