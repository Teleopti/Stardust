using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace Teleopti.Support.Security
{
    public class CommonHelper
    {
        private string _connectionString;
        public CommonHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IList<DataRow> ReadData(string query)
        {
            IList<DataRow> ret = new List<DataRow>();

            DataSet settet = new DataSet();
            settet.Locale = CultureInfo.CurrentCulture;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataAdapter Adap = new SqlDataAdapter(command))
                {
                    Adap.Fill(settet, "Data");
                }
            }
            foreach (DataRow row in settet.Tables[0].Rows)
            {
                ret.Add(row);
            }
            return ret;
        }

        protected string ReadString(string query)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using(var command=new SqlCommand(query,connection))
                {
                    connection.Open();
                    return (string)command.ExecuteScalar();
                }
            }
        }

        

		public void UpdateAuditPersonAssignmentRows(IList<DataRow> rows)
		{
			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				using (var command = new SqlCommand())
				{
					command.CommandType = CommandType.Text;
					connection.Open();
					command.Connection = connection;
					foreach (var dataRow in rows)
					{
						command.CommandText = "update [Auditing].PersonAssignment_AUD set TheDate = '" + dataRow.Field<DateTime>("TheDate") +
											  "' where Id='" + dataRow.Field<Guid>("Id").ToString() + "'";
						command.ExecuteNonQuery();
					}

				}
			}
		}

    }
}
