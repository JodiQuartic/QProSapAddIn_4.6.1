using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Security;
using System.Windows;
using System.IO;
using System.Security.Cryptography;

namespace SapHanaAddIn
{
    internal class HanaPropertiesViewModel : Page
    {
        private string _origModuleSetting1 = "";
        private string _origModuleSetting2 = "";
        private string _origModuleSetting3 = "";
        private SecureString _SecurePassword = new SecureString() ;
        public SecureString SecurePassword
        {
            get { return _SecurePassword;
            }
            set { } }
        private bool IsDirty()
        {
            if (_origModuleSetting1 != ModuleSetting1)
                return true;
            if (_origModuleSetting2 != ModuleSetting2)
                return true;
            if (_origModuleSetting3 != ModuleSetting3)
                return true;
            return false;
        }

        /// <summary>
        /// Invoked when the OK or apply button on the property sheet has been clicked.
        /// </summary>
        /// <returns>A task that represents the work queued to execute in the ThreadPool.</returns>
        /// <remarks>This function is only called if the page has set its IsModified flag to true.</remarks>
        protected override Task CommitAsync()
        {
            if (IsDirty())
            {
                // store the new settings in the dictionary ... save happens in OnProjectSave
                Dictionary<string, string> settings = HanaConfigModule.Current.Settings;

                if (settings.ContainsKey("Setting1"))
                    settings["Setting1"] = ModuleSetting1.ToString();
                else
                    settings.Add("Setting1", ModuleSetting1.ToString());

                if (settings.ContainsKey("Setting2"))
                    settings["Setting2"] = ModuleSetting2;
                else
                    settings.Add("Setting2", ModuleSetting2);

                if (settings.ContainsKey("Setting3"))
                    settings["Setting3"] = ModuleSetting3;
                else
                    settings.Add("Setting3", ModuleSetting3);
                // set the project dirty
                Project.Current.SetDirty(true);
            }
            return Task.FromResult(0);
        }


        private string _moduleSetting1;
        public string ModuleSetting1
        {
            get { return _moduleSetting1; }
            set
            {
                if (SetProperty(ref _moduleSetting1, value, () => ModuleSetting1))
                    //You must set "IsModified = true" to have your CommitAsync called
                    base.IsModified = true;
            }
        }


        private string _moduleSetting2;
        public string ModuleSetting2
        {
            get { return _moduleSetting2; }
            set
            {
                if (SetProperty(ref _moduleSetting2, value, () => ModuleSetting2))
                    //You must set "IsModified = true" to have your CommitAsync called
                    base.IsModified = true;
            }
        }
        private string _moduleSetting3;
        public string ModuleSetting3
        {
            get { return _moduleSetting3; }
            set
            {
                if (SetProperty(ref _moduleSetting3, value, () => ModuleSetting3))
                    //You must set "IsModified = true" to have your CommitAsync called
                    base.IsModified = true;
            }
        }
        /// <summary>
        /// Called when the page loads because to has become visible.
        /// </summary>
        /// <returns>A task that represents the work queued to execute in the ThreadPool.</returns>
        protected override Task InitializeAsync()
        {
            Dictionary<string, string> settings = HanaConfigModule.Current.Settings;

            // assign to the values biniding to the controls
            if (settings.ContainsKey("Setting1"))
                ModuleSetting1 = settings["Setting1"];
            else
                ModuleSetting1 = "";

            if (settings.ContainsKey("Setting2"))
                ModuleSetting2 = settings["Setting2"];
            else
                ModuleSetting2 = "";

            if (settings.ContainsKey("Setting3"))
                ModuleSetting3 = settings["Setting3"];
            else
                ModuleSetting3 = "";
            // keep track of the original values (used for comparison when saving)
            _origModuleSetting1 = ModuleSetting1;
            _origModuleSetting2 = ModuleSetting2;
            _origModuleSetting3 = ModuleSetting3;
            string len = ModuleSetting3;
            if (len != "")
            {
                byte[] inBuffer = new byte[int.Parse(len)];
                byte[] outBuffer;
                string pass = "";
                
                string contentPath = System.IO.Path.Combine(Project.Current.HomeFolderPath,  "Data.dat");
                using (FileStream fStream = new FileStream(contentPath, FileMode.Open))
                {
                    if (fStream.CanRead)
                    {
                        fStream.Read(inBuffer, 0, int.Parse(len));
                        byte[] entropy = new byte[20];
                        try
                        {
                            outBuffer = ProtectedData.Unprotect(inBuffer, null, DataProtectionScope.CurrentUser);
                            pass = UnicodeEncoding.ASCII.GetString(outBuffer);
                        }
                        catch (Exception)
                        {
                            pass = "";
                            outBuffer = null;
                            SecurePassword = new SecureString();
                            System.Windows.MessageBox.Show("Could not get a stored password for HANA \r\n Please set it in Project Settings << HANA Options");
                            return Task.FromResult(0); ;
                        }
                        
                    }
                }
                SecurePassword = new SecureString();
                foreach (char cc in pass)
                {
                    SecurePassword.AppendChar(cc);
                }
                pass = "";
                outBuffer = null;
            }
            else
            {
                SecurePassword = new SecureString();
            }


            return Task.FromResult(0);
        }

        /// <summary>
        /// Called when the page is destroyed.
        /// </summary>
        protected override void Uninitialize()
        {
        }
        public static string AddinAssemblyLocation()
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            return System.IO.Path.GetDirectoryName(
                              Uri.UnescapeDataString(
                                      new Uri(asm.CodeBase).LocalPath));
        }
        /// <summary>
        /// Text shown inside the page hosted by the property sheet
        /// </summary>

    }

    /// <summary>
    /// Button implementation to show the property sheet.
    /// </summary>

}
