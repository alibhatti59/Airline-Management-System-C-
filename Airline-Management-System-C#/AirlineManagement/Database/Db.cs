using System.Data.SqlClient;

namespace AirlineManagement.Database
{
    public static class Db
    {
        private const string FallbackConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Ali Bhatti\Source\Repos\Airline-Management-System-New\AirlineManagement\AirlineManagement\AirlineManagement\AirlineDatabase.mdf;Integrated Security=True;Connect Timeout=30";

        public static string ConnectionString
        {
            get
            {
                return FallbackConnectionString;
            }
        }

        public static SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}

