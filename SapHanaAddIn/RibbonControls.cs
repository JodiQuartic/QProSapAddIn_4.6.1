
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Sap.Data.Hana;
using ArcGIS.Core.Geometry;
using System.Security;
using System.Windows.Input;

namespace SapHanaAddIn
{
    public class cboEnv : ComboBox
    {
        public static Dictionary<string, string> conProps { get { return lstEnvNames; } set { lstEnvNames = value; } }
        private static Dictionary<string, string> lstEnvNames = new Dictionary<string, string>();
        
        private static cboEnv comboBox;
        public static cboEnv cboBox
        {
            get
            { return comboBox; }
            set
            {   //action here 
                //update schemalist

                comboBox = value;
            }
        }
        public cboEnv()
        {
            //setEnvs(this);
            cboBox = this;

        }
        protected override void OnDropDownOpened()
        {
            FrameworkApplication.State.Deactivate("condition_state_notReady");

           
            setEnvs(this);
            base.OnDropDownOpened();
        }

        public static async void setEnvs(cboEnv cmb)
        {
            await QueuedTask.Run(() =>
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                cmb.Clear();
                foreach (ConnectionItem item in HanaConfigModule.Current.ConnectionItems)
                {
                    cmb.Add(new ComboBoxItem(item.name, item));
                }
                Mouse.OverrideCursor = null;

            });
        }
        public static string AddinAssemblyLocation()
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            return System.IO.Path.GetDirectoryName(
                              Uri.UnescapeDataString(
                                      new Uri(asm.CodeBase).LocalPath));
        }
        protected override async void OnSelectionChange(ComboBoxItem item)
        {
            await QueuedTask.Run(() =>
            {
                try
                {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    FrameworkApplication.State.Activate("condition_state_hasProps");

                    if (cboEnv.cboBox.SelectedItem == null)
                    {
                        FrameworkApplication.State.Deactivate("condition_state_hasProps");
                        return;
                    }

                    //recreate connection each time cboenv changes 
                    if (Globals.hanaConn == null)
                    {
                        Globals.hanaConn = new HanaConnection();
                    }
                    else
                    {
                        Globals.hanaConn.Close();
                        Globals.hanaConn = null;
                    }

                    if (Globals.isHanaConn == null) { Globals.isHanaConn = new bool(); }
                    else { Globals.isHanaConn = null; }

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
                    Globals.hanaConn.Open();

                    FrameworkApplication.State.Activate("condition_state_isconnected");
                    IPlugInWrapper wrapper = FrameworkApplication.GetPlugInWrapper("lblHasConn");

                    if (wrapper != null)
                    {
                        wrapper.Caption = "Connected";
                    }
                    IPlugInWrapper wrapper2 = FrameworkApplication.GetPlugInWrapper("btnConnect");
                    if (wrapper2 != null)
                    {
                        wrapper2.Caption = "Connected to " + item.Text;
                    }
                    IPlugInWrapper wrapper3 = FrameworkApplication.GetPlugInWrapper("lblHasConnTrue");
                    if (wrapper3 != null)
                    {
                        wrapper3.Caption = "Connected to " + item.Text;
                        wrapper3.Checked = false;
                    }

                    //
                    ////clear schemas (it is bound)
                    //when a schema is selected, init the table cbo values
                    //TableViewerPanelViewModel vm = FrameworkApplication.DockPaneManager.Find("SapHanaAddIn_TableViewerPanel") as TableViewerPanelViewModel;
                    //IPlugInWrapper wrapper = FrameworkApplication.GetPlugInWrapper(vm);

                    //if (vm != null)
                    //{
                    //    HanaCommand cmd = new HanaCommand("select * from schemas", Globals.hanaConn);
                    //    HanaDataReader dr = cmd.ExecuteReader();
                    //    vm.Schemas.Clear();
                    //    while (dr.Read())
                    //    {
                    //        vm.Schemas.Add(dr.GetString(0));
                    //    }
                    //    dr.Close();
                    //}

                }
                catch (Exception ex)
                {
                    FrameworkApplication.State.Deactivate("condition_state_hasProps");
                    FrameworkApplication.State.Deactivate("condition_state_isconnected");

                    IPlugInWrapper wrapper = FrameworkApplication.GetPlugInWrapper("lblHasConn");
                    if (wrapper != null)
                    {
                        wrapper.Caption = "Not Connected";
                    }
                    IPlugInWrapper wrapper2 = FrameworkApplication.GetPlugInWrapper("btnConnect");
                    if (wrapper2 != null)
                    {
                        wrapper2.Caption = "Not Connected" ;
                    }
                    IPlugInWrapper wrapper3 = FrameworkApplication.GetPlugInWrapper("lblHasConnTrue");
                    if (wrapper3 != null)
                    {
                        wrapper3.Caption = "Not Connected";
                        wrapper3.Checked = false;
                    }

                    TableViewerPanelViewModel vm = FrameworkApplication.DockPaneManager.Find("SapHanaAddIn_TableViewerPanel") as TableViewerPanelViewModel;
                    if (vm != null)
                    {
                        vm.Schemas.Clear();
                    }

                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Database or login was not valid. " + ex.Message);

                    Mouse.OverrideCursor = null;
                
            }
            Mouse.OverrideCursor = null;

            });
            Mouse.OverrideCursor = null;
        }
    }
    public class btnSqlExplorer : Button
        {
            protected override void OnClick()
            {
                TableViewerPanelViewModel.Show();
            }
        }
       
        class toolSAPIdentify : MapTool
        {
            public toolSAPIdentify()
            {
                IsSketchTool = true;
                SketchType = SketchGeometryType.Rectangle;

                //To perform a interactive selection or identify in 3D or 2D, sketch must be created in screen coordinates.
                SketchOutputMode = SketchOutputMode.Screen;
            }
            protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
            {
                return QueuedTask.Run(() =>
                {
                    var mapView = MapView.Active;
                    if (mapView == null)
                        return true;

                    //Get all the features that intersect the sketch geometry and flash them in the view. 
                    var results = mapView.GetFeatures(geometry);
                    foreach (var item in results)
                    {
                        FeatureLayer pfl = (FeatureLayer)item.Key;
                        QueryFilter pQF = new QueryFilter();
                        pQF.ObjectIDs = item.Value;
                        RowCursor pCur = pfl.Search(pQF);
                        while (pCur.MoveNext())
                        {
                            Row prow = pCur.Current;
                            Feature pfeat = (Feature)prow;

                            //prow[0];
                            //prow[0] = 's';

                        }

                        //MessageBox.Show(item.Key.ToString());
                    }
                    mapView.FlashFeature(results);

                    //Show a message box reporting each layer the number of the features.
                    MessageBox.Show(String.Join("\n", results.Select(kvp => String.Format("{0}: {1}", kvp.Key.Name, kvp.Value.Count()))), "Identify Result");
                    return true;

                });
            }
            protected override Task HandleMouseDownAsync(MapViewMouseButtonEventArgs e)
            {
                return QueuedTask.Run(() =>
                {
                    var mapPoint = MapView.Active.ClientToMap(e.ClientPoint);
                });
            }


        }
    }