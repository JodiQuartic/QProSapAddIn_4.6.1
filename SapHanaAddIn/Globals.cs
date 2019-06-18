
namespace SapHanaAddIn
{
    public static class Globals
    {
        //-------------unmodifiable

        //public const string gisDevConnStr = "user id=CITY;password=San.Diego;server=atlasdev;Trusted_Connection=no;database=sdw;connection timeout=30";
        //public const string gisQaConnStr = "user id=CITY;password=San.Diego;server=atlasqa;Trusted_Connection=no;database=sdwqa;connection timeout=30";

        //public const string hanaDevConnStr = "Server=sapde1hana:30015;UserID=jluostarinen;Password=Sap2019!";
        ////public const string hanaQaConnStr = "Server=sapqe1hana.ad.sannet.gov:31015;UserID=jluostarinen;Password=Sap2018!";
        //public const string hanaQaConnStr = "Server=sapqe2hana.ad.sannet.gov:30015;UserID=jluostarinen;Password=Sap2018!";

        //-----------------modifiable
        public static Sap.Data.Hana.HanaConnection hanaConn { get; set; }
        public static bool? isHanaConn { get; set; }

        //public static System.Collections.ObjectModel.Collection<string> collSchemas { get; set; }
        //public static System.Collections.ObjectModel.Collection<string> collTables { get; set; }
    }
}
