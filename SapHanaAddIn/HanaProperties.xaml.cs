using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace SapHanaAddIn
{
    /// <summary>
    /// Interaction logic for HanaPropertiesView.xaml
    /// </summary>
    public partial class HanaPropertiesView : UserControl
    {
        public HanaPropertiesView()
        {
            InitializeComponent();
            
            
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            { ((dynamic)this.DataContext).SecurePassword = ((PasswordBox)sender).SecurePassword; }
            HanaConfigModule.Current.Hpass = ((PasswordBox)sender).SecurePassword;
            Project.Current.SetDirty(true);
            //string theString = new NetworkCredential("", ((PasswordBox)sender).SecurePassword).Password;
            //if (this.DataContext != null)
            //{ ((dynamic)this.DataContext).ModuleSetting3 = theString; }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (((dynamic)this.DataContext).SecurePassword != null )
            {
                
                //HANNApasswordBox.Password = ((dynamic)this.DataContext).SecurePassword;

                
            }
            
        }

        private void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();



            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".xml";
            dlg.Filter = "xml Files (*.xml)|*.xml";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document d
                string filename = dlg.FileName;
                ServerName.Text = filename;
            }

            string contentPath;
            HanaConfigModule.Current.Settings.TryGetValue("Setting1", out contentPath);
            XmlDocument xmlDoc = new XmlDocument();
            using (StreamReader reader = new StreamReader(ServerName.Text))
            {
                string xmlinput = reader.ReadToEnd();
                xmlDoc.LoadXml(xmlinput);
            }
            HanaConfigModule.Current.environmrnts.Clear();
            try
            {
                foreach (XmlNode nose in xmlDoc.DocumentElement.ChildNodes)
                {
                    //HanaConfigModule.Current.environmrnts.Add(nose.ChildNodes[0].InnerText.Trim(), "Server=" + nose.ChildNodes[1].InnerText.Trim() + ";UserID=" + nose.ChildNodes[2].InnerText.Trim() + ";Password=");
                    HanaConfigModule.Current.environmrnts.Add(nose.ChildNodes[0].InnerText.Trim(), "Server=" + nose.ChildNodes[1].InnerText.Trim() );
                }
            }
            catch (Exception)
            {

                MessageBox.Show("There was a problem reading the xml file. Please make sure it is in the right format");
            }


        }
    }
}
