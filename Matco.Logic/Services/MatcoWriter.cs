using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matco.Logic.Services
{
    public class MatcoWriter
    {
        public static void Write(string s)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["default"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionstring))
            {
                using (SqlCommand command = new SqlCommand("EEK_sp_Matco", connection))
                {
                    command.CommandTimeout = 300;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@input", SqlDbType.VarChar).Value = s;

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
