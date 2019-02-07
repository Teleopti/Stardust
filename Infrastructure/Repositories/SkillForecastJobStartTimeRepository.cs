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

		public SkillForecastJobStartTimeRepository(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, INow now, SkillForecastSettingsReader skillForecastSettingsReader)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_now = now;
			_skillForecastSettingsReader = skillForecastSettingsReader;
		}

		public DateTime? GetLastCalculatedTime(Guid bu)
		{
			return null;
		}

		public void UpdateJobStartTime(Guid businessUnitId)
		{
			var now = _now.UtcDateTime();
			using (var connection = new SqlConnection(_currentUnitOfWorkFactory.Current().ConnectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
				{
					var effctedRows = 0;
					using (var updateCommand =
						new SqlCommand(
							@"UPDATE [dbo].[SkillForecastJobStartTime] set LockTimestamp = @timestamp, StartTime =  @StartTime  WHERE BusinessUnit = @bu",
							connection, transaction))
					{
						updateCommand.Parameters.AddWithValue("@bu", businessUnitId);
						updateCommand.Parameters.AddWithValue("@startTime", now);
						updateCommand.Parameters.AddWithValue("@timestamp", now.AddMinutes(_skillForecastSettingsReader.MaximumEstimatedExecutionTimeOfJobInMinutes));
						effctedRows = updateCommand.ExecuteNonQuery();
					}
					
					if (effctedRows == 0)
					{
						using (var insertCommand =
							new SqlCommand(
								@"insert into [SkillForecastJobStartTime] (BusinessUnit, StartTime, LockTimestamp) Values (@bu,@startTime,@lockTimestamp)",
								connection, transaction))
						{
							insertCommand.Parameters.AddWithValue("@bu", businessUnitId);
							insertCommand.Parameters.AddWithValue("@startTime", now);
							insertCommand.Parameters.AddWithValue("@lockTimestamp", now.AddMinutes(_skillForecastSettingsReader.MaximumEstimatedExecutionTimeOfJobInMinutes));
							//set lock time to null
							insertCommand.ExecuteNonQuery();
						}
					}
					

					transaction.Commit();
				}
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
					using (var selectCommand = new SqlCommand(@"select LockTimestamp from [SkillForecastJobStartTime] where BusinessUnit = @bu", connection, transaction))
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

		public void ResetLock(Guid businessUnitId)
		{
			using (var connection = new SqlConnection(_currentUnitOfWorkFactory.Current().ConnectionString))
			{
				connection.Open();
				using (var updateCommand = new SqlCommand(@"UPDATE [dbo].[SkillForecastJobStartTime] set LockTimestamp = NULL WHERE BusinessUnit = @bu", connection))
				{
					updateCommand.Parameters.AddWithValue("@bu", businessUnitId);
					updateCommand.ExecuteNonQuery();
				}
			}
		}
	}
}