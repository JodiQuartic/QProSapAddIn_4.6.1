using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
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

namespace QProSapAddIn
{
    /// <summary>
    /// Interaction logic for SapPropertiesView.xaml
    /// </summary>
    public partial class SapPropertiesView : UserControl
    {
        public ObservableCollection<ConnectionItem> MyColl { get; set; }
        public SapPropertiesView()
        {
            InitializeComponent();
            
            
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            { ((dynamic)this.DataContext).SecurePassword = ((PasswordBox)sender).SecurePassword; }
            SapConfigModule.Current.Hpass = ((PasswordBox)sender).SecurePassword;
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

            //dgConnections.ItemsSource = MyColl;


        }

        //private void btnSelectFile_Click(object sender, RoutedEventArgs e)
        //{
        //    Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();



        //    // Set filter for file extension and default file extension 
        //    dlg.DefaultExt = ".xml";
        //    dlg.Filter = "xml Files (*.xml)|*.xml";


        //    // Display OpenFileDialog by calling ShowDialog method 
        //    Nullable<bool> result = dlg.ShowDialog();


        //    // Get the selected file name and display in a TextBox 
        //    if (result == true)
        //    {
        //        // Open document d
        //        string filename = dlg.FileName;
        //        ServerName.Text = filename;
        //    }

        //    string contentPath;
        //    SapConfigModule.Current.Settings.TryGetValue("Setting1", out contentPath);
        //    XmlDocument xmlDoc = new XmlDocument();
        //    using (StreamReader reader = new StreamReader(ServerName.Text))
        //    {
        //        string xmlinput = reader.ReadToEnd();
        //        xmlDoc.LoadXml(xmlinput);
        //    }
        //    SapConfigModule.Current.environmrnts.Clear();
        //    try
        //    {
        //        foreach (XmlNode nose in xmlDoc.DocumentElement.ChildNodes)
        //        {
        //            //SapConfigModule.Current.environmrnts.Add(nose.ChildNodes[0].InnerText.Trim(), "Server=" + nose.ChildNodes[1].InnerText.Trim() + ";UserID=" + nose.ChildNodes[2].InnerText.Trim() + ";Password=");
        //            SapConfigModule.Current.environmrnts.Add(nose.ChildNodes[0].InnerText.Trim(), "Server=" + nose.ChildNodes[1].InnerText.Trim() );
        //        }
        //    }
        //    catch (Exception)
        //    {

        //        MessageBox.Show("There was a problem reading the xml file. Please make sure it is in the right format");
        //    }


        //}

        private void dgConnections_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "pass")
            {
                //DataGrid dg = (DataGrid)sender;
                //foreach (ConnectionItem item in dg.Items)
                //{

                //}
                e.Cancel = true;
            }
            else
            {
                e.Cancel = true;
            }
            

        }

        private void PasswordBox_PasswordChanged_1(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            { ((dynamic)this.DataContext).SecurePassword = ((PasswordBox)sender).SecurePassword; }
            SapConfigModule.Current.Hpass = ((PasswordBox)sender).SecurePassword;
            var oo = e.Source;
            ConnectionItem sss= dgConnections.CurrentItem as ConnectionItem;

            SapConfigModule.Current.ConnectionItems[dgConnections.SelectedIndex].pass = ((PasswordBox)sender).SecurePassword;
            foreach (ConnectionItem  item in SapConfigModule.Current.ConnectionItems)
            {
                ConnectionItem ss = (ConnectionItem)dgConnections.CurrentItem;
                if (item.name == ss.name)
                {
                    item.pass = ((PasswordBox)sender).SecurePassword;
                }
            } 
            Project.Current.SetDirty(true);
        }

        private void dgConnections_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            Project.Current.SetDirty(true);
        }

        private void dgConnections_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Project.Current.SetDirty(true);

        }
    }
}
