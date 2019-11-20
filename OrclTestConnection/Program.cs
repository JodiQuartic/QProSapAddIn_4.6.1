using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace OrclTestConnection
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create a connection to Oracle
            string conString = "User Id=sde; password=Quart2019; Data Source=localhost:1521/orcl; Pooling=false;";

            //How to connect to an Oracle DB without SQL*Net configuration file
            //also known as tnsnames.ora.
            //"Data Source=localhost:1521/pdborcl; Pooling=false;";

            //How to connect to an Oracle Database with a Database alias.
            //Uncomment below and comment above.
            //"Data Source=ORCL;Pooling=false;";

            OracleConnection con = new OracleConnection();
            con.ConnectionString = conString;
            con.Open();

            //Create a command within the context of the connection
            //Use the command to display employee names and salary from the Employees table
            OracleCommand cmd = con.CreateCommand();
            cmd.CommandText = "select BASIN from SEWER_PIPES";

            //Execute the command and use datareader to display the data
            OracleDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine("FieldValue: " + reader.GetString(0));
            }
            Console.ReadLine();

        }
    }
}
