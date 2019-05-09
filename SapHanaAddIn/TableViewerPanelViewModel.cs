using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Sap.Data.Hana;
using System.Collections.ObjectModel;


namespace SapHanaAddIn
{
    internal class TableViewerPanelViewModel : DockPane 
    {
        private const string _dockPaneID = "SapHanaAddIn_TableViewerPanel";
        public ObservableCollection<HanaTables> HTables { get; set; }
        private string spatialCol;
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

        /// <summary>
        /// Show the DockPane.
        /// </summary>
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
                //_querytext.Clear();
                //_querytext.Add( "SELECT TOP 1000 " + string.Join(", ", colls.ToArray()) + " FROM \"" + _currentSelected + "\".\"" + value + "\"");
                _querytext = "";
                _querytext = "SELECT TOP 1000 " + string.Join(", ", colls.ToArray()) + " FROM \"" + _currentSelected + "\".\"" + value + "\"";
                spatialCol = "";
                cmd.CommandText = "select COLUMN_NAME from SYS.TABLE_COLUMNS where schema_name like '" + _currentSelected + "' and table_name = '" + value + "' and data_type_id = 29812";
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    spatialCol = dr.GetString(0);

                }
                dr.Close();
                _currenttableselected = value;
            }
        }


        private string _querytext  ;
        public string QueryTxt
        { 
            get { return _querytext; }
            set
            {
                _querytext = value;
                
                
            }
        }
        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Search Options";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }


    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class TableViewerPanel_ShowButton : Button
    {
        protected override void OnClick()
        {
            TableViewerPanelViewModel.Show();
        }
    }
}
