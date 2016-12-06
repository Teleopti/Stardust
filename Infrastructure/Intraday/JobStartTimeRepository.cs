using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Intraday
{
	public interface IJobStartTimeRepository
	{
		void Persist(Guid buId, DateTime datetime);
		IDictionary<Guid, DateTime> LoadAll();
	}

	public class JobStartTimeRepository : IJobStartTimeRepository
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public JobStartTimeRepository(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public void Persist(Guid buId, DateTime datetime)
		{
			var connectionString = _currentUnitOfWorkFactory.Current().ConnectionString;


			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
				{
					var deleteCommand = new SqlCommand();
					deleteCommand.CommandText = @"delete from [JobStartTime] where BusinessUnit  = @bu";
					deleteCommand.Connection = connection;
					deleteCommand.Transaction = transaction;
					deleteCommand.Parameters.AddWithValue("@bu", buId);
					deleteCommand.ExecuteNonQuery();

					var insertCommand = new SqlCommand();
					insertCommand.CommandText = @"insert into [JobStartTime](BusinessUnit, StartTime) Values (@bu,@startTime) ";
					insertCommand.Connection = connection;
					insertCommand.Transaction = transaction;
					insertCommand.Parameters.AddWithValue("@bu", buId);
					insertCommand.Parameters.AddWithValue("@startTime", datetime);
					insertCommand.ExecuteNonQuery();

					transaction.Commit();
				}
			}
		}

		public IDictionary<Guid, DateTime> LoadAll()
		{
			var result =
				((NHibernateUnitOfWork)_currentUnitOfWorkFactory.Current().CurrentUnitOfWork()).Session.CreateSQLQuery(
						@"SELECT BusinessUnit, StartTime 
				 FROM [JobStartTime]")
					.SetResultTransformer(Transformers.AliasToBean(typeof(StartTimePerBu)))
					.List<StartTimePerBu>();

			return result.ToDictionary(x => x.BusinessUnit, y => y.StartTime);
		}
	}

	internal class StartTimePerBu
	{
		public Guid BusinessUnit { get; set; }
		public DateTime StartTime { get; set; }
	}
}