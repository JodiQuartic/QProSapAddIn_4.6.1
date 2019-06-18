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
            HTables = new ObservableCollection<HanaTables>();
            try
            {

                if (Globals.isHanaConn == null) { Globals.isHanaConn = new bool(); }
                if (Globals.hanaConn == null) { Globals.hanaConn = new HanaConnection(); }
                if (Globals.hanaConn != null) { Globals.hanaConn.Close(); }

                //dispose of panel if it exists, so dropdowns all get reset.
                DockPane pne = FrameworkApplication.DockPaneManager.Find("SapHanaAddIn_TableViewerPanel");
                if (pne != null)
                {
                    TableViewerPanelViewModel vm = FrameworkApplication.DockPaneManager.Find("SapHanaAddIn_TableViewerPanel") as TableViewerPanelViewModel;
                    vm = null;
                    pne = null;
                }

                HanaPropertiesViewModel ass = new HanaPropertiesViewModel();
                HanaPropertiesView dd = new HanaPropertiesView();
                string fff = ass.ModuleSetting2;
                HanaConfigModule sds = HanaConfigModule.Current;
                ComboBoxItem itm = (ComboBoxItem)cboEnv.cboBox.SelectedItem;
                string sss = itm.Text;
                ConnectionItem connitem = itm.Icon as ConnectionItem;
                Globals.hanaConn.ConnectionString = "Server=" + connitem.server;
                Globals.isHanaConn = true;
                SecureString ss = new SecureString();
                ss = connitem.pass;
                ss.MakeReadOnly();
                HanaCredential hcr = new HanaCredential(connitem.userid, ss);
                Globals.hanaConn.Credential = hcr;
                Globals.hanaConn.Open(); if (Globals.hanaConn == null)

                HanaCommand cmd = new HanaCommand("select * from schemas", Globals.hanaConn);
                HanaDataReader dr = cmd.ExecuteReader();
                this._schemas.Clear();
                while (dr.Read())
                {
                    _schemas.Add(dr.GetString(0));
                }
                dr.Close();


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
        //public async Task retrieveSchemas()
        //{
        //    await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
        //    {
        //        try
        //        {
        //            //set schemas for dropdown
        //            HanaCommand cmd = new HanaCommand("SELECT schema_name,table_name FROM sys.tables where schema_name = '" + value + "'", Globals.hanaConn);
        //            HanaDataReader dr = cmd.ExecuteReader();
        //            _schemas.Clear();

        //            while (dr.Read())
        //            {
        //                Schemas.Add(dr.GetString(1));
        //            }
        //            dr.Close();
        //            Globals.collSchemas = _schemas;
        //        }
        //        catch (HanaException ex)
        //        {
        //            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.Errors[0].Source + " : " + ex.Errors[0].Message + " (" +
        //             ex.Errors[0].NativeError.ToString() + ")",
        //             "Failed to retrieve schemas from HANA");
        //        }
        //    });
        //}
        //public async Task retrieveTables()
        //{
        //    await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
        //    {
        //        try
        //        {
        //            //set tables for dropdown
        //            HanaCommand cmd = new HanaCommand("SELECT table_name FROM sys.tables where schema_name = " + _currentSelected, Globals.hanaConn);
        //            HanaDataReader dr = cmd.ExecuteReader();
        //            _tables.Clear();

        //            while (dr.Read())
        //            {
        //                Tables.Add(dr.GetString(1));
        //            }
        //            dr.Close();
        //            Globals.collTables = _tables;
        //        }
        //        catch (HanaException ex)
        //        {
        //            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.Errors[0].Source + " : " + ex.Errors[0].Message + " (" +
        //             ex.Errors[0].NativeError.ToString() + ")",
        //             "Failed to retrieve tables from HANA");
        //        }
        //    });
        //}
        public ObservableCollection<HanaTables> HTables { get; set; }
        public ObservableCollection<string> _schemas = new ObservableCollection<string>();
        public ObservableCollection<string> Schemas
        {
            get
            {
                return _schemas;
            }
            set
            {
                HanaCommand cmd = new HanaCommand("select * from schemas", Globals.hanaConn);
                HanaDataReader dr = cmd.ExecuteReader();
                this._schemas.Clear();
                while (dr.Read())
                {
                    _schemas.Add(dr.GetString(0));
                }
                dr.Close();
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

        private string _currenttable;
        public string CurrentTable
        {
            get { return _currenttable; }
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

                //_querytext.Clear();
                //_querytext.Add( "SELECT TOP 1000 " + string.Join(", ", colls.ToArray()) + " FROM \"" + _currentSelected + "\".\"" + value + "\"");
                _querytext.SelectString = "";
                _querytext.SelectString = "SELECT TOP 1000 " + string.Join(", ", colls.ToArray()) + " FROM \"" + _currentSelected + "\".\"" + value + "\"";
                SpatialCol = "";
                cmd.CommandText = "select COLUMN_NAME from SYS.TABLE_COLUMNS where schema_name like '" + _currentSelected + "' and table_name = '" + value + "' and data_type_id = 29812";
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    SpatialCol = dr.GetString(0);
                }

                dr.Close();
                _currenttable = value;
            }
        }
       

        private string spatialCol;
        public string SpatialCol
        {
            get { return spatialCol; }
            set {
                spatialCol = value;
                //NotifyPropertyChanged("SpatialCol");
            }
        }
        private string objidCol;
        public string ObjidCol
        {
            get { return objidCol; }
            set
            {
                objidCol = value;
                //NotifyPropertyChanged("ObjidCol");
            }
        }
        private string tblName;
        public string TblName
        {
            get { return tblName; }
            set
            {
                tblName = value;
                //NotifyPropertyChanged("TblName");
            }
        }
        private Boolean hasRows;
        public Boolean HasRows
        {
            get { return hasRows; }
            set
            {
                hasRows = value;
                //NotifyPropertyChanged("HasRows");
            }
        }
        private Boolean hasSql;
        public Boolean HasSql
        {
            get { return hasSql; }
            set
            {
                hasSql = value;
                //NotifyPropertyChanged("HasSql");
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
                //NotifyPropertyChanged("Results");
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
                    { hasRows = true; }
                       
                    dr.Close();
                }
                else { hasRows = false; }
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
