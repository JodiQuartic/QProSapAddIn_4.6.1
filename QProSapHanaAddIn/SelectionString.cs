using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QProSapAddIn
{
    class SelectionString : INotifyPropertyChanged

    {
        private string _selectstring = "";
        public string SelectString
        {
            get { return _selectstring; }
            set
            {
                _selectstring = value;
                OnPropertyChanged("SelectString");
            }
        }
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
