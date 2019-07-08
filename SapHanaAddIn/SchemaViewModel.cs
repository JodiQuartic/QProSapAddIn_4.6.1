using ArcGIS.Desktop.Framework;
using System.Windows.Input;
using MicroMvvm;

namespace SapHanaAddIn
{
    public class SchemaViewModel : ObservableObject
        {
            #region Construction
            /// <summary>
            /// Constructs the default instance of a SchemaViewModel
            /// </summary>
            public SchemaViewModel()
            {
               
            }
            #endregion

            #region Members
            Schema _schema;
            int _count = 0;
            #endregion

        #region Properties
        public Schema Schema
        {
            get
            {
                return _schema;
            }
            set
            {
                _schema = value;
            }
        }

        public string SchemaName
            {
            get { return Schema.SchemaName; }
            set
            {
                if (Schema.SchemaName != value)
                {
                    Schema.SchemaName = value;
                    RaisePropertyChanged("SchemaName");
                }
            }
        }
            #endregion

            #region Commands
            void UpdateSchemaNameExecute()
            {
                ++_count;
                SchemaName = string.Format("Elvis ({0})", _count);
            }

            bool CanUpdateSchemaNameExecute()
            {
                return true;
            }

            public ICommand UpdateSchemaName { get { return new MicroMvvm.RelayCommand(UpdateSchemaNameExecute, CanUpdateSchemaNameExecute); } }

            #endregion
        }
    }
