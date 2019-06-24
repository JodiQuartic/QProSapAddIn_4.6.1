using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping;
using Sap.Data.Hana;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArcGIS.Core.Geometry;

namespace SapHanaAddIn
{
    /// <summary>
    /// Interaction logic for TableViewerPanelView.xaml
    /// </summary>
    public partial class TableViewerPanelView : UserControl
    {
        public string _spatialcolumn = "";
        public string _objidcolumn = "";
        public string _actreccount = "";
        public string _totreccount = "";
        public string _querystring = "";
        public string _currenttable = "";
        public string _tbls = "";

        public TableViewerPanelView()
        {
            InitializeComponent();
        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                //{

                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                
                //check for valid connection
                if (Globals.hanaConn == null || Globals.hanaConn.State != ConnectionState.Open)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Connect to a database first.", "Not connected");
                    return;
                }

                //check for sql string
                string txtSQLStatement = txtQueryText.Text;
                if (txtSQLStatement.Trim().Length < 1)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Please enter the command text.", "Empty command text");
                    return;
                }

                if (txtSpatialCol != null)
                { _spatialcolumn = (txtSpatialCol.Text.Remove(0, 15)); }
                if (txtObjidCol.Text != null)
                { _objidcolumn = (txtObjidCol.Text.Remove(0, 16)); }
                if (txtTotRecCount.Text != null)
                { _totreccount = (txtTotRecCount.Text); }
                //if (txtActRecCount.Text != null)
                //{ _actreccount = (txtActRecCount.Text); }
                if (cboTables.Items != null)
                { _tbls = cboTables.Items.ToString(); }
                if (txtQueryText.Text != null)
                { _querystring = txtQueryText.Text; }
                if (cboTables.SelectedValue.ToString() != null)
                { _currenttable = cboTables.SelectedValue.ToString(); }

                string qtest = "";
                if (_spatialcolumn != "none" && _spatialcolumn != "")
                {
                    //if (txtSQLStatement.IndexOf(sc) > 0)
                    //{
                    qtest = txtSQLStatement.Replace(_spatialcolumn, _spatialcolumn + ".st_aswkt()" + " as " + _spatialcolumn);
                }
                else
                {
                    qtest = txtSQLStatement;
                }

                HanaCommand cmd = new HanaCommand(qtest, Globals.hanaConn);
                HanaDataReader dr = null;
                System.Windows.Forms.DataGrid dgResults = new System.Windows.Forms.DataGrid();

                dr = cmd.ExecuteReader();
                dgResults.DataSource = null;
                dgResults.Refresh();

                if (dr != null)
                {
                    dgResults.DataSource = dr;
                    DataTable dt = new DataTable();
                    dt.Load(dr);
                    dgForResults.ItemsSource = dt.DefaultView;
                    
                    //count of actual rows
                    _actreccount= dt.Rows.Count.ToString();
                    //TableViewerPanelViewModel vm1 = FrameworkApplication.DockPaneManager.Find("SapHanaAddIn_TableViewerPanel") as TableViewerPanelViewModel;
                    //((vm1)(this.DataContext)).ActRecCount = _actreccount;

                    dr.Close();

                    if (dgForResults.HasItems)
                    {
                        //dg column properties
                        foreach (DataGridColumn col in dgForResults.Columns)
                        {
                            if (col.Header.ToString() == "")
                            { col.Header = col.ToString(); }
                            if (col.ToString().Length > 0)
                            { col.Width = col.ToString().Length + 15; }
                        }
                         
                    }
                    else
                    {
                        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("This table has no rows.", "Table is empty");
                        return;
                    }
                }
                else
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("This data reader is empty", "Table is empty");
                    return;
                }

               // });
            }
            catch (Exception ex)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.Message, "Failed to execute SQL statement");
                Mouse.OverrideCursor = null;
            }

            Mouse.OverrideCursor = null;
        }

        private void btnAddTOC_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                if (MapView.Active != null)
                {
                    //position - don't know why this doesn't work in xaml
                    //dgForResults.Height = this.ActualHeight * .7;
                    //dgForResults.Width = 500;
                    //dpForGrid.Width = 500;
                    //dpMain.Width = 500;

                    if (txtSpatialCol != null)
                    { _spatialcolumn = (txtSpatialCol.Text.Remove(0, 15)); }
                    if (txtObjidCol.Text != null)
                    { _objidcolumn = (txtObjidCol.Text.Remove(0, 16)); }
                    //if (txtActRecCount.Text != null)
                    //{ _actreccount = (txtActRecCount.Text); }
                    if (txtTotRecCount.Text != null)
                    { _totreccount = (txtTotRecCount.Text); }
                    if (cboTables.Items != null)
                    { _tbls = cboTables.Items.ToString(); }
                    if (txtQueryText.Text != null)
                    { _querystring = txtQueryText.Text; }
                    if (cboTables.SelectedValue.ToString() != null)
                    { _currenttable = cboTables.SelectedValue.ToString(); }

                    Task sssss = OpenEnterpriseGeodatabase();
                    return;
                }
                else
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Please activate a map");
                }

            }
            catch (Exception ex)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.Message);
                Mouse.OverrideCursor = null;
            }
            Mouse.OverrideCursor = null;
        }

        public async Task OpenEnterpriseGeodatabase()
        {
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                try
                {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

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

                    IPlugInWrapper wrapper = FrameworkApplication.GetPlugInWrapper("cboEnv");
                    ArcGIS.Desktop.Framework.Contracts.ComboBoxItem item2 = (ArcGIS.Desktop.Framework.Contracts.ComboBoxItem)cboEnv.cboBox.SelectedItem;
                    ConnectionItem connitem = item2.Icon as ConnectionItem;
                    string tst2 = new System.Net.NetworkCredential(string.Empty, connitem.pass).Password;
                    DatabaseConnectionProperties connectionProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.Hana)
                    {
                        AuthenticationMode = AuthenticationMode.DBMS,

                      
                        Instance = cboEnv.cboBox.Text, //@"sapqe2hana",

                        // Provided that a login called gdb has been created and corresponding schema has been created with the required permissions.
                        User = connitem.userid,// 
                        Password = tst2,
                        Version = "dbo.DEFAULT"
                    };

                    //create a query layer
                    using (Geodatabase geodatabase = new Geodatabase(connectionProperties))
                    {
                        Connector pCon = geodatabase.GetConnector();
                        pCon.ToString();
                    }
                    using (Database db = new Database(connectionProperties))
                    {
                        string thequery = "";
                        string txtSQLStatement = _querystring;

                        //append spatial column
                        if (_spatialcolumn != "" && _spatialcolumn != "none")
                        {
                            thequery = txtSQLStatement.Insert(txtSQLStatement.IndexOf("SELECT") + 16, _spatialcolumn + ", ");
                        }
                        else
                        {
                            thequery = txtSQLStatement;
                        }

                        //create table name for the layer
                        Random r = new Random();
                        int n = r.Next();
                        string s = n.ToString();
                        s = s.Substring(s.Length - 4);
                        s = _currenttable + "_" + s;

                        QueryDescription qds = db.GetQueryDescription(txtSQLStatement, "MySelect");// " select GEF_OBJECTID, GEF_OBJKEY, OBJNR, ERNAM, ERDAT, KTEXT, IDAT2, AENAM, ARTPR, GEF_SHAPE from JLUOSTARINEN.ORDERSWLINE", "MySelect");                                                                                                   

                        // Pro requires a pk
                        // can't assume the field name OBJECTID in HANA is a valid field type to use for objectid according to Esri...
                        string oflds = qds.GetObjectIDFields();
                        string oc = "";
                        if (oflds.Length > 0)
                        {
                            oc = oflds.ToString().Split(',')[0];
                            qds.SetObjectIDFields(oc);
                        }
                        else
                        {
                            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("This table does not have a valid key field in the database for ArcGIS.  An Attempt will be made to create a temporary one.");
                            //construct objectid as it is a required column in Pro
                            //row_number() over(partition by " + tempid + ") as OBJECTID";))
                            //string fakeObjCol = objidCol.Split('-')[1];
                            var lst = qds.GetFields();
                            if (lst != null)
                            {
                                if (lst.Count > 0)
                                {
                                    oc = lst[0].Name;
                                }
                            }

                            string fakeObjSql = "row_number()" + " over(partition by " + oc + ")" + " as OBJID";
                            thequery = txtSQLStatement.Insert(txtSQLStatement.IndexOf("FROM") - 1, ", " + fakeObjSql);

                            qds.SetObjectIDFields(oc);
                        }
                        
                        if (qds.IsSpatialQuery())
                        {
                            try
                            {
                                GeometryType gt = qds.GetShapeType();
                                qds.SetShapeType(gt);

                                ArcGIS.Core.Data.Table pTab = db.OpenTable(qds);
                                var serverConnection = new CIMInternetServerConnection { URL = "Fill in the URL of the WMS service" };
                                var connt = new CIMWMTSServiceConnection
                                {
                                    ServerConnection = serverConnection,
                                };

                                FeatureLayer pFL = (FeatureLayer)LayerFactory.Instance.CreateLayer(pTab.GetDataConnection(), MapView.Active.Map, layerName: s);

                                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Layer Successfully added to the Contents Tab: " + s);
                                //MapView.Active.RedrawAsync(true);
                                pFL.Select(null, SelectionCombinationMethod.New);
                                MapView.Active.ZoomToSelected();

                            }
                            catch (Exception ex)
                            {
                                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.Message,
                          "Failed to add table.  There is a problem with either the Geometry field type or the data within it.");
                                return;
                            }
                        }
                        else
                        {
                            ArcGIS.Core.Data.Table pTab = db.OpenTable(qds);
                            var serverConnection = new CIMInternetServerConnection { URL = "Fill in the URL of the WMS service" };
                            var connt = new CIMWMTSServiceConnection
                            {
                                ServerConnection = serverConnection,
                            };

                            StandaloneTable pFL = (StandaloneTable)StandaloneTableFactory.Instance.CreateStandaloneTable(pTab.GetDataConnection(), MapView.Active.Map, tableName: s);
                            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Table successfully added to the Contents Tab: " + s);
                        }

                    }
                }
                catch (Exception ex)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.Message);
                    Mouse.OverrideCursor = null;

                }
                Mouse.OverrideCursor = null;
            });
        }

        //this is for datagrid right click context menu
        //private void MenuItem_MouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    try
        //    {
        //        ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
        //        {
        //            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        //            //Get the clicked MenuItem
        //            //var menuItem = (MenuItem)sender;

        //            //Get the ContextMenu to which the menuItem belongs
        //            //var contextMenu = (ContextMenu)menuItem.Parent;

        //            //Find the placementTarget - returns all rows
        //            //var items = (DataGrid)contextMenu.PlacementTarget;

        //            //Get the underlying item, that you cast to your object that is bound
        //            //to the DataGrid (and has subject and state as property) - as long as it is selected first
        //            //var theRow = (DataGrid)item.SelectedCells[0].Item;

        //            //find the clicked row
        //            // explicitly select a row when it is right-clicked
        //            DataGridRow rowContainer = UIHelpers.TryFindFromPoint<DataGridRow>((UIElement)sender, e.GetPosition(dgForResults));

        //            if (rowContainer == null)
        //            {
        //                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Row is null");
        //                return;
        //            }
        //            else
        //            {
        //                //make clicked row the selected row
        //                (sender as DataGrid).SelectedIndex = rowContainer.GetIndex();
        //                int rowIndex = rowContainer.GetIndex();

        //                //get the spatial field column
        //                if (_spatialcolumn != "" && _spatialcolumn != null)
        //                {
        //                    string colValue = lblSpatialCol.Content.ToString();
        //                    System.Windows.Controls.Primitives.DataGridCellsPresenter presenter = GetVisualChild<System.Windows.Controls.Primitives.DataGridCellsPresenter>(rowContainer);
        //                }
        //                else
        //                { ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("This row does not have a spatial column."); }
        //            }
        //        });
        //    }
        //    catch (Exception ee)
        //    {
        //        ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ee.ToString());
        //        Mouse.OverrideCursor = null;
        //    }
        //    Mouse.OverrideCursor = null;

        //}




    }
}


