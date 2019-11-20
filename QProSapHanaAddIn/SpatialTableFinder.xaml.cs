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
        public SpatialTableFinder()
        {
            InitializeComponent();

            //  We have declared the view model instance declaratively in the xaml.
            //  Get the reference to it here, so we can use it in the button click event.
            SpatialTableFinderViewModel _spatialtablefinderViewModel = (SpatialTableFinderViewModel)base.DataContext;

            ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                Task r = ExecuteSqlCb2();
            });

        }

        public async Task ExecuteSqlCb2()
        {
            try
            {
               await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                {
                    bool dpexists = FrameworkApplication.DockPaneManager.IsDockPaneCreated("QProSapAddIn_SpatialTableFinder");
                    if (dpexists)
                    {
                        SpatialTableFinderViewModel vm = FrameworkApplication.DockPaneManager.Find("QProSapAddIn_SpatialTableFinder") as SpatialTableFinderViewModel;
                        Task r = vm.ExecuteSqlCallback2();
                    }
                });

            }
            catch (Exception ex)
            { MessageBox.Show("Error  :  " + ex.Message.ToString() + " "); }

        }
        private void btnAddTOC2_Click(object sender, RoutedEventArgs e)
        {
            QueuedTask.Run(() =>
            {
                Task r = AddToTOCC2();
            });
        }

        public async Task AddToTOCC2()
        {
            try
            {
                await QueuedTask.Run(() =>
                {
                    bool dpexists = FrameworkApplication.DockPaneManager.IsDockPaneCreated("QProSapAddIn_SpatialTableFinder");
                    if (dpexists)
                    {
                        SpatialTableFinderViewModel vm = FrameworkApplication.DockPaneManager.Find("QProSapAddIn_SpatialTableFinder") as SpatialTableFinderViewModel;

                        //var pd = new ProgressDialog("Adding data to map.", "Canceled", false);
                        //pd.Show();
                        var cpd = new CancelableProgressorSource();

                        Task r = vm.AddToTOCCallback2(cpd);
                    }
                });

            }
            catch (Exception ex)
            { MessageBox.Show("Error  :  " + ex.Message.ToString() + " "); }
        }

        private void DgForResults2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var itm = dgForResults2.SelectedItem;

                DataRowView drv = itm as DataRowView;
                SpatialTableFinderViewModel vm = FrameworkApplication.DockPaneManager.Find("QProSapAddIn_SpatialTableFinder") as SpatialTableFinderViewModel;
                vm.SelRow = drv;


            }
            catch { }


        }
    }
}


