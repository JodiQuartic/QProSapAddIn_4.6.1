
namespace SapHanaAddIn
{
    /// <summary>
    /// Model of a 'schema'.
    /// </summary>
    public class Schema
    {
        #region Members
        string _schemaName;

        #endregion


        #region Properties
        /// <summary>
        /// The schema name.
        /// </summary>
        public string SchemaName
        {
            get { return _schemaName; }
            set { _schemaName = value; }
        }


        #endregion
    }
}
