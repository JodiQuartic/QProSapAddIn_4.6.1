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

namespace SapHanaAddIn
{
    internal class TableViewerPanelViewModel : DockPane, INotifyPropertyChanged
    {
        public const string _dockPaneID = "SapHanaAddIn_TableViewerPanel";
   
        public ObservableCollection<HanaTables> HTables { get; set; }

        private string spatialCol;
        public string SpatialCol
        {
            get { return spatialCol; }
            set {
                spatialCol = value;
                NotifyPropertyChanged("SpatialCol");
            }
        }
        private string objidCol;
        public string ObjidCol
        {
            get { return objidCol; }
            set
            {
                objidCol = value;
                NotifyPropertyChanged("ObjidCol");
            }
        }

        //private int _reccount;
        //public int recCount
        //{
        //    get { return _reccount; }
        //    set
        //    {
        //        //get record count
        //        string qtst = "SELECT COUNT(*) FROM  " + cboSchema.SelectedItem.ToString() + "." + cboTables.SelectedItem.ToString();
        //        HanaCommand cmd2 = new HanaCommand(qtst, Globals.hanaConn);
        //        HanaDataReader dr2 = null;
        //        System.Windows.Forms.DataGrid dgResults2 = new System.Windows.Forms.DataGrid();
        //        dr2 = cmd2.ExecuteReader();
        //        if (dr2 != null)
        //        {
        //            DataTable dt2 = new DataTable();
        //            dt2.Load(dr2);
        //            if (dr2 != null)
        //            {
        //                if (dt2.Rows.Count > 0)
        //                {
        //                    recCount = dt2.Rows.Count;
        //                }
        //            }
        //        }

        //        _reccount = value;

        //    }
        //}

        private TableViewerPanelViewModel selectedRow;
        public TableViewerPanelViewModel SelectedRow
        {
            get { return selectedRow; }
            set { selectedRow = value; }
        }
        protected TableViewerPanelViewModel()
        {
            HTables = new ObservableCollection<HanaTables>();
            try
            {
                if (Globals.hanaConn == null || Globals.hanaConn.State == System.Data.ConnectionState.Closed)
                {
                    MessageBox.Show("Select an environment first.", "No environment is selected.");
                    return;
                }
                else
                {

                    //HanaCommand cmd = new HanaCommand("SELECT schema_name,table_name FROM sys.tables", Globals.hanaConn);
                    HanaCommand cmd = new HanaCommand("select * from schemas", Globals.hanaConn);

                    HanaDataReader dr = cmd.ExecuteReader();

                    //comboBoxSchemas.Items.Clear();
                    _schemas.Clear();
                    while (dr.Read())
                    {
                        _schemas.Add(dr.GetString(0));
                        //comboBoxSchemas.Items.Add(dr.GetString(0));
                    }
                    dr.Close();
                }
            }
            catch (HanaException ex)
            {
                MessageBox.Show(ex.Errors[0].Source + " : " + ex.Errors[0].Message + " (" +
                 ex.Errors[0].NativeError.ToString() + ")",
                 "Failed to initialize table drop down.");
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

        private ObservableCollection<string> _schemas = new ObservableCollection<string>();
        public ObservableCollection<string> Schemas
        {
            get
            {
                return _schemas;
            }
        }
        private ObservableCollection<string> _tables = new ObservableCollection<string>() ;
        public ObservableCollection<string> Tables
        {
            get
            {
                return _tables;
            }
            set
            {
                _tables = value;
            }
        }
        private string _currentSelected;
        public string CurrentSelected
        {
            get { return _currentSelected; }
            set
            {
                HanaCommand cmd = new HanaCommand("SELECT schema_name,table_name FROM sys.tables where schema_name = '" + value + "'", Globals.hanaConn);
                HanaDataReader dr = cmd.ExecuteReader();
                //comboBoxTables.Items.Clear();
                _tables.Clear();
                while (dr.Read())
                {
                    _tables.Add(dr.GetString(1));
                }
                dr.Close();
                _currentSelected = value;
                //RaisePropertyChanged(CurrentSelected);
            }
        }
        private string _currenttableselected;
        public string CurrentTableSelected
        {
            get { return _currenttableselected; }
            set
            {
                HanaCommand cmd = new HanaCommand("select COLUMN_NAME from SYS.TABLE_COLUMNS where schema_name like '" + _currentSelected + "' and table_name = '" + value + "'", Globals.hanaConn);
                HanaDataReader dr = cmd.ExecuteReader();
                List<string> colls = new List<string>();
     
                while (dr.Read())
                {
                    colls.Add(dr.GetString(0));
 
                }
                dr.Close();
                
                //find if there is a spatial field column
                //_querytext.Clear();
                //_querytext.Add( "SELECT TOP 1000 " + string.Join(", ", colls.ToArray()) + " FROM \"" + _currentSelected + "\".\"" + value + "\"");
                _querytext.SelectString = "";
                _querytext.SelectString = "SELECT TOP 1000 " + string.Join(", ", colls.ToArray()) + " FROM \"" + _currentSelected + "\".\"" + value + "\"";
                SpatialCol = "";
                //recCount = 0;
                cmd.CommandText = "select COLUMN_NAME from SYS.TABLE_COLUMNS where schema_name like '" + _currentSelected + "' and table_name = '" + value + "' and data_type_id = 29812";
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    SpatialCol = dr.GetString(0);
                }
                dr.Close();

                //find if there is a OBJECTID column
                ObjidCol = "";
                //select COLUMN_NAME from SYS.CONSTRAINTS where schema_name like 'JLUOSTARINEN' and table_name = 'TESTCREATEFC'
                cmd.CommandText = "select COLUMN_NAME from SYS.CONSTRAINTS where schema_name = '" + _currentSelected + "' and table_name = '" + value + "' ";
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    ObjidCol = dr.GetString(0);
                }
                dr.Close();
                if (ObjidCol == "")
                {
                    cmd.CommandText = "select COLUMN_NAME from SYS.TABLE_COLUMNS where schema_name = '" + _currentSelected + "' and table_name = '" + value + "' and COLUMN_NAME = 'OBJECTID'";
                    dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        ObjidCol = dr.GetString(0);
                    }
                    dr.Close();
                }
                else if (ObjidCol == "")
                {
                    cmd.CommandText = "select COLUMN_NAME from SYS.TABLE_COLUMNS where schema_name = '" + _currentSelected + "' and table_name = '" + value + "' and COLUMN_NAME = 'OBJNR'";
                    dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        ObjidCol = dr.GetString(0);
                    }
                    dr.Close();
                }
                else if (ObjidCol == "")
                {
                    MessageBox.Show("This table does not have a primary id field set.");
                }



                _currenttableselected = value;
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
                //txtSQLStatement.SelectAll();
                //txtSQLStatement.Focus();
                return;
            }

            string qtest = qtest = txtSQLStatement;
            if (spatialCol != "")
            {
                if (txtSQLStatement.IndexOf(spatialCol) > 0)
                {
                    qtest = txtSQLStatement.Replace(spatialCol, spatialCol + ".st_aswkt()" + " as " + spatialCol);
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

                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                dgResults.DataSource = null;

                //cmd.CommandText = "SELECT " + colls + " FROM \"" + comboBoxSchemas.SelectedItem.ToString() + "\".\"" + comboBoxTables.SelectedItem.ToString() + "\"";
                dr = cmd.ExecuteReader();
                dgResults.DataSource = null;
                dgResults.Refresh();
                if (dr != null)
                {
                    dgResults.DataSource = dr;
                    DataTable dt = new DataTable();
                    dt.Load(dr);
                    _results = dt.DefaultView;

                    FrameworkApplication.State.Activate("condition_state_hasResults");
                    dr.Close();
                }

              

                Mouse.OverrideCursor = null;
            }
            catch (HanaException ex)
            {
                MessageBox.Show(ex.Errors[0].Source + " : " + ex.Errors[0].Message + " (" +
                ex.Errors[0].NativeError.ToString() + ")",
                "Failed to execute SQL statement");
                if (dr != null)
                {
                    dr.Close();
                }

                Mouse.OverrideCursor = null;
            }
            Mouse.OverrideCursor = null;
            //txtSQLStatement.SelectAll();
            //txtSQLStatement.Focus();
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
