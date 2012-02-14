using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace Teleopti.Ccc.DBConverter
{
    public class CommonHelper
    {
        private string _connectionString;
        public CommonHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected CommonHelper(){}

        protected IList<DataRow> ReadData(string query)
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

        public DateTime? PublishedToDate()
        {
            DateTime? ret = null;
            IList<DataRow> result = ReadData("SELECT act_date FROM special_dates WHERE item_key = 'sched_date'");

            if (result.Count > 0)
                ret = (DateTime)result[0]["act_date"];

            return ret;

        }

    }
}
