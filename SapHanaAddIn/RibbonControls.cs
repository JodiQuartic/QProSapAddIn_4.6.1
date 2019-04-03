
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows;
using ArcGIS.Desktop.Mapping.Events;
using Sap.Data.Hana;
using System.IO;
using System.Windows.Input;
using ArcGIS.Core.Geometry;
using System.Xml;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Security;

namespace SapHanaAddIn
{
    
    public class cboEnv : ComboBox
    {
        public static Dictionary<string, string> conProps { get { return lstEnvNames; } set { lstEnvNames = value;  } }
        private static Dictionary<string, string> lstEnvNames = new Dictionary<string, string>();
        //public Dictionary<string, string> lstEnvNames = new Dictionary<string, string>();
        private static cboEnv comboBox;
        public static cboEnv cboBox { get { return comboBox; } set { comboBox = value; } }
        public cboEnv()
        {
            setEnvs(this);
            cboBox = this;
            
        }
        protected override void OnDropDownOpened()
        {
            setEnvs(this);
            base.OnDropDownOpened();
        }



        public static async void setEnvs(cboEnv cmb)
        {
            await QueuedTask.Run(() =>
            {

                cmb.Clear();
                foreach (KeyValuePair<string, string> item in HanaConfigModule.Current.environmrnts)
                {
                    cmb.Add(new ComboBoxItem(item.Key.ToString()));
                }
                
                ////string contentPath = System.IO.Path.Combine( AddinAssemblyLocation(), "HANAServers", "HannaConnections.xml");
                //string contentPath;
                //HanaConfigModule.Current.Settings.TryGetValue("Setting1", out contentPath);
                ////var assembly = Assembly.GetExecutingAssembly();
                //////var resourceName = "Quartic.SapHanaAddIn.HannaConnections.xml";
                ////string resourceName2 = assembly.GetManifestResourceNames().Single(str => str.EndsWith("HannaConnections.xml"));
                //////List<string> lstEnvNames = new List<string>();

                //XmlDocument xmlDoc = new XmlDocument();
                ////using (Stream stream = assembly.GetManifestResourceStream(resourceName2))

                //using (StreamReader reader = new StreamReader(contentPath))
                //{
                //    string result = reader.ReadToEnd();

                //    xmlDoc.LoadXml(result);
                //}
                //foreach (XmlNode nose in xmlDoc.DocumentElement.ChildNodes)
                //{

                //    lstEnvNames.Add(nose.ChildNodes[0].InnerText.Trim(),"Server=" + nose.ChildNodes[1].InnerText.Trim() + ";UserID=" + nose.ChildNodes[2].InnerText.Trim() +";Password=");
                //}

                //foreach (KeyValuePair<string,string> item in lstEnvNames)
                //{
                //    cmb.Add(new ComboBoxItem(item.Key.ToString()));
                //}

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


                    //switch (item.Text)
                    //{

                    //    case ("DevGis"):
                    //        Globals.sqlConn.ConnectionString = Globals.gisDevConnStr;
                    //        break;
                    //    case ("DevHana"):
                    //        Globals.hanaConn = new HanaConnection();
                    //        Globals.hanaConn.ConnectionString = Globals.hanaDevConnStr;
                    //        Globals.isHanaConn = new bool();
                    //        Globals.isHanaConn = true;
                    //        break;
                    //    case ("QAGis"):
                    //        Globals.sqlConn.ConnectionString = Globals.gisQaConnStr;
                    //        break;
                    //    case ("QAHana"):
                    //        Globals.hanaConn = new HanaConnection();
                    //        Globals.hanaConn.ConnectionString = Globals.hanaQaConnStr;
                    //        Globals.isHanaConn = new bool();
                    //        Globals.isHanaConn = true;
                    //        break;
                    //    case ("ProdGis"):
                    //        //Globals.ConnectionString = Globals.gisProdConnStr;
                    //        break;
                    //    case ("ProdHana"):
                    //        //Globals.hanaConn.ConnectionString = Globals.hanaProdConnStr;
                    //        break;
                    //}
                }
                catch (HanaException ex)
                {
                    MessageBox.Show(ex.Errors[0].Source + " : " + ex.Errors[0].Message + " (" +
                     ex.Errors[0].NativeError.ToString() + ")",
                     "Failed to open Connection" + " " + item.Text);
                }
        });
        }
    }
    public class btnConnect : Button
    {
        protected override void OnClick()
        {

            ConnectToDB();
        }
       private static Task ConnectToDB()
        {
            return QueuedTask.Run(() =>
            {
                try
                {
                    HanaPropertiesViewModel ass = new HanaPropertiesViewModel();
                    HanaPropertiesView dd = new HanaPropertiesView();
                    string fff = ass.ModuleSetting2;
                    HanaConfigModule sds = HanaConfigModule.Current;
                    //sds.
                    if (Globals.isHanaConn == null)
                    {
                        Globals.isHanaConn = new bool();

                    }
                    if (Globals.hanaConn == null)
                    {

                        Globals.hanaConn = new HanaConnection();
                    }
                    if (Globals.hanaConn.State == System.Data.ConnectionState.Open)
                    {
                        Globals.hanaConn.Close();
                    }
                    ComboBoxItem item = (ComboBoxItem)cboEnv.cboBox.SelectedItem;
                    string sss = item.Text;

                    //bool ext = cboEnv.conProps.TryGetValue(item.Text, out sss);
                    bool ext = HanaConfigModule.Current.environmrnts.TryGetValue(item.Text, out sss);
                    HanaConfigModule config = HanaConfigModule.Current;
                    //IDictionary<string, string> sett = config.Settings;
                    //string len;
                    //sett.TryGetValue("Setting3", out len);
                    //byte[] inBuffer = new byte[int.Parse(len)];
                    //byte[] outBuffer;
                    //string pass = "";
                    //using (FileStream fStream = new FileStream("Data.dat", FileMode.Open))
                    //{
                    //    if (fStream.CanRead)
                    //    {
                    //        fStream.Read(inBuffer, 0, int.Parse(len));
                    //        byte[] entropy = new byte[20];
                    //        outBuffer = ProtectedData.Unprotect(inBuffer, null, DataProtectionScope.CurrentUser);
                    //        pass = UnicodeEncoding.ASCII.GetString(outBuffer);
                    //    }
                    //}                        


                    if (ext)
                    {
                        Globals.hanaConn.ConnectionString = sss; //+ pass;
                        Globals.isHanaConn = true;
                        //Globals.hanaConn.ConnectionString = string.Concat(sss, pass);
                        SecureString ss = new SecureString();
                        ss = HanaConfigModule.Current.Hpass;
                        ss.MakeReadOnly();
                        //foreach (char cc in pass)
                        //{
                        //    ss.AppendChar(cc);
                        //}
                        //ss.MakeReadOnly();
                        //Globals.hanaConn.ConnectionString = "Server = sapqe2hana.ad.sannet.gov:30015; UserID = jluostarinen; Password = Sap2018!";
                        //HanaCredential hcr = new HanaCredential("jluostarinen", ss);
                        string user;
                        if (HanaConfigModule.Current.Settings.TryGetValue("Setting2",out user))
                        {
                            HanaCredential hcr = new HanaCredential(user, ss);
                            Globals.hanaConn.Credential = hcr;
                        } 

                    }
                    
                    Globals.hanaConn.Open();

                    //if (Globals.hanaConn == null)
                    //{
                    //    MessageBox.Show("Select an environment first.", "No environment is selected.");
                    //    return;
                    //}

                    //else if ((bool)Globals.isHanaConn)
                    //{

                    //    if (Globals.hanaConn.State == System.Data.ConnectionState.Closed)
                    //    {
                    //        Globals.hanaConn.Open();
                    //    }
                    //    else
                    //    {
                    //        Globals.hanaConn.Close();
                    //        Globals.hanaConn.Open();
                    //    }

                    //cboEnv.cboBox.Caption = "ReConnect";

                    //}
                    //}
                    IPlugInWrapper wrapper = FrameworkApplication.GetPlugInWrapper("lblHasConn");


                    if (wrapper != null)
                    {
                        wrapper.Caption = "Connected";
                    }
                    IPlugInWrapper wrapper2 = FrameworkApplication.GetPlugInWrapper("btnConnect");
                    if (wrapper2 != null)
                    {
                        wrapper2.Caption = "Currently connected to: " + item.Text;
                    }
                    IPlugInWrapper wrapper3 = FrameworkApplication.GetPlugInWrapper("lblHasConnTrue");
                    if (wrapper3 != null)
                    {
                        wrapper3.Caption = "Currently connected to: " + item.Text;
                        wrapper3.Checked = false;

                    }


                    else if (Globals.isHanaConn == false)
                    {

                    }

                }

                 catch (HanaException ex)
                {
                    if (ex.Message== "authentication failed")
                    {
                        MessageBox.Show("authentication failed. Please reset you password in the Hanna Settings of ArcGIS PRo Options ");
                    }
                    else
                    {
                        MessageBox.Show(ex.Errors[0].Source + " : " + ex.Errors[0].Message + " (" +  ex.Errors[0].NativeError.ToString() + ")", "Failed to open Connection");
                    }

                    
                }
            });
        }
    }
    public class btnSqlExplorer : Button
    {
        protected override void OnClick()
        {
            System.Windows.Forms.Form tv = new HANATableViewer.Form1();
            tv.Show();

            //tv.Activate();
        }
    }
    public class btnAddViewToMap : Button
    {
        protected override void OnClick()
        {
            AddHanaViewToMap();
        }

        private static Task AddHanaViewToMap()
        {
            return QueuedTask.Run(() =>
            {
                try
                {
                    //
                    const string SCHEMA = "SAP_GEF";
                    const string GEF_TABLE = "sap.gef.data::gef_geom_3857.point";

                    HanaConnection _conn = new HanaConnection("Server=sapqe1hana:31015;UserID=jluostarinen;Password=Sap2018!");
                    
                    _conn.Open();
                    HanaDataAdapter dataAdapter = new HanaDataAdapter(
                    "SELECT GEF_OBJKEY AS \"Gefobjnr\" FROM  \"" + SCHEMA + "\".\"" + GEF_TABLE + "\"",_conn);
                       
                    //"SELECT t.TEXT AS \"Name\", p.PRODUCTID as \"Product ID\", p.CATEGORY as \"Category\"" +
                    //" FROM \"" + SCHEMA + "\".\"" + PRODUCTS_TABLE + "\" p INNER JOIN \"" + SCHEMA + "\".\"" + TEXT_TABLE + "\" t ON t.TEXTID = p.NAMEID " + "INNER JOIN \"" + SCHEMA + "\".\"" + PARTNER_TABLE + "\" bp ON p.\"SUPPLIERID.PARTNERID\" = bp.PARTNERID", conn);
                    
                    //Create a new DataTable and use your adapter to fill the table.
                    System.Data.DataTable testTable = new System.Data.DataTable();
                    dataAdapter.Fill(testTable);

                    //really we are dealing with an esri querylayer here.
                    //but the hana query layer in Pro has lotsa bugs...  so maybe use a copy? and make a featurelayer

                    //how to cast to esri type?


                    //create a copy of the hana data as a featurelayer
                    //var layer = await QueuedTask.Run(() => MapView.Active.Map.Layers[0].GetDefinition() as CIMFeatureLayer);
                    //var fLayer = await QueuedTask.Run(() => LayerFactory.CreateLayer(polyLayer.FeatureTable.DataConnection, MapView.Active.Map)) as FeatureLayer;
                    //await QueuedTask.Run(() => fLayer.SetName("My Name"));
                    //await QueuedTask.Run(() => fLayer.SetDefinition(polyLayer));


                    var mapView = MapView.Active;
                    var map = mapView.Map;
                    var layer = MapView.Active.Map.Layers[0].GetDefinition() as CIMFeatureLayer;

                    //var fLayer = LayerFactory.CreateLayer(pointLayer.FeatureTable.DataConnection, MapView.Active.Map) as FeatureLayer;
                    //fLayer.SetName("My Name");
                    //fLayer.SetDefinition(pointLayer);


                    //var v = mapView.Map.StandaloneTables.FirstOrDefault(t => t.Name == Globals.AMPEventsViewTableName);
                }

                catch (HanaException ex)
                {
                    MessageBox.Show(ex.Errors[0].Source + " : " + ex.Errors[0].Message + " (" +
                     ex.Errors[0].NativeError.ToString() + ")",
                     "Failed to initialize");
                }


            });
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
            return QueuedTask.Run( () => {
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
