using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class JobStartTimeRepository : IJobStartTimeRepository
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly INow _now;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		public JobStartTimeRepository(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, INow now, ICurrentBusinessUnit currentBusinessUnit)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_now = now;
			_currentBusinessUnit = currentBusinessUnit;
		}

		public bool CheckAndUpdate(int thresholdMinutes)
		{
			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			var startTime = DateTime.MinValue;
			using (var connection = new SqlConnection(_currentUnitOfWorkFactory.Current().ConnectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
				{
					using (var selectCommand = new SqlCommand(@"select StartTime from [JobStartTime] where BusinessUnit = @bu", connection, transaction))
					{
						selectCommand.Parameters.AddWithValue("@bu", bu);
						var scalar = selectCommand.ExecuteScalar();
						if (scalar != null)
							startTime = (DateTime) scalar;
					}

					//if less than one hour and lock is not null and in the past 
					if (startTime.AddMinutes(thresholdMinutes) > _now.UtcDateTime()) return false;

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
			}
			return true;
		}

		public void Update(Guid bu)
		{
			using (var connection = new SqlConnection(_currentUnitOfWorkFactory.Current().ConnectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
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
						insertCommand.ExecuteNonQuery();
					}
					transaction.Commit();
				}
			}
		}

		public void UpdateLockTimestamp()
		{
		}

		public void ResetLockTimestamp()
		{
		}

		//public IDictionary<Guid, DateTime> LoadAll()
		//{
		//	//var result =
		//	//	((NHibernateUnitOfWork) _currentUnitOfWorkFactory.Current().CurrentUnitOfWork()).Session.CreateSQLQuery(
		//	//		@"SELECT BusinessUnit, StartTime  FROM [JobStartTime] with(nolock)")
		//	//	.SetResultTransformer(Transformers.AliasToBean(typeof(StartTimePerBu)))
		//	//	.List<StartTimePerBu>();
		//	var result =
		//		((NHibernateUnitOfWork)_currentUnitOfWorkFactory.Current().CurrentUnitOfWork()).Session.CreateSQLQuery(
		//			@"SELECT BusinessUnit, StartTime FROM [JobStartTime]")
		//		.SetResultTransformer(Transformers.AliasToBean(typeof(StartTimePerBu)))
		//		.List<StartTimePerBu>();
		//	return result.ToDictionary(x => x.BusinessUnit, y => y.StartTime);
		//}
	}

	//internal class StartTimePerBu
	//{
	//	public Guid BusinessUnit { get; set; }
	//	public DateTime StartTime { get; set; }
	//}
}