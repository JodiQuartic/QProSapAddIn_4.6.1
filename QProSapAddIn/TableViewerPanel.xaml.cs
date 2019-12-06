using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Core;


namespace QProSapAddIn
{
    /// <summary>
    /// Interaction logic for TableViewerPanelView.xaml
    /// </summary>
    public partial class TableViewerPanel : UserControl
    {
        public string _spatialcolumn = "";
        public string _objidcolumn = "";
        public string _actreccount = "";
        public string _totreccount = "";
        public string _querystring = "";
        public string _currenttable = "";
        public string _tbls = "";

        
        public TableViewerPanel()
        {
            InitializeComponent();

            //  We have declared the view model instance declaratively in the xaml.
            //  Get the reference to it here, so we can use it in the button click event.
            TableViewerPanelViewModel _tableviewerpanelViewModel = (TableViewerPanelViewModel)base.DataContext;

        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                Task r = ExecuteSqlCb();
            });
        }
        private void btnAddTOC_Click(object sender, RoutedEventArgs e)
        {
            QueuedTask.Run(() =>
            {
                Task r = AddToTOCCb();
            });
        }
        private void btnAddMap_Click(object sender, RoutedEventArgs e)
        {
            QueuedTask.Run(() =>
            {
                Task r = AddToMapCb();
            });
        }

        private void cboSchemas_DropDownOpened(object sender, EventArgs e)
        {
            ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                Task r = RefreshSchemasCb();
            });
        }
        private void cboSchemas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                Task r = RefreshTablesCb();
            });
        }
        private void cboTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                Task r = TableSelectedCb();
            });
        }

        public async Task RefreshSchemasCb()
        {
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                bool dpexists = FrameworkApplication.DockPaneManager.IsDockPaneCreated("QProSapAddIn_TableViewerPanel");
                if (dpexists)
                {
                    TableViewerPanelViewModel vm = FrameworkApplication.DockPaneManager.Find("QProSapAddIn_TableViewerPanel") as TableViewerPanelViewModel;
                    Task r = vm.RefreshSchemasCallback();                   
                }

            });
        }

        public async Task RefreshTablesCb()
        {
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                bool dpexists = FrameworkApplication.DockPaneManager.IsDockPaneCreated("QProSapAddIn_TableViewerPanel");
                if (dpexists)
                {
                    TableViewerPanelViewModel vm = FrameworkApplication.DockPaneManager.Find("QProSapAddIn_TableViewerPanel") as TableViewerPanelViewModel;
                    Task r = vm.RefreshTablesCallback();
                
                }
                    

            });
        }

        public async Task TableSelectedCb()
        {
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                bool dpexists = FrameworkApplication.DockPaneManager.IsDockPaneCreated("QProSapAddIn_TableViewerPanel");
                if (dpexists)
                {
                    TableViewerPanelViewModel vm = FrameworkApplication.DockPaneManager.Find("QProSapAddIn_TableViewerPanel") as TableViewerPanelViewModel;
                    Task r = vm.TableSelectedCallback();
                }
            });
        }

        public async Task ExecuteSqlCb()
        {
            try { 

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                bool dpexists = FrameworkApplication.DockPaneManager.IsDockPaneCreated("QProSapAddIn_TableViewerPanel");
                if (dpexists)
                {
                    var cpd = new CancelableProgressorSource();
                    TableViewerPanelViewModel vm = FrameworkApplication.DockPaneManager.Find("QProSapAddIn_TableViewerPanel") as TableViewerPanelViewModel;
                    Task r = vm.ExecuteSqlCallback(cpd);
                }
            });

        }
                    catch (Exception ex)
            { MessageBox.Show("Error  :  " + ex.Message.ToString() + " "); }

}

        public async Task AddToMapCb()
        {
            try
            {

                await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                {
                    bool dpexists = FrameworkApplication.DockPaneManager.IsDockPaneCreated("QProSapAddIn_TableViewerPanel");
                    if (dpexists)
                    {
                        var cpd = new CancelableProgressorSource();
                        TableViewerPanelViewModel vm = FrameworkApplication.DockPaneManager.Find("QProSapAddIn_TableViewerPanel") as TableViewerPanelViewModel;
                        Task r = vm.AddToMapCallback(cpd);
                    }
                });

            }
            catch (Exception ex)
            { MessageBox.Show("Error  :  " + ex.Message.ToString() + " "); }

        }

        public async Task AddToTOCCb()
        {
            try { 

            await QueuedTask.Run(() =>
            {
                bool dpexists = FrameworkApplication.DockPaneManager.IsDockPaneCreated("QProSapAddIn_TableViewerPanel");
                if (dpexists)
                {
                    TableViewerPanelViewModel vm = FrameworkApplication.DockPaneManager.Find("QProSapAddIn_TableViewerPanel") as TableViewerPanelViewModel;

                    //var pd = new ProgressDialog("Adding data to map.", "Canceled", false);
                    //pd.Show();
                    var cpd = new CancelableProgressorSource();
                    Task r = vm.AddToTOCCallback(cpd);
                }
            });

            }
                catch (Exception ex)
                { MessageBox.Show("Error  :  " + ex.Message.ToString() + " "); }
        }

        //private void DgForResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    try
        //    {
        //        var itm = dgForResults.SelectedItem;

        //        TableViewerPanelViewModel vm = FrameworkApplication.DockPaneManager.Find("QProSapAddIn_TableViewerPanel") as TableViewerPanelViewModel;
        //        vm.QueryText = itm as SelectionString;
        //    }
        //    catch { }
        //}

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


