using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
namespace SapHanaAddIn
{
    public class HanaTables : INotifyPropertyChanged
    {
        private List<string> _tablename;
        public List<string> TableName
        {
            get { return _tablename; }
            set
            {
                _tablename = value;
                RaisePropertyChanged("TableName");
            }
        }

        private List<string> _schemaname;
        public List<string> SchemaName
        {
            get { return _schemaname; }
            set
            {
                _schemaname = value;
                RaisePropertyChanged("SchemaName");
            }
        }

        private string _currentSelected;
        public string CurrentSelected
        {
            get { return _currentSelected; }
            set
            {
                _currentSelected = value;
                RaisePropertyChanged(CurrentSelected);
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        //protected void OnPropertyChanged(string name)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    if (handler != null)
        //    {
        //        handler(this, new PropertyChangedEventArgs(name));
        //    }
        //}
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
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
