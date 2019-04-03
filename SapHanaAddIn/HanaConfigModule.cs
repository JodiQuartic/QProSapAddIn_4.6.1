using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core.Events;
using System.Security.Cryptography;
using System.IO;
using System.Security;
using ArcGIS.Desktop.Core;
using System.Xml;

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

        }

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
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
                        foreach (char cc in pass)
                        {
                            _Hpass.AppendChar(cc);
                        }
                        
                    }
                }
            }

            if (_moduleSettings["Setting1"] != "")
            {
                string contentPath;
                _moduleSettings.TryGetValue("Setting1", out contentPath);
                XmlDocument xmlDoc = new XmlDocument();

                using (StreamReader reader = new StreamReader(contentPath))
                {
                    string result = reader.ReadToEnd();

                    xmlDoc.LoadXml(result);
                }
                foreach (XmlNode nose in xmlDoc.DocumentElement.ChildNodes)
                {
                    try
                    {
                        //_environmrnts.Add(nose.ChildNodes[0].InnerText.Trim(), "Server=" + nose.ChildNodes[1].InnerText.Trim() + ";UserID=" + nose.ChildNodes[2].InnerText.Trim() + ";Password=");
                        HanaConfigModule.Current.environmrnts.Add(nose.ChildNodes[0].InnerText.Trim(), "Server=" + nose.ChildNodes[1].InnerText.Trim());
                    }
                    catch (Exception)
                    {

                        return Task.FromResult(0); 
                    }
                   
                }
            }


            return Task.FromResult(0);
        }

        protected override Task   OnWriteSettingsAsync(ModuleSettingsWriter settings)
        {
            foreach (string key in _moduleSettings.Keys)
            {
                if (key=="Setting3")
                {
                    try
                    {
                        string tst = new System.Net.NetworkCredential(string.Empty, _Hpass).Password;
                        byte[] plaintxt = UnicodeEncoding.ASCII.GetBytes(tst);
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
                            settings.Add(key, ciphertext.Length.ToString());
                            //length = ciphertext.Length;
                            fStream.Close();
                        }
                        //settings.Add(key, UnicodeEncoding.ASCII.GetString(ciphertext,0,ciphertext.Length));

                        settings.Add(key, ciphertext.Length.ToString());
                    }
                    catch (Exception sss)
                    {
                        string ss =sss.Message;
                        throw;
                    }
                    
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
