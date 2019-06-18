using System.Collections.Generic;
using System.ComponentModel;
namespace SapHanaAddIn
{
    public class HanaTables : INotifyPropertyChanged
    {
        private List<string> _tablelist;
        public List<string> TableList
        {
            get { return _tablelist; }
            set
            {
                _tablelist = value;
                RaisePropertyChanged("TableList");
            }
        }


        private string _currentselected;
        public string CurrentSelected
        {
            get { return _currentselected; }
            set
            {
                _currentselected = value;
                RaisePropertyChanged("CurrentSelected");
            }
        }
        private string _currenttable;
        public string CurrentTable
        {
            get { return _currenttable; }
            set
            {
                _currenttable = value;
                RaisePropertyChanged("CurrentTable");
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    public class querytext : INotifyPropertyChanged
    {
        private string _txtQuery;
        public string TxtQuery
        {
            get { return _txtQuery; }
            set
            {
                _txtQuery = value;
                RaisePropertyChanged(TxtQuery);
            }
        }
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
