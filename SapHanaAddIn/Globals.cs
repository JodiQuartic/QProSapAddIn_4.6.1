
namespace SapHanaAddIn
{
    public static class Globals
    {
        //-------------unmodifiable
        //public const string hanaQaConnStr = "Server=sapxxhana.ad.sannet.gov:30015;UserID=xx;Password=xx";

        //-----------------modifiable
        public static Sap.Data.Hana.HanaConnection hanaConn { get; set; }

    }
}
