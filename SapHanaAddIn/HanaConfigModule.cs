using System;
using System.Collections.Generic;
using System.Text;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core.Events;
using System.Security.Cryptography;
using System.IO;
using System.Security;
using ArcGIS.Desktop.Core;
using ArcGIS.Core.Events;

namespace SapHanaAddIn
{
    internal class HanaConfigModule : Module
    {
        private static HanaConfigModule _this = null;

        private HanaConfigModule()
        {
            ProjectOpenedEvent.Subscribe(OnProjectOpen);
            ProjectClosedEvent.Subscribe(OnProjectClose);

        }
        private void OnProjectClose(ProjectEventArgs obj)
        {
            hasSettings = false;
        }

        private void OnProjectOpen(ProjectEventArgs obj)
        {
            if (!hasSettings)
            {
                _moduleSettings.Clear();
            }
            FrameworkApplication.State.Deactivate("condition_state_isconnected");
            FrameworkApplication.State.Deactivate("condition_state_hasResult");

            //get dsn settings from the local odbc settings and use those for the connectionitems
            //List<string> enumdsn = EnumDsn(); 
            //ListODBCsources();
        }

        public static HanaConfigModule Current
        {
            get
            {
                return _this ?? (_this = (HanaConfigModule)FrameworkApplication.FindModule("SapHanaAddIn_Config"));
            }
        }

        private Dictionary<string, string> _moduleSettings = new Dictionary<string, string>();
        internal Dictionary<string, string> Settings
        {
            get { return _moduleSettings; }
            set { _moduleSettings = value; }
        }

        #region Read dns settings
        //private List<string> EnumDsn()
        //{
        //    List<string> list = new List<string>();
        //    list.AddRange(EnumDsn(Registry.CurrentUser));
        //    list.AddRange(EnumDsn(Registry.LocalMachine));
        //    return list;
        //}

        //public static class OdbcWrapper
        //{
        //    [DllImport("odbc32.dll")]
        //    public static extern int SQLDataSources(int EnvHandle, int Direction, StringBuilder ServerName, int ServerNameBufferLenIn,
        //ref int ServerNameBufferLenOut, StringBuilder Driver, int DriverBufferLenIn, ref int DriverBufferLenOut);

        //    [DllImport("odbc32.dll")]
        //    public static extern int SQLAllocEnv(ref int EnvHandle);
        //}

        //public void ListODBCsources()
        //    {
        //        int envHandle = 0;
        //        const int SQL_FETCH_NEXT = 1;
        //        const int SQL_FETCH_FIRST_SYSTEM = 32;

        //        if (OdbcWrapper.SQLAllocEnv(ref envHandle) != -1)
        //        {
        //            int ret;
        //            StringBuilder serverName = new StringBuilder(1024);
        //            StringBuilder driverName = new StringBuilder(1024);
        //            StringBuilder serverNode = new StringBuilder(1024);
        //            int snLen = 0;
        //            int driverLen = 0;
        //            int servernodeLen = 0;
        ////ret = OdbcWrapper.SQLDataSources(envHandle, SQL_FETCH_FIRST_SYSTEM,
        //            serverName, serverName.Capacity, ref snLen,
        //            driverName, driverName.Capacity, ref driverLen,
        //            serverNode, serverNode.Capacity, ref servernodeLen);
        //while (ret == 0)
        //{
        //    if (driverName.ToString() == "HDBODBC")
        //    {
        //        System.Windows.Forms.MessageBox.Show(serverName + System.Environment.NewLine + driverName);
        //        ret = OdbcWrapper.SQLDataSources(envHandle, SQL_FETCH_NEXT, serverName, serverName.Capacity, ref snLen,
        //                driverName, driverName.Capacity, ref driverLen);
        //    }
        //}
        //    }

        //}

        //private IEnumerable<string> EnumDsn(RegistryKey rootKey)
        //{
        //    RegistryKey regKey = rootKey.OpenSubKey(@"Software\ODBC\ODBC.INI\ODBC Data Sources");
        //    if (regKey != null)
        //    {
        //        foreach (string name in regKey.GetValueNames())
        //        {
        //            //string nm = regKey.GetValue(name, "").ToString();
        //            //RegistryKey regKey = rootKey.OpenSubKey("@" + nm);
        //            //yield return name;
        //        }
        //    }
        //    else
        //    {
        //        //Msgbox("There are no Data Service Names Setup on this machine. You must first setup valid connections to HANA in Microsoft ODBC Administrator. ");
        //    }

        //}
        #endregion
        #region Connection properties
        private SecureString _Hpass = new SecureString();
        internal SecureString Hpass
        {
            get { return _Hpass; }
            set { _Hpass = value; }
        }

        private Dictionary<string, string> _environmrnts = new Dictionary<string, string>();
        internal Dictionary<string, string> environmrnts
        {
            get { return _environmrnts; }
            set { _environmrnts = value; }
        }

        private List<ConnectionItem> _ConnectionItems = new List<ConnectionItem>() ;
        internal List<ConnectionItem > ConnectionItems
        {
            get { return _ConnectionItems; }
            set { _ConnectionItems = value; }
        }

        private List<string> _hanatables = new List<string>();
        internal List<string> HanaTables
        {
            get { return _hanatables; }
            set { _hanatables = value; }
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }
        //     Panes have two portions: a view-model class that must derive from thePane abstract
        //     class and a view class that must be a FrameworkElement (typically a custom UserControl).
        //     The two components are either associated with each other through DAML or manually
        //     via OnCreateContent. If necessary, the Content property provides the view-model
        //     with access to the view.
        protected override void OnPaneOpened(Pane pane)
        {
            if (pane.ContentID == "SapHanaAddIn_TableViewerPanel")
            { }
        }

        private bool hasSettings = false;
        protected override Task OnReadSettingsAsync(ModuleSettingsReader settings)
        {

            // set the flag
            hasSettings = true;
            // clear existing setting values
            _moduleSettings.Clear();

            if (settings == null) return Task.FromResult(0);

            // Settings defined in the Property sheet’s viewmodel.	
            string[] keys = new string[] { "Setting1", "Setting2", "Setting3" };

            foreach (string key in keys)
            {
                object value = settings.Get(key);
                if (value != null)
                {
                    if (_moduleSettings.ContainsKey(key))
                        _moduleSettings[key] = value.ToString();
                    else
                        _moduleSettings.Add(key, value.ToString());
                }
            }
            if (_moduleSettings["Setting3"] != "")
            {
                string len;
                _moduleSettings.TryGetValue("Setting3", out len);
                string contentPath = System.IO.Path.Combine(Project.Current.HomeFolderPath, "Data.dat");
                string projectPath = Project.Current.HomeFolderPath;
                using (FileStream fStream = new FileStream(contentPath, FileMode.Open))
                {
                    if (fStream.CanRead)
                    {
                        byte[] inBuffer = new byte[int.Parse(len)];
                        byte[] outBuffer;
                        string pass = "";
                        fStream.Read(inBuffer, 0, int.Parse(len));
                        byte[] entropy = new byte[20];
                        outBuffer = ProtectedData.Unprotect(inBuffer, null, DataProtectionScope.CurrentUser);
                        pass = UnicodeEncoding.ASCII.GetString(outBuffer);

                        string[] conns = pass.TrimStart(';').Split(';');
                        foreach (string con in conns)
                        {
                            ConnectionItem coni = new SapHanaAddIn.ConnectionItem();
                            string[] cells = con.Split(',');
                            for (int i = 0; i < cells.Length; i++)
                            {
                                switch (i)
                                {
                                    case 0:
                                        coni.name = cells[i];
                                        break;
                                    case 1:
                                        coni.server= cells[i];
                                        break;
                                    case 2:
                                        coni.userid= cells[i];
                                        break;
                                    case 3:
                                        SecureString ss = new SecureString();
                                        foreach (char cc  in cells[i])
                                        {
                                            ss.AppendChar(cc);
                                        }
                                        coni.pass = ss;
                                        break;
                                    default:

                                        break;
                                }
                            }
                            ConnectionItems.Add(coni);

                        }
                        foreach (char cc in pass)
                        {
                            _Hpass.AppendChar(cc);
                        }
                        
                    }
                }
            }


            return Task.FromResult(0);
        }
       
        protected override Task   OnWriteSettingsAsync(ModuleSettingsWriter settings)
        {
            if (ConnectionItems.Count > 0)
            {
                try
                {
                    string fullString = ""; //= String.Join<ConnectionItem>(String.Empty, ConnectionItems.ToArray());
                    foreach (ConnectionItem item in ConnectionItems)
                    {
                        string tst2 = new System.Net.NetworkCredential(string.Empty, item.pass).Password;
                        string[] ss = { item.name, item.server, item.userid, tst2 };
                        fullString = fullString + ";" + string.Join(",", ss);
                    }

                    string tst = new System.Net.NetworkCredential(string.Empty, _Hpass).Password;
                    byte[] plaintxt = UnicodeEncoding.ASCII.GetBytes(fullString);
                    //string password = new System.Net.NetworkCredential(string.Empty, _Hpass).Password;
                    byte[] entropy = new byte[20];
                    using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                    {
                        rng.GetBytes(entropy);
                    }
                    string contentPath = System.IO.Path.Combine(Project.Current.HomeFolderPath, "Data.dat");
                    byte[] ciphertext = System.Security.Cryptography.ProtectedData.Protect(plaintxt, null, DataProtectionScope.CurrentUser);
                    FileStream fStream = new FileStream(contentPath, FileMode.OpenOrCreate);
                    string encryt = UnicodeEncoding.ASCII.GetString(ciphertext, 0, ciphertext.Length);
                    // Write the encrypted data to a stream.
                    if (fStream.CanWrite && ciphertext != null)
                    {
                        fStream.Write(ciphertext, 0, ciphertext.Length);
                        settings.Add("Setting3", ciphertext.Length.ToString());
                        //if (_moduleSettings.ContainsKey("Setting3"))
                        //{
                        //    _moduleSettings.tr
                        //} 
                        //foreach (string key in _moduleSettings.Keys)
                        //{
                        //    if (key == "Setting3")
                        //    {
                        //        settings.Add(key, ciphertext.Length.ToString());
                        //    }
                        //}

                        //length = ciphertext.Length;
                        fStream.Close();
                    }
                    //settings.Add(key, UnicodeEncoding.ASCII.GetString(ciphertext,0,ciphertext.Length));

                    
                }
                catch (Exception sss)
                {
                    string ss = sss.Message;
                    throw;
                }
            }
            foreach (string key in _moduleSettings.Keys)
            {
                if (key=="Setting3")
                {
                    //try
                    //{
                    //    string fullString = ""; //= String.Join<ConnectionItem>(String.Empty, ConnectionItems.ToArray());
                    //    foreach (ConnectionItem item in ConnectionItems)
                    //    {
                    //        string tst2 = new System.Net.NetworkCredential(string.Empty, item.pass).Password;
                    //        string[] ss = { item.name,item.server,item.userid, tst2 };
                    //        fullString = fullString + ";" + string.Join(",",ss);
                    //    }

                    //    string tst = new System.Net.NetworkCredential(string.Empty, _Hpass).Password;
                    //    byte[] plaintxt = UnicodeEncoding.ASCII.GetBytes(fullString);
                    //    //string password = new System.Net.NetworkCredential(string.Empty, _Hpass).Password;
                    //    byte[] entropy = new byte[20];
                    //    using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                    //    {
                    //        rng.GetBytes(entropy);
                    //    }
                    //    string contentPath = System.IO.Path.Combine(Project.Current.HomeFolderPath, "Data.dat");
                    //    byte[] ciphertext = System.Security.Cryptography.ProtectedData.Protect(plaintxt, null, DataProtectionScope.CurrentUser);
                    //    FileStream fStream = new FileStream(contentPath, FileMode.OpenOrCreate);
                    //    string encryt = UnicodeEncoding.ASCII.GetString(ciphertext, 0, ciphertext.Length);
                    //    // Write the encrypted data to a stream.
                    //    if (fStream.CanWrite && ciphertext != null)
                    //    {
                    //        fStream.Write(ciphertext, 0, ciphertext.Length);
                    //        settings.Add(key, ciphertext.Length.ToString());
                    //        //length = ciphertext.Length;
                    //        fStream.Close();
                    //    }
                    //    //settings.Add(key, UnicodeEncoding.ASCII.GetString(ciphertext,0,ciphertext.Length));

                    //    settings.Add(key, ciphertext.Length.ToString());
                    //}
                    //catch (Exception sss)
                    //{
                    //    string ss =sss.Message;
                    //    throw;
                    //}
                    
                }
                else
                {
                    settings.Add(key, _moduleSettings[key]);
                }
                
            }
            return Task.FromResult(0);

        }

        #endregion OverridesDictionary<string, string> _environmrnts
        public static string AddinAssemblyLocation()
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            return System.IO.Path.GetDirectoryName(
                              Uri.UnescapeDataString(
                                      new Uri(asm.CodeBase).LocalPath));
        }


   }
}
