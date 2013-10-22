﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Rta.Server
{
    public class DatabaseWriter : IDatabaseWriter
    {
        private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
        private readonly IDatabaseConnectionStringHandler _databaseConnectionStringHandler;
        private static readonly ILog LoggingSvc = LogManager.GetLogger(typeof(DatabaseWriter));

        public DatabaseWriter(IDatabaseConnectionFactory databaseConnectionFactory,
                              IDatabaseConnectionStringHandler databaseConnectionStringHandler)
        {
            _databaseConnectionFactory = databaseConnectionFactory;
            _databaseConnectionStringHandler = databaseConnectionStringHandler;
        }

        public RtaStateGroupLight AddAndGetNewRtaState(string stateCode, Guid platformTypeId, Guid businessUnit)
        {
            var stateId = Guid.NewGuid();
            var defaultStateGroupId = Guid.Empty;
            var defaultStateGroupName = "";
            string getDefaultStateGroupQuery = string.Format(@"SELECT Name, Id, BusinessUnit FROM RtaStateGroup WHERE DefaultStateGroup = 1 AND BusinessUnit = {0}", businessUnit);
            const string insert = @"INSERT INTO RtaState VALUES ('{0}', N'{1}', N'{1}', '{2}', '{3}')";

            using (var connection = _databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.AppConnectionString()))
            {
                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = getDefaultStateGroupQuery;
                connection.Open();
                string insertStatement = string.Empty;
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    defaultStateGroupId = reader.GetGuid(reader.GetOrdinal("Id"));
                    defaultStateGroupName = reader.GetString(reader.GetOrdinal("Name"));

                    insertStatement = string.Format(insert, stateId, stateCode, platformTypeId, defaultStateGroupId);
                }
                reader.Close();

                if (!string.IsNullOrEmpty(insertStatement))
                {
                    command = connection.CreateCommand();
                    command.CommandText = insertStatement;
                    command.ExecuteNonQuery();
                }
            }
            return new RtaStateGroupLight
                {
                    StateGroupId = defaultStateGroupId,
                    StateGroupName = defaultStateGroupName,
                    BusinessUnitId = businessUnit,
                    StateName = stateCode,
                    PlatformTypeId = platformTypeId,
                    StateCode = stateCode,
                    StateId = stateId
                };
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
            MessageId = "0")]
        public void AddOrUpdate(IList<IActualAgentState> actualAgentStates)
        {
            if (!actualAgentStates.Any())
                return;

            using (
                var connection =
                    _databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.DataStoreConnectionString()))
            {
                connection.Open();
                foreach (var newState in actualAgentStates)
                {
                    var command = connection.CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "[RTA].[rta_addorupdate_actualagentstate]";

                    command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@PersonId",
                            SqlDbType = SqlDbType.UniqueIdentifier,
                            Direction = ParameterDirection.Input,
                            Value = newState.PersonId
                        });
                    command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@StateCode",
                            SqlDbType = SqlDbType.NVarChar,
                            Direction = ParameterDirection.Input,
                            Value = newState.StateCode
                        });
                    command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@PlatformTypeId",
                            SqlDbType = SqlDbType.UniqueIdentifier,
                            Direction = ParameterDirection.Input,
                            Value = newState.PlatformTypeId
                        });
                    command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@State",
                            SqlDbType = SqlDbType.NVarChar,
                            Direction = ParameterDirection.Input,
                            Value = newState.State
                        });
                    command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@AlarmName",
                            SqlDbType = SqlDbType.NVarChar,
                            Direction = ParameterDirection.Input,
                            Value = newState.AlarmName
                        });
                    command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@StateId",
                            SqlDbType = SqlDbType.UniqueIdentifier,
                            Direction = ParameterDirection.Input,
                            Value = newState.StateId
                        });
                    command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@Scheduled",
                            SqlDbType = SqlDbType.NVarChar,
                            Direction = ParameterDirection.Input,
                            Value = newState.Scheduled
                        });
                    command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@ScheduledId",
                            SqlDbType = SqlDbType.UniqueIdentifier,
                            Direction = ParameterDirection.Input,
                            Value = newState.ScheduledId
                        });
                    command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@AlarmId",
                            SqlDbType = SqlDbType.UniqueIdentifier,
                            Direction = ParameterDirection.Input,
                            Value = newState.AlarmId
                        });
                    command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@ScheduledNext",
                            SqlDbType = SqlDbType.NVarChar,
                            Direction = ParameterDirection.Input,
                            Value = newState.ScheduledNext
                        });
                    command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@ScheduledNextId",
                            SqlDbType = SqlDbType.UniqueIdentifier,
                            Direction = ParameterDirection.Input,
                            Value = newState.ScheduledNextId
                        });
                    command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@StateStart",
                            SqlDbType = SqlDbType.DateTime,
                            Direction = ParameterDirection.Input,
                            Value = newState.StateStart
                        });
                    command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@NextStart",
                            SqlDbType = SqlDbType.DateTime,
                            Direction = ParameterDirection.Input,
                            Value = newState.NextStart
                        });
                    command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@AlarmStart",
                            SqlDbType = SqlDbType.DateTime,
                            Direction = ParameterDirection.Input,
                            Value = newState.AlarmStart
                        });
                    command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@Color",
                            SqlDbType = SqlDbType.Int,
                            Direction = ParameterDirection.Input,
                            Value = newState.Color
                        });
                    command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@StaffingEffect",
                            SqlDbType = SqlDbType.Float,
                            Direction = ParameterDirection.Input,
                            Value = newState.StaffingEffect
                        });
                    command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@ReceivedTime",
                            SqlDbType = SqlDbType.DateTime,
                            Direction = ParameterDirection.Input,
                            Value = newState.ReceivedTime
                        });
                    if (newState.BatchId != null)
                    {
                        command.Parameters.Add(new SqlParameter
                            {
                                ParameterName = "@BatchId",
                                SqlDbType = SqlDbType.DateTime,
                                Direction = ParameterDirection.Input,
                                Value = newState.BatchId
                            });
                    }
                    else
                        command.Parameters.Add(new SqlParameter
                            {
                                ParameterName = "@BatchId",
                                Direction = ParameterDirection.Input,
                                Value = DBNull.Value
                            });
                    command.Parameters.Add(new SqlParameter
                        {
                            ParameterName = "@OriginalDataSourceId",
                            SqlDbType = SqlDbType.NVarChar,
                            Direction = ParameterDirection.Input,
                            Value = newState.OriginalDataSourceId
                        });
                    command.ExecuteNonQuery();
                    LoggingSvc.DebugFormat("Saved state: {0} to database", newState);
                }
            }
        }
    }
}