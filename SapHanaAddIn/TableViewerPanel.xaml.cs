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
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Dialogs;


namespace SapHanaAddIn
{
    /// <summary>
    /// Interaction logic for TableViewerPanelView.xaml
    /// </summary>
    public partial class TableViewerPanelView : UserControl
    {
        public string _spatialcolumn = "";
        public string _objidcolumn = "";
        //public int _recCount = 0;
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
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Connect to a database first.", "Not connected");
                return;
            }
            string txtSQLStatement = txtQueryTXT.Text;
            if (txtSQLStatement.Trim().Length < 1)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Please enter the command text.", "Empty command text");
                //txtSQLStatement.SelectAll();
                //txtSQLStatement.Focus();
                return;
            }
            //**
            string qtest = qtest = txtSQLStatement;
            string spatialCol = lblSpatialCol.Content.ToString();
            string objidCol = lblObjidCol.Content.ToString();

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

                    //dg column properties
                    foreach (DataGridColumn col in dgSelectedResults.Columns)
                    {
                        if (col.Header.ToString() == "")
                        { col.Header = col.ToString(); }
                        if (col.ToString().Length > 0)
                        { col.Width = col.ToString().Length + 15; }
                    }

                    //position - don't know why this doesn't work in xaml
                    dgSelectedResults.Height = this.ActualHeight * .7;
                    dgSelectedResults.Width = this.ActualWidth;



                    Mouse.OverrideCursor = null;
                }
            }
            catch (HanaException ex)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.Errors[0].Source + " : " + ex.Errors[0].Message + " (" +
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
            //Globals.hanaConn.Close();
            //Visibility = Visibility.Collapsed;

        }

        private void btnAddTOC_Click(object sender, RoutedEventArgs e)
        {

            if (MapView.Active != null)
            {
                _spatialcolumn = lblSpatialCol.Content.ToString();
                _objidcolumn = lblObjidCol.Content.ToString();
                //_recCount = Convert.ToInt32(lblrecCount.Content);
                _QUERYSTring = txtQueryTXT.Text;
                Task sssss = OpenEnterpriseGeodatabase();
                return;
            }
            else
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Please activate a map");
            }
        }
        public async Task OpenEnterpriseGeodatabase()
        {
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {

                try
                {
                    // Opening a Non-Versioned db instance.
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
                    ArcGIS.Desktop.Framework.Contracts.ComboBoxItem item2 = (ArcGIS.Desktop.Framework.Contracts.ComboBoxItem)cboEnv.cboBox.SelectedItem;

                    ConnectionItem connitem = item2.Icon as ConnectionItem;
                    string tst2 = new System.Net.NetworkCredential(string.Empty, connitem.pass).Password;
                    DatabaseConnectionProperties connectionProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.Hana)
                    {
                        AuthenticationMode = AuthenticationMode.DBMS,

                        // Where testMachine is the machine where the instance is running and testInstance is the name of the db instance.
                        Instance = cboEnv.cboBox.Text, //@"sapqe2hana",

                        // Provided that a login called gdb has been created and corresponding schema has been created with the required permissions.
                        User = connitem.userid,//  "jluostarinen",
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
                        string spatialquery = "";
                        string spatialCol = _spatialcolumn;
                        string objidCol = _objidcolumn;

                        //int recCount = _recCount;
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

                        if (qds.IsSpatialQuery())
                        {
                            //this needs to be key - required
                            qds.SetObjectIDFields(objidCol);

                            ArcGIS.Core.Data.Table pTab = db.OpenTable(qds);
                            var serverConnection = new CIMInternetServerConnection { URL = "Fill in the URL of the WMS service" };
                            var connt = new CIMWMTSServiceConnection
                            {
                                ServerConnection = serverConnection,
                            };

                            FeatureLayer pFL = (FeatureLayer)LayerFactory.Instance.CreateLayer(pTab.GetDataConnection(), MapView.Active.Map, layerName: "NewQueryLayer");

                            //MapView.Active.RedrawAsync(true);
                            pFL.Select(null, SelectionCombinationMethod.New);
                            MapView.Active.ZoomToSelected();
                            pFL.ClearSelection();
                            pFL.QueryExtent();
                        }
                        else
                        {
                            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("This table does not have a spatial field.  It will be added as a stand-alone table");
                            //Table ptab = (Table )LayerFactory.Instance.
                        }
                    }



                }
                catch (HanaException ex)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.Errors[0].Source + " : " + ex.Errors[0].Message + " (" +
                    ex.Errors[0].NativeError.ToString() + ")",
                    "Failed to execute SQL statement");
                }
            });
        }

        private static void ResetDataConnectionFeatureService(Layer dataConnectionLayer, string newConnectionString)
        {
            #region Reset the URL of a feature service layer 
            CIMStandardDataConnection dataConnection = dataConnectionLayer.GetDataConnection() as CIMStandardDataConnection;
            dataConnection.WorkspaceConnectionString = newConnectionString;
            dataConnectionLayer.SetDataConnection(dataConnection);
            #endregion
        }

        public class PercentConverter : System.Windows.Data.IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value == null)
                    return 0;
                var valor = (int)(int.Parse(value.ToString()) * 0.8); //80% of my Window Height
                return valor;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                var valor = (int)(int.Parse(value.ToString()) / 0.8);
                return valor;
            }
        }

        private void MenuItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                {

                //Get the clicked MenuItem
                //var menuItem = (MenuItem)sender;

                //Get the ContextMenu to which the menuItem belongs
                //var contextMenu = (ContextMenu)menuItem.Parent;

                //Find the placementTarget - returns all rows
                //var items = (DataGrid)contextMenu.PlacementTarget;

                //Get the underlying item, that you cast to your object that is bound
                //to the DataGrid (and has subject and state as property) - as long as it is selected first
                //var theRow = (DataGrid)item.SelectedCells[0].Item;

                //find the clicked row
                // explicitly select a row when it is right-clicked
                DataGridRow rowContainer = UIHelpers.TryFindFromPoint<DataGridRow>((UIElement)sender, e.GetPosition(dgSelectedResults));

                if (rowContainer == null)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Row is null");
                    return;
                }
                else
                {
                        //make clicked row the selected row
                        (sender as DataGrid).SelectedIndex = rowContainer.GetIndex();
                        int rowIndex = rowContainer.GetIndex();

                        //get the spatial field column
                        if (_spatialcolumn != "" && _spatialcolumn != null)
                        {
                            string colValue = lblSpatialCol.Content.ToString();
                            System.Windows.Controls.Primitives.DataGridCellsPresenter presenter = GetVisualChild<System.Windows.Controls.Primitives.DataGridCellsPresenter>(rowContainer);

                            // try to get the cell but it may possibly be virtualized
                            //DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(_spatialcolumn);
                            //if (cell == null)
                            //{
                            //    // now try to bring into view and retreive the cell
                            //    dgSelectedResults.ScrollIntoView(rowContainer, dgSelectedResults.Columns[_spatialcolumn]);

                            //    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(_spatialcolumn);
                            //}


                            ////make temporary point event
                            //IDisposable _graphic = null;
                            //CIMPointSymbol _pointSymbol = null;

                            //var mapView = MapView.Active;
                            //if (mapView == null) return;
                            //if (coord.IsUnknown != true)
                            //{
                            //    System.Diagnostics.Debug.WriteLine("Lat: {0}, Long: {1}", coord.Latitude, coord.Longitude);

                            //    MapPoint zoomToPnt = MapPointBuilder.CreateMapPoint(coord.Longitude, coord.Latitude, SpatialReferences.WGS84);
                            //    var geoProject = GeometryEngine.Instance.Project(zoomToPnt, SpatialReferences.WebMercator) as MapPoint;
                            //    var expandSize = 200.0;
                            //    var minPoint = MapPointBuilder.CreateMapPoint(geoProject.X - expandSize, geoProject.Y - expandSize, SpatialReferences.WebMercator);
                            //    var maxPoint = MapPointBuilder.CreateMapPoint(geoProject.X + expandSize, geoProject.Y + expandSize, SpatialReferences.WebMercator);
                            //    Envelope env = EnvelopeBuilder.CreateEnvelope(minPoint, maxPoint);
                            //    mapView.ZoomTo(env, new TimeSpan(0, 0, 3));
                            //    _graphic = MapView.Active.AddOverlay(zoomToPnt, _pointSymbol.MakeSymbolReference());

                            //}
                            //else
                            //{
                            //    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Unknown latitude and longitude.");
                            //}
                        }
                        else
                        { ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("This row does not have a spatial column."); }
                    }
        });
        }
        catch (Exception ee)
        {
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ee.ToString());
        }


    }

        /// /// Common UI related helper methods. 
        /// 
        public static class UIHelpers
        {

            #region find parent

            /// <summary>
            /// Finds a parent of a given item on the visual tree.
            /// </summary>
            /// <typeparam name="T">The type of the queried item.</typeparam>
            /// <param name="child">A direct or indirect child of the
            /// queried item.</param>
            /// <returns>The first parent item that matches the submitted
            /// type parameter. If not matching item can be found, a null
            /// reference is being returned.</returns>
            public static T TryFindParent<T>(DependencyObject child) where T : DependencyObject
            {
                //get parent item
                DependencyObject parentObject = GetParentObject(child);

                //we've reached the end of the tree
                if (parentObject == null) return null;

                //check if the parent matches the type we're looking for
                T parent = parentObject as T;
                if (parent != null)
                {
                    return parent;
                }
                else
                {
                    //use recursion to proceed with next level
                    return TryFindParent<T>(parentObject);
                }
            }


            /// <summary>
            /// This method is an alternative to WPF's
            /// <see cref="VisualTreeHelper.GetParent"/> method, which also
            /// supports content elements. Do note, that for content element,
            /// this method falls back to the logical tree of the element.
            /// </summary>
            /// <param name="child">The item to be processed.</param>
            /// <returns>The submitted item's parent, if available. Otherwise
            /// null.</returns>
            public static DependencyObject GetParentObject(DependencyObject child)
            {
                if (child == null) return null;
                ContentElement contentElement = child as ContentElement;

                if (contentElement != null)
                {
                    DependencyObject parent = ContentOperations.GetParent(contentElement);
                    if (parent != null) return parent;

                    FrameworkContentElement fce = contentElement as FrameworkContentElement;
                    return fce != null ? fce.Parent : null;
                }

                //if it's not a ContentElement, rely on VisualTreeHelper
                return System.Windows.Media.VisualTreeHelper.GetParent(child);
            }

            #endregion

            /// <summary>
            /// Tries to locate a given item within the visual tree,
            /// starting with the dependency object at a given position. 
            /// </summary>
            /// <typeparam name="T">The type of the element to be found
            /// on the visual tree of the element at the given location.</typeparam>
            /// <param name="reference">The main element which is used to perform
            /// hit testing.</param>
            /// <param name="point">The position to be evaluated on the origin.</param>
            public static T TryFindFromPoint<T>(UIElement reference, Point point)
              where T : DependencyObject
            {
                DependencyObject element = reference.InputHitTest(point)
                                             as DependencyObject;
                if (element == null) return null;
                else if (element is T) return (T)element;
                else return TryFindParent<T>(element);
            }
        }

        public static T GetVisualChild<T>(System.Windows.Media.Visual parent) where T : System.Windows.Media.Visual
        {
            T child = default(T);
            int numVisuals = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                System.Windows.Media.Visual v = (System.Windows.Media.Visual)System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

     
    }
}

