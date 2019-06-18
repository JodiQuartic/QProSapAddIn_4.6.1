
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
        public static cboEnv cboBox { get { return comboBox; } set { comboBox = value; } }
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
                cmb.Clear();
                foreach (ConnectionItem item in HanaConfigModule.Current.ConnectionItems)
                {
                    cmb.Add(new ComboBoxItem(item.name, item));
                }

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

                if (Globals.isHanaConn == null)
                {
                    Globals.isHanaConn = new bool();
                }

                if (Globals.hanaConn == null)
                {
                    Globals.hanaConn = new HanaConnection();
                }
                if (Globals.hanaConn != null)
                {
                    Globals.hanaConn.Close();
                }

                //dispose of panel if it exists, so dropdowns all get reset.
                DockPane pne = FrameworkApplication.DockPaneManager.Find("SapHanaAddIn_TableViewerPanel");
                if (pne != null)
                {
                    TableViewerPanelViewModel vm = FrameworkApplication.DockPaneManager.Find("SapHanaAddIn_TableViewerPanel") as TableViewerPanelViewModel;
                    vm = null;
                    pne = null;
                    }

                //clear schemas (it is bound)
                //if (Globals.collSchemas == null)
                //{
                //    Globals.collSchemas = new System.Collections.ObjectModel.Collection<string>();
                //}

                //Globals.collSchemas.Clear();
                //while (dr.Read())
                //{
                //    Globals.collSchemas.Add(dr.GetString(0));
                //}
                //dr.Close();

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
                    else if (Globals.isHanaConn == false)
                    {

                    }
                }
                catch (HanaException ex)
                {
                    MessageBox.Show(ex.Errors[0].Source + " : " + ex.Errors[0].Message + " (" +
                     ex.Errors[0].NativeError.ToString() + ")",
                     "Failed to open Connection" + " " + item.Text);
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