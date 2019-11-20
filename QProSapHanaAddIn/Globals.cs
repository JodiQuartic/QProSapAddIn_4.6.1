namespace QProSapAddIn
{
    public static class Globals
    {
        //-------------unmodifiable
        public const string RdbmsType = "Oracle";

        //-----------------modifiable
        public static  Oracle.ManagedDataAccess.Client.OracleConnection DBConn { get; set; }
        public static Sap.Data.Hana.HanaConnection HanaDBConn { get; set; }

    }
}
