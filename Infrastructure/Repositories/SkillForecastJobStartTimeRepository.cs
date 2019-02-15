using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SkillForecastJobStartTimeRepository : ISkillForecastJobStartTimeRepository
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly INow _now;
		private readonly SkillForecastSettingsReader _skillForecastSettingsReader;
		private readonly object _jobLock = new object();

		public SkillForecastJobStartTimeRepository(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, INow now,
			SkillForecastSettingsReader skillForecastSettingsReader)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_now = now;
			_skillForecastSettingsReader = skillForecastSettingsReader;
		}

		public DateTime? GetLastCalculatedTime(Guid businessUnitId)
		{
			DateTime? startTime = null;
			using (var connection = new SqlConnection(_currentUnitOfWorkFactory.Current().ConnectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
				{
					using (var selectCommand =
						new SqlCommand(@"select starttime from [SkillForecastJobStartTime] where BusinessUnit = @bu", connection,
							transaction))
					{
						selectCommand.Parameters.AddWithValue("@bu", businessUnitId);
						using (var reader = selectCommand.ExecuteReader())
						{
							if (reader.HasRows)
							{
								reader.Read();
								if (!reader.IsDBNull(0))
									startTime = reader.GetDateTime(0);
							}
						}
					}
				}
			}
			return startTime;
		}

		public bool UpdateJobStartTime(Guid businessUnitId)
		{
			lock (_jobLock)
			{
				var result = false;
				using (var connection = new SqlConnection(_currentUnitOfWorkFactory.Current().ConnectionString))
				{
					connection.Open();

					var getCommand = connection.CreateCommand();

					getCommand.CommandType = CommandType.StoredProcedure;
					getCommand.CommandText = @"dbo.UpdateJobStartTime";
					getCommand.Parameters.AddWithValue("@businessunitId", businessUnitId);
					getCommand.Parameters.AddWithValue("@now", _now.UtcDateTime());
					getCommand.Parameters.AddWithValue("@maxExecutionTime", _skillForecastSettingsReader.MaximumEstimatedExecutionTimeOfJobInMinutes);
					getCommand.Parameters.Add("@returnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
					getCommand.ExecuteNonQuery();

					result = Convert.ToBoolean(getCommand.Parameters["@returnValue"].Value);
				}
				return result;
			}

		}

		public bool IsLockTimeValid(Guid businessUnitId)
		{
			DateTime? lockTimestamp = null;
			using (var connection = new SqlConnection(_currentUnitOfWorkFactory.Current().ConnectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
				{
					using (var selectCommand =
						new SqlCommand(@"select LockTimestamp from [SkillForecastJobStartTime] where BusinessUnit = @bu", connection,
							transaction))
					{
						selectCommand.Parameters.AddWithValue("@bu", businessUnitId);
						using (var reader = selectCommand.ExecuteReader())
						{
							if (reader.HasRows)
							{
								reader.Read();
								if (!reader.IsDBNull(0))
									lockTimestamp = reader.GetDateTime(0);
							}
						}
					}
				}
			}

			if (lockTimestamp.HasValue)
			{
				return _now.UtcDateTime() < lockTimestamp;
			}

			return true;
		}

		public void ResetLock(Guid businessUnitId, string connectionString = "")
		{
			if (String.IsNullOrEmpty(connectionString))
				connectionString = _currentUnitOfWorkFactory.Current().ConnectionString;
			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction())
				{
					using (var updateCommand =
						new SqlCommand(@"UPDATE [dbo].[SkillForecastJobStartTime] set LockTimestamp = NULL WHERE BusinessUnit = @bu",
							connection, transaction))
					{
						updateCommand.Parameters.AddWithValue("@bu", businessUnitId);
						updateCommand.ExecuteNonQuery();
					}

					transaction.Commit();
				}

			}
		}
	}
}