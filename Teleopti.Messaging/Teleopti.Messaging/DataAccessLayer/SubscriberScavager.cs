using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Core;
using Teleopti.Messaging.Exceptions;
using log4net;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class SubscriberScavager
    {
        private const string Parameter = "@SCAVAGE_DATETIME_WINDOW";
        private const string CommandText = "msg.sp_Scavage_Subscribers";
        private readonly string _connectionString;
		private static ILog Logger = LogManager.GetLogger(typeof(SubscriberScavager));

        public SubscriberScavager(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void ScavageSubscriber(DateTime scavageDateTimeWindow)
        {
            try
            {
                using (SqlConnection connection = ConnectionFactory.GetInstance(_connectionString).GetOpenConnection())
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = CommandText;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = connection;
                        SqlParameter parameter = new SqlParameter(Parameter, SqlDbType.DateTime);
                        parameter.Value = scavageDateTimeWindow;
                        cmd.Parameters.Add(parameter);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.Error("Subscriber scavager error.", exc);
                throw new DatabaseException("ScavageSubscriber(DateTime scavageDateTimeWindow)", exc);
            }
        }
    }
}
