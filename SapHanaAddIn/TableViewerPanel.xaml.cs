using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping;
using Microsoft.Win32;
using Sap.Data.Hana;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace SapHanaAddIn
{
    /// <summary>
    /// Interaction logic for TableViewerPanelView.xaml
    /// </summary>
    public partial class TableViewerPanelView : UserControl
    {
        public string _spatialcolumn = "";
        public string _hasspatialcolumn = "";
        public string _QUERYSTring = "";
        public TableViewerPanelView()
        {
            InitializeComponent();
        }

        private void cboSchema_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            

        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            
            if (Globals.hanaConn == null || Globals.hanaConn.State != ConnectionState.Open)
            {
                MessageBox.Show("Connect to a database first.", "Not connected");
                return;
            }
            string txtSQLStatement = txtQueryTXT.Text;
            if (txtSQLStatement.Trim().Length < 1)
            {
                MessageBox.Show("Please enter the command text.", "Empty command text");
                //txtSQLStatement.SelectAll();
                //txtSQLStatement.Focus();
                return;
            }

            string qtest = qtest = txtSQLStatement;
            string spatialCol = lblSpatialCol.Content.ToString();
            string hasspatialCol = lblhasSpatialCol.Content.ToString();
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


                ////cmd.CommandText = "select COLUMN_NAME from TABLE_COLUMNS where schema_name like '" + comboBoxSchemas.SelectedItem.ToString() + "' and table_name = '" + comboBoxTables.SelectedItem.ToString() + "' and data_type_id != 29812";


                ////HanaParameter parm1 = new HanaParameter();
                //cmd.Prepare();


                //string colls = "";
                //char[] charsToTrim = { ',', ' ' };
                //if (dr != null)
                //{
                //    while (dr.Read())
                //    {
                //        colls = colls + dr.GetString(0) + ",";
                //    }

                //}
                //colls = colls.TrimEnd(charsToTrim);
                //dr.Close();

                //cmd.CommandText = "SELECT " + colls + " FROM \"" + comboBoxSchemas.SelectedItem.ToString() + "\".\"" + comboBoxTables.SelectedItem.ToString() + "\"";
                dr = cmd.ExecuteReader();
                dgResults.DataSource = null;
                dgResults.Refresh();
                if (dr != null)
                {
                    dgResults.DataSource = dr;
                    DataTable dt = new DataTable();
                    dt.Load(dr);
                    dgSelectedResults.ItemsSource = dt.DefaultView;
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
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Globals.hanaConn.Close();
            Visibility = Visibility.Collapsed;
        }

        private void btnAddTOC_Click(object sender, RoutedEventArgs e)
        {

            if (MapView.Active != null)
            {
                _spatialcolumn = lblSpatialCol.Content.ToString();
                _hasspatialcolumn = lblhasSpatialCol.Content.ToString();
                _QUERYSTring = txtQueryTXT.Text;
                Task sssss = OpenEnterpriseGeodatabase();
                return;
            }
            else
            {
                MessageBox.Show("Please activate a map");
            }
        }
        public async Task OpenEnterpriseGeodatabase()
        {
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() => {

                try
                { 
                    // Opening a Non-Versioned SQL Server instance.
                    RegistryKey reg = (Registry.LocalMachine).OpenSubKey("Software");
                    reg = reg.OpenSubKey("ODBC");
                    reg = reg.OpenSubKey("ODBC.INI");
                    reg = reg.OpenSubKey("ODBC Data Sources");
                    string instance = "";
                    foreach (string item in reg.GetValueNames())
                    {
                        instance = item;
                        string vvv = reg.GetValue(item).ToString();
                    }

                    IPlugInWrapper wrapper = FrameworkApplication.GetPlugInWrapper("cboEnv");

                    //ArcGIS.Desktop.Framework.Contracts.ComboBox ddd = (ArcGIS.Desktop.Framework.Contracts.ComboBox)wrapper;
                    ArcGIS.Desktop.Framework.Contracts.ComboBoxItem item2 = (ArcGIS.Desktop.Framework.Contracts.ComboBoxItem)cboEnv.cboBox.SelectedItem;
                    // = ddd.Text;
                    ConnectionItem connitem = item2.Icon as ConnectionItem;
                    string tst2 = new System.Net.NetworkCredential(string.Empty, connitem.pass).Password;
                    DatabaseConnectionProperties connectionProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.Hana)
                    {
                        AuthenticationMode = AuthenticationMode.DBMS,

                        // Where testMachine is the machine where the instance is running and testInstance is the name of the SqlServer instance.
                        Instance = cboEnv.cboBox.Text, //@"sapqe2hana",

                        // Provided that a database called LocalGovernment has been created on the testInstance and geodatabase has been enabled on the database.
                        //Database = "LocalGovernment",

                        // Provided that a login called gdb has been created and corresponding schema has been created with the required permissions.
                        User = connitem.userid,//  "jluostarinen",
                        Password = tst2,
                        Version = "dbo.DEFAULT"
                    };

                    using (Geodatabase geodatabase = new Geodatabase(connectionProperties))
                    {
                        Connector pCon = geodatabase.GetConnector();
                        pCon.ToString();
                        // Use the geodatabase
                    }
                    using (Database db = new Database(connectionProperties))
                    {
                        string spatialquery = "";
                        string spatialCol = _spatialcolumn;// lblSpatialCol.Content.ToString();
                        string hasspatialCol = _hasspatialcolumn;
                        string txtSQLStatement = _QUERYSTring;// txtQueryTXT.Text;
                                                              //"Select * from 'JLUOSTARINEN'.'Points'", "MySelect"
                                                              //QueryDescription qds = db.GetQueryDescription("Select * from \"JLUOSTARINEN\".\"TESTCREATEFC\"", "MySelect");
                        if (spatialCol != "")
                        {
                            spatialquery = txtSQLStatement.Insert(txtSQLStatement.IndexOf("SELECT") + 7, spatialCol + ", ");
                        }
                        else
                        {
                            spatialquery = txtSQLStatement;
                        }
                        QueryDescription qds = db.GetQueryDescription(txtSQLStatement, "MySelect");// " select GEF_OBJECTID, GEF_OBJKEY, OBJNR, ERNAM, ERDAT, KTEXT, IDAT2, AENAM, ARTPR, GEF_SHAPE from JLUOSTARINEN.ORDERSWLINE", "MySelect");

                        //dataConnection.WorkspaceConnectionString = workspaceConnectionString;
                        //dataConnection.WorkspaceFactory = WorkspaceFactory.OLEDB;
                        //dataConnection.DatasetType = esriDatasetType.esriDTFeatureClass;
                        //dataConnection.Dataset = "JLUOSTARINEN.ORDERSWLINE";
                        if (qds.IsSpatialQuery())
                        {
                            qds.SetObjectIDFields("OBJECTID");
                            ArcGIS.Core.Data.Table pTab = db.OpenTable(qds);
                            //string workspaceConnectionString = db.GetConnectionString();
                            var serverConnection = new CIMInternetServerConnection { URL = "Fill in the URL of the WMS service" };
                            var connt = new CIMWMTSServiceConnection
                            {
                                ServerConnection = serverConnection,
                            };
                            CIMStandardDataConnection dataConnection = new CIMStandardDataConnection();
                            CIMWMTSServiceConnection sd = new CIMWMTSServiceConnection();

                            FeatureLayer pFL = (FeatureLayer)LayerFactory.Instance.CreateLayer(pTab.GetDataConnection(), MapView.Active.Map, 0);
                            //MapView.Active.RedrawAsync(true);
                            pFL.Select(null, SelectionCombinationMethod.New);
                            MapView.Active.ZoomToSelected();
                            pFL.ClearSelection();
                            pFL.QueryExtent();
                        }
                        else
                        {
                            //Table ptab = (Table )LayerFactory.Instance.
                        }
                    }



                }
                catch (HanaException ex)
                {
                    MessageBox.Show(ex.Errors[0].Source + " : " + ex.Errors[0].Message + " (" +
                    ex.Errors[0].NativeError.ToString() + ")",
                    "Failed to execute SQL statement");
                }




            });
        }
    }
}
