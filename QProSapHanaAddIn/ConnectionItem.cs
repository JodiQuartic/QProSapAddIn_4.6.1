using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace QProSapAddIn
{
    public class ConnectionItem:INotifyPropertyChanged
    {
        public string name { get; set; }
        public string server { get; set; }
        public string userid { get; set; }
        public SecureString pass { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;


        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
