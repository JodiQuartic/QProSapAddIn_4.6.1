using System.Collections.Generic;
using System.ComponentModel;
namespace SapHanaAddIn
{
    public class HanaTables : INotifyPropertyChanged
    {

        private List<string> _schemalist;
        public List<string> SchemaList
        {
            get { return _schemalist; }
            set
            {
                _schemalist = value;
                RaisePropertyChanged("SchemaList");
            }
        }
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

        private string _currentschema;
        public string CurrentSchema
        {
            get { return _currentschema; }
            set
            {
                _currentschema = value;
                RaisePropertyChanged("CurrentSchema");
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
