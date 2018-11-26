using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class JobStartTimeRepository : IJobStartTimeRepository
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly INow _now;

		public JobStartTimeRepository(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, INow now)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_now = now;
		}

		public bool CheckAndUpdate(int thresholdMinutes, Guid bu)
		{
			var startTime = DateTime.MinValue;
			DateTime? lockTimestamp = null;
			using (var connection = new SqlConnection(_currentUnitOfWorkFactory.Current().ConnectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
				{
					using (var selectCommand = new SqlCommand(@"select StartTime, LockTimestamp from [JobStartTime] where BusinessUnit = @bu", connection, transaction))
					{
						selectCommand.Parameters.AddWithValue("@bu", bu);
						using (var reader = selectCommand.ExecuteReader())
						{
							if (reader.HasRows)
							{
								reader.Read();
								startTime = reader.GetDateTime(0);
								if(!reader.IsDBNull(1))
									lockTimestamp = reader.GetDateTime(1);
							}
						}
					}
					
					var isInvalidLockTimestamp = lockTimestamp != null && lockTimestamp < _now.UtcDateTime();
					if (startTime.AddMinutes(thresholdMinutes) < _now.UtcDateTime() || isInvalidLockTimestamp)
					{
						using (var deleteCommand = new SqlCommand(@"delete from [JobStartTime] where BusinessUnit = @bu", connection, transaction))
						{
							deleteCommand.Parameters.AddWithValue("@bu", bu);
							deleteCommand.ExecuteNonQuery();
						}

						using (var insertCommand = new SqlCommand(@"insert into [JobStartTime] (BusinessUnit, StartTime) Values (@bu,@startTime)", connection, transaction))
						{
							insertCommand.Parameters.AddWithValue("@bu", bu);
							insertCommand.Parameters.AddWithValue("@startTime", _now.UtcDateTime());
							//set lock time to null
							insertCommand.ExecuteNonQuery();
						}
						transaction.Commit();
					}
					else return false;
				}
			}
			return true;
		}

		public void UpdateLockTimestamp(Guid bu)
		{
			using (var connection = new SqlConnection(_currentUnitOfWorkFactory.Current().ConnectionString))
			{
				connection.Open();
				using (var updateCommand = new SqlCommand(@"UPDATE [dbo].[JobStartTime] set LockTimestamp = @timestamp WHERE BusinessUnit = @bu", connection))
				{
					updateCommand.Parameters.AddWithValue("@bu", bu);
					updateCommand.Parameters.AddWithValue("@timestamp", _now.UtcDateTime().AddMinutes(5));
					updateCommand.ExecuteNonQuery();
				}
			}
		}

		public void ResetLockTimestamp(Guid bu)
		{
			using (var connection = new SqlConnection(_currentUnitOfWorkFactory.Current().ConnectionString))
			{
				connection.Open();
				using (var updateCommand = new SqlCommand(@"UPDATE [dbo].[JobStartTime] set LockTimestamp = NULL WHERE BusinessUnit = @bu", connection))
				{
					updateCommand.Parameters.AddWithValue("@bu", bu);
					updateCommand.ExecuteNonQuery();
				}
			}
		}

		public void RemoveLock(Guid bu)
		{
			using (var connection = new SqlConnection(_currentUnitOfWorkFactory.Current().ConnectionString))
			{
				connection.Open();
				using (var deleteCommand = new SqlCommand(@"delete from [JobStartTime] where BusinessUnit = @bu", connection))
				{
					deleteCommand.Parameters.AddWithValue("@bu", bu);
					deleteCommand.ExecuteNonQuery();
				}
			}
		}
	}
}