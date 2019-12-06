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
    /// Interaction logic for SpatialTableFinder.xaml
    /// </summary>
    public partial class SpatialTableFinder : UserControl
    {
        public string _strMessage = "";
        public string _spatialCol = "";
        public string _objidCol = "";
        public string _querytext = "";
        public string _currentTable = "";
        public string _currentSchema = "";
        


        public SpatialTableFinder()
        {
            InitializeComponent();

            //  We have declared the view model instance declaratively in the xaml.
            //  Get the reference to it here, so we can use it in the button click event.
            SpatialTableFinderViewModel _spatialtablefinderViewModel = (SpatialTableFinderViewModel)base.DataContext;

        }

        public async Task GetSpatTabsCb()
        {
            try
            {
                await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                 {
                     bool dpexists = FrameworkApplication.DockPaneManager.IsDockPaneCreated("QProSapAddIn_SpatialTableFinder");
                     if (dpexists)
                     {
                         SpatialTableFinderViewModel vm = FrameworkApplication.DockPaneManager.Find("QProSapAddIn_SpatialTableFinder") as SpatialTableFinderViewModel;
                         Task r = vm.GetSpatTabsCallback();
                     }
                 });

            }
            catch (Exception ex)
            { MessageBox.Show("Error  :  " + ex.Message.ToString() + " "); }

        }

        public async Task AddToMapCb2()
        {
            try
            {


                await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                {
                    bool dpexists = FrameworkApplication.DockPaneManager.IsDockPaneCreated("QProSapAddIn_SpatialTableFinder");
                    if (dpexists)
                    {
                        var cpd = new CancelableProgressorSource();
                        SpatialTableFinderViewModel vm = FrameworkApplication.DockPaneManager.Find("QProSapAddIn_SpatialTableFinder") as SpatialTableFinderViewModel;
                        Task r = vm.AddToMapCallback2(cpd);
                    }
                });

            }
            catch (Exception ex)
            { MessageBox.Show("Error  :  " + ex.Message.ToString() + " "); }
        }

        private void btnGetSpatTabs_Click(object sender, RoutedEventArgs e)
        {
            QueuedTask.Run(() =>
            {
                Task r = GetSpatTabsCb();
            });
        }
        private void btnAddMap2_Click(object sender, RoutedEventArgs e)
        {
            QueuedTask.Run(() =>
            {
                Task r = AddToMapCb2();
            });
        }

        private void DgForResults2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var itm = dgForResults2.SelectedItem;

                DataRowView drv = itm as DataRowView;
                SpatialTableFinderViewModel vm = FrameworkApplication.DockPaneManager.Find("QProSapAddIn_SpatialTableFinder") as SpatialTableFinderViewModel;
                vm.SelRow = drv;
                vm.CurrentSchema = drv["OWNER"].ToString();
                vm.CurrentTable = drv["TABLE_NAME"].ToString();

            }
            catch { }


        }
    }
}


