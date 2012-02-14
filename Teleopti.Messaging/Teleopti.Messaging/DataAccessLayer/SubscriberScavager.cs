using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Teleopti.Core;
using Teleopti.Logging;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class SubscriberScavager
    {
        private const string Parameter = "@SCAVAGE_DATETIME_WINDOW";
        private const string CommandText = "msg.sp_Scavage_Subscribers";
        private readonly string _connectionString;

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
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exc.Message + exc.StackTrace);
                throw new DatabaseException("ScavageSubscriber(DateTime scavageDateTimeWindow)", exc);
            }
        }

    
    }
}
