﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

		const string getDefaultStateGroupQuery = @"SELECT Name, Id, BusinessUnit FROM RtaStateGroup WHERE DefaultStateGroup = 1 AND BusinessUnit = @BusinessUnitId";
		const string insert = @"INSERT INTO RtaState VALUES (@StateId, @StateCode, @PlatformTypeId, @DefaultStateGroupId)";
            
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

            using (var connection = _databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.AppConnectionString()))
            {
                var command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = getDefaultStateGroupQuery;
	            command.Parameters.Add(new SqlParameter
	            {
		            ParameterName = "@BusinessUnitId",
		            SqlDbType = SqlDbType.UniqueIdentifier,
		            Direction = ParameterDirection.Input,
		            Value = businessUnit
	            });
                connection.Open();
	            var parameters = new List<SqlParameter>();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    defaultStateGroupId = reader.GetGuid(reader.GetOrdinal("Id"));
                    defaultStateGroupName = reader.GetString(reader.GetOrdinal("Name"));

					parameters.Add(new SqlParameter
					{
						ParameterName = "@StateId",
						SqlDbType = SqlDbType.UniqueIdentifier,
						Direction = ParameterDirection.Input,
						Value = stateId
					});

					parameters.Add(new SqlParameter
					{
						ParameterName = "@StateCode",
						SqlDbType = SqlDbType.NVarChar,
						Direction = ParameterDirection.Input,
						Value = stateCode
					});

					parameters.Add(new SqlParameter
					{
						ParameterName = "@PlatformTypeId",
						SqlDbType = SqlDbType.UniqueIdentifier,
						Direction = ParameterDirection.Input,
						Value = platformTypeId
					});

					parameters.Add(new SqlParameter
					{
						ParameterName = "@DefaultStateGroupId",
						SqlDbType = SqlDbType.UniqueIdentifier,
						Direction = ParameterDirection.Input,
						Value = defaultStateGroupId
					});

					break;
                }
                reader.Close();

                if (parameters.Count>0)
                {
                    command = connection.CreateCommand();
                    command.CommandText = insert;
	                parameters.ForEach(p => command.Parameters.Add(p));
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

        public void AddOrUpdate(IActualAgentState actualAgentState)
        {
            using (
                var connection =
                    _databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.DataStoreConnectionString()))
            {
	            connection.Open();
	            var command = connection.CreateCommand();
	            command.CommandType = CommandType.StoredProcedure;
	            command.CommandText = "[RTA].[rta_addorupdate_actualagentstate]";

	            command.Parameters.Add(new SqlParameter
	            {
		            ParameterName = "@PersonId",
		            SqlDbType = SqlDbType.UniqueIdentifier,
		            Direction = ParameterDirection.Input,
		            Value = actualAgentState.PersonId
	            });
	            command.Parameters.Add(new SqlParameter
	            {
		            ParameterName = "@StateCode",
		            SqlDbType = SqlDbType.NVarChar,
		            Direction = ParameterDirection.Input,
		            Value = actualAgentState.StateCode
	            });
	            command.Parameters.Add(new SqlParameter
	            {
		            ParameterName = "@PlatformTypeId",
		            SqlDbType = SqlDbType.UniqueIdentifier,
		            Direction = ParameterDirection.Input,
		            Value = actualAgentState.PlatformTypeId
	            });
	            command.Parameters.Add(new SqlParameter
	            {
		            ParameterName = "@State",
		            SqlDbType = SqlDbType.NVarChar,
		            Direction = ParameterDirection.Input,
		            Value = actualAgentState.State
	            });
	            command.Parameters.Add(new SqlParameter
	            {
		            ParameterName = "@AlarmName",
		            SqlDbType = SqlDbType.NVarChar,
		            Direction = ParameterDirection.Input,
		            Value = actualAgentState.AlarmName
	            });
	            command.Parameters.Add(new SqlParameter
	            {
		            ParameterName = "@StateId",
		            SqlDbType = SqlDbType.UniqueIdentifier,
		            Direction = ParameterDirection.Input,
		            Value = actualAgentState.StateId
	            });
	            command.Parameters.Add(new SqlParameter
	            {
		            ParameterName = "@Scheduled",
		            SqlDbType = SqlDbType.NVarChar,
		            Direction = ParameterDirection.Input,
		            Value = actualAgentState.Scheduled
	            });
	            command.Parameters.Add(new SqlParameter
	            {
		            ParameterName = "@ScheduledId",
		            SqlDbType = SqlDbType.UniqueIdentifier,
		            Direction = ParameterDirection.Input,
		            Value = actualAgentState.ScheduledId
	            });
	            command.Parameters.Add(new SqlParameter
	            {
		            ParameterName = "@AlarmId",
		            SqlDbType = SqlDbType.UniqueIdentifier,
		            Direction = ParameterDirection.Input,
		            Value = actualAgentState.AlarmId
	            });
	            command.Parameters.Add(new SqlParameter
	            {
		            ParameterName = "@ScheduledNext",
		            SqlDbType = SqlDbType.NVarChar,
		            Direction = ParameterDirection.Input,
		            Value = actualAgentState.ScheduledNext
	            });
	            command.Parameters.Add(new SqlParameter
	            {
		            ParameterName = "@ScheduledNextId",
		            SqlDbType = SqlDbType.UniqueIdentifier,
		            Direction = ParameterDirection.Input,
		            Value = actualAgentState.ScheduledNextId
	            });
	            command.Parameters.Add(new SqlParameter
	            {
		            ParameterName = "@StateStart",
		            SqlDbType = SqlDbType.DateTime,
		            Direction = ParameterDirection.Input,
		            Value = actualAgentState.StateStart
	            });
	            command.Parameters.Add(new SqlParameter
	            {
		            ParameterName = "@NextStart",
		            SqlDbType = SqlDbType.DateTime,
		            Direction = ParameterDirection.Input,
		            Value = actualAgentState.NextStart
	            });
	            command.Parameters.Add(new SqlParameter
	            {
		            ParameterName = "@AlarmStart",
		            SqlDbType = SqlDbType.DateTime,
		            Direction = ParameterDirection.Input,
		            Value = actualAgentState.AlarmStart
	            });
	            command.Parameters.Add(new SqlParameter
	            {
		            ParameterName = "@Color",
		            SqlDbType = SqlDbType.Int,
		            Direction = ParameterDirection.Input,
		            Value = actualAgentState.Color
	            });
	            command.Parameters.Add(new SqlParameter
	            {
		            ParameterName = "@StaffingEffect",
		            SqlDbType = SqlDbType.Float,
		            Direction = ParameterDirection.Input,
		            Value = actualAgentState.StaffingEffect
	            });
	            command.Parameters.Add(new SqlParameter
	            {
		            ParameterName = "@ReceivedTime",
		            SqlDbType = SqlDbType.DateTime,
		            Direction = ParameterDirection.Input,
		            Value = actualAgentState.ReceivedTime
	            });
	            if (actualAgentState.BatchId != null)
	            {
		            command.Parameters.Add(new SqlParameter
		            {
			            ParameterName = "@BatchId",
			            SqlDbType = SqlDbType.DateTime,
			            Direction = ParameterDirection.Input,
			            Value = actualAgentState.BatchId
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
		            Value = actualAgentState.OriginalDataSourceId
	            });
	            command.ExecuteNonQuery();
	            LoggingSvc.DebugFormat("Saved state: {0} to database", actualAgentState);
            }
        }
    }
}