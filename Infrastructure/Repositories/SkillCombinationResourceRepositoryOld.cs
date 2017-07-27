using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using NHibernate.SqlAzure;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SkillCombinationResourceRepositoryOld : ISkillCombinationResourceRepository
	{
		private readonly INow _now;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly RetryPolicy _retryPolicy;
		private readonly object skillCombinationLock = new object();
		private readonly IStardustJobFeedback _stardustJobFeedback;

		public SkillCombinationResourceRepositoryOld(INow now, ICurrentUnitOfWork currentUnitOfWork,
												  ICurrentBusinessUnit currentBusinessUnit, IStardustJobFeedback stardustJobFeedback)
		{
			_now = now;
			_currentUnitOfWork = currentUnitOfWork;
			_currentBusinessUnit = currentBusinessUnit;
			_stardustJobFeedback = stardustJobFeedback;
			_retryPolicy = new RetryPolicy<SqlTransientErrorDetectionStrategyWithTimeouts>(5, TimeSpan.FromMilliseconds(100));
		}

		private Guid persistSkillCombination(IEnumerable<Guid> skillCombination, SqlConnection connection, SqlTransaction transaction)
		{
			var combinationId = Guid.NewGuid();
			var dt = new DataTable();
			dt.Columns.Add("Id", typeof(Guid));
			dt.Columns.Add("SkillId", typeof(Guid));
			dt.Columns.Add("InsertedOn", typeof(DateTime));

			var insertedOn = _now.UtcDateTime();


			foreach (var skill in skillCombination)
			{
				var row = dt.NewRow();
				row["SkillId"] = skill;
				row["Id"] = combinationId;
				row["InsertedOn"] = insertedOn;
				dt.Rows.Add(row);
			}


			using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
			{
				sqlBulkCopy.DestinationTableName = "[ReadModel].[SkillCombination]";
				sqlBulkCopy.WriteToServer(dt);
			}

			return combinationId;
		}

		private Dictionary<GuidCombinationKey, Guid> loadSkillCombination(SqlConnection connection, SqlTransaction transaction)
		{
		    
			var skillCombinations = new Dictionary<GuidCombinationKey, Guid>();
			var result = new List<internalSkillCombination>();
			using (var command = new SqlCommand("select Id, SkillId from [ReadModel].[SkillCombination]", connection, transaction))
			{

				if (transaction == null)
					_currentUnitOfWork.Current().Session().Transaction.Enlist(command);
				
				using (var reader = command.ExecuteReader())
				{
					if (!reader.HasRows) return skillCombinations;
					while (reader.Read())
					{
						var internalSkillCombination = new internalSkillCombination
						{
							Id = reader.GetGuid(0),
							SkillId = reader.GetGuid(1)
						};
						result.Add(internalSkillCombination);
					}
				}
			}
			var groups = result.GroupBy(x => x.Id);
			foreach (var group in groups)
			{
				var key = keyFor(group.Select(x => x.SkillId));
				if (skillCombinations.ContainsKey(key))
					continue;
				skillCombinations.Add(key, group.Key);
			}

			return skillCombinations;
		}

		private static GuidCombinationKey keyFor(IEnumerable<Guid> skillIds)
		{
			return new GuidCombinationKey(skillIds.OrderBy(x => x).ToArray());
		}

		private class internalSkillCombination
		{
			public Guid Id { get; set; }
			public Guid SkillId { get; set; }
		}

	    public virtual void PersistSkillCombinationResource(DateTime dataLoaded,
	        IEnumerable<SkillCombinationResource> skillCombinationResources)
	    {
			_retryPolicy.ExecuteAction(() =>
			{
				tryPersistSkillCombinationResource(dataLoaded,  skillCombinationResources);
			});
		}

		private void tryPersistSkillCombinationResource(DateTime dataLoaded,
			IEnumerable<SkillCombinationResource> skillCombinationResources)
		{
			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			_stardustJobFeedback.SendProgress(
				$"Start persist {skillCombinationResources.Count()} skillCombinationResources for bu {bu}.");
			lock (skillCombinationLock)
			{
				var connectionString = _currentUnitOfWork.Current().Session().Connection.ConnectionString;
				using (var connection = new SqlConnection(connectionString))
				{
					connection.OpenWithRetry(_retryPolicy);
					var dt = new DataTable();
					dt.Columns.Add("SkillCombinationId", typeof(Guid));
					dt.Columns.Add("StartDateTime", typeof(DateTime));
					dt.Columns.Add("EndDateTime", typeof(DateTime));
					dt.Columns.Add("Resource", typeof(double));
					dt.Columns.Add("InsertedOn", typeof(DateTime));
					dt.Columns.Add("BusinessUnit", typeof(Guid));
					var insertedOn = _now.UtcDateTime();


					using (var transaction = connection.BeginTransaction())
					{
						var skillCombinations = loadSkillCombination(connection, transaction);
						foreach (var skillCombinationResource in skillCombinationResources)
						{
							var key = keyFor(skillCombinationResource.SkillCombination);
							Guid id;

							if (!skillCombinations.TryGetValue(key, out id))
							{
								id = persistSkillCombination(skillCombinationResource.SkillCombination, connection,transaction);
								skillCombinations.Add(key, id);
							}

							var row = dt.NewRow();
							row["SkillCombinationId"] = id;
							row["StartDateTime"] = skillCombinationResource.StartDateTime;
							row["EndDateTime"] = skillCombinationResource.EndDateTime;
							row["Resource"] = skillCombinationResource.Resource;
							row["InsertedOn"] = insertedOn;
							row["BusinessUnit"] = bu;
							dt.Rows.Add(row);
						}
						transaction.Commit();
					}


					using (var transaction = connection.BeginTransaction())
					{
						using (var deleteCommand = new SqlCommand(@"DELETE d FROM ReadModel.SkillCombinationResourceDelta d
							INNER JOIN ReadModel.SkillCombination c ON d.SkillCombinationId = c.Id 
							INNER JOIN dbo.Skill s ON c.SkillId = s.Id
							WHERE d.InsertedOn < @dataLoaded and s.businessunit = @buid", connection, transaction))
						{
							deleteCommand.Parameters.AddWithValue("@buid", bu);
							deleteCommand.Parameters.AddWithValue("@dataLoaded", dataLoaded);
							deleteCommand.ExecuteNonQuery();
						}

						using (var deleteCommand = new SqlCommand(@"DELETE FROM [ReadModel].[SkillCombinationResource] 
						WHERE businessunit = @buid", connection, transaction))
						{
							deleteCommand.Parameters.AddWithValue("@buid", bu);
							deleteCommand.ExecuteNonQuery();
						}

						using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
						{
							sqlBulkCopy.DestinationTableName = "[ReadModel].[SkillCombinationResource]";
							sqlBulkCopy.WriteToServer(dt);
						}
						transaction.Commit();
					}
					
				}
			}
		}



		public IEnumerable<SkillCombinationResource> LoadSkillCombinationResources(DateTimePeriod period)
		{
			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			var result = _currentUnitOfWork.Current().Session()
				.CreateSQLQuery(
					@"SELECT r.SkillCombinationId, r.StartDateTime, r.EndDateTime, 
(CASE WHEN (r.Resource + ISNULL(SUM(d.DeltaResource), 0)) <= 0 THEN 0 ELSE r.Resource + ISNULL(SUM(d.DeltaResource), 0) END) as Resource, c.SkillId from 
[ReadModel].[SkillCombinationResource] r INNER JOIN [ReadModel].[SkillCombination] c ON c.Id = r.SkillCombinationId 
LEFT JOIN [ReadModel].[SkillCombinationResourceDelta] d ON d.SkillCombinationId = r.SkillCombinationId AND d.StartDateTime = r.StartDateTime
 WHERE r.StartDateTime < :endDateTime AND r.EndDateTime > :startDateTime AND r.BusinessUnit = :bu GROUP BY r.SkillCombinationId, r.StartDateTime, r.EndDateTime, r.Resource, c.SkillId Order By r.SkillCombinationId, c.SkillId")
				.SetDateTime("startDateTime", period.StartDateTime)
				.SetDateTime("endDateTime", period.EndDateTime)
				.SetParameter("bu", bu)
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(RawSkillCombinationResource)))
				.List<RawSkillCombinationResource>();

			var mergedResult =
				result.GroupBy(x => new {x.SkillCombinationId, x.StartDateTime, x.EndDateTime, x.Resource})
					.Select(
						x =>
							new SkillCombinationResourceWithCombinationId
							{
								StartDateTime = x.Key.StartDateTime.Utc(),
								EndDateTime = x.Key.EndDateTime.Utc(),
								Resource = x.Key.Resource,
								SkillCombinationId = x.Key.SkillCombinationId,
								SkillCombination = x.Select(s => s.SkillId).OrderBy(s => s)
							});

			return mergedResult;
		}

		public void PersistChanges(IEnumerable<SkillCombinationResource> deltas)
		{
			if (!deltas.Any()) return;
			_retryPolicy.ExecuteAction(() =>
			{
				tryPersistChanges(deltas);
			});
		}

	    private void tryPersistChanges(IEnumerable<SkillCombinationResource> deltas)
	    {
	        var reliableConnection = (ReliableSqlDbConnection) _currentUnitOfWork.Current().Session().Connection;
	        var connection = reliableConnection.ReliableConnection.Current;

			var dt = new DataTable();
			dt.Columns.Add("SkillCombinationId", typeof(Guid));
			dt.Columns.Add("StartDateTime", typeof(DateTime));
			dt.Columns.Add("EndDateTime", typeof(DateTime));
			dt.Columns.Add("InsertedOn", typeof(DateTime));
			dt.Columns.Add("DeltaResource", typeof(double));

			var skillCombinations = loadSkillCombination(connection, null);

			foreach (var delta in deltas)
	        {
	            Guid id;
	            if (!skillCombinations.TryGetValue(keyFor(delta.SkillCombination), out id)) continue;

	            var row = dt.NewRow();
	            row["SkillCombinationId"] = id;
	            row["StartDateTime"] = delta.StartDateTime;
	            row["EndDateTime"] = delta.EndDateTime;
	            row["InsertedOn"] = _now.UtcDateTime();
				row["DeltaResource"] = delta.Resource;

				dt.Rows.Add(row);

				var numberResources =
					_currentUnitOfWork.Current()
						.Session()
						.CreateSQLQuery(
							"SELECT 1 FROM [ReadModel].[SkillCombinationResource] WHERE StartDateTime = :StartDateTime AND SkillCombinationId = :id")
						.SetParameter("StartDateTime", delta.StartDateTime)
						.SetParameter("id", id)
						.UniqueResult<int?>();

				if (numberResources.HasValue) continue;

				var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
				var lastUpdated = GetLastCalculatedTime();

				_currentUnitOfWork.Current().Session()
					.CreateSQLQuery(@"
					               		INSERT INTO [ReadModel].[SkillCombinationResource] (SkillCombinationId, StartDateTime, EndDateTime, Resource, InsertedOn, BusinessUnit)
					               		VALUES (:SkillCombinationId, :StartDateTime, :EndDateTime, :Resource, :InsertedOn, :BusinessUnit)")
					.SetParameter("SkillCombinationId", id)
					.SetParameter("StartDateTime", delta.StartDateTime)
					.SetParameter("EndDateTime", delta.EndDateTime)
					.SetParameter("Resource", 0)
					.SetParameter("InsertedOn", lastUpdated)
					.SetParameter("BusinessUnit", bu)
					.ExecuteUpdate();
			}
	        using (var cmd = new SqlCommand())
	        {
	            _currentUnitOfWork.Current().Session().Transaction.Enlist(cmd);

	            using (
	                var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, cmd.Transaction))
	            {
	                bulk.DestinationTableName = "[ReadModel].[SkillCombinationResourceDelta]";
	                bulk.WriteToServer(dt);
	            }
	        }
	    }

	    public DateTime GetLastCalculatedTime()
		{
			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			var latest = _currentUnitOfWork.Current().Session()
				.CreateSQLQuery("SELECT top(1) InsertedOn from [ReadModel].SkillCombinationResource Where BusinessUnit = :bu order by InsertedOn desc ")
				.SetParameter("bu", bu)
				.UniqueResult<DateTime>();

			return latest;
		}

		public void PersistSkillCombinationResourceBpo(DateTime utcDateTime, List<ImportSkillCombinationResourceBpo> combinationResources)
		{
			throw new NotImplementedException();
		}

		public Dictionary<Guid, string> LoadSourceBpo(SqlConnection connection)
		{
			throw new NotImplementedException();
		}

		public IList<SkillCombinationResourceBpo> LoadBpoSkillCombinationResources()
		{
			throw new NotImplementedException();
		}
	}

}