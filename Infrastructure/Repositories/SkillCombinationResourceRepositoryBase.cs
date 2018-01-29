using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using NHibernate.SqlAzure;
using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
	public abstract class SkillCombinationResourceRepositoryBase : ISkillCombinationResourceRepository, ISkillCombinationResourceReader
	{
		private readonly INow _now;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly RetryPolicy _retryPolicy;
		private readonly IStardustJobFeedback _stardustJobFeedback;

		protected SkillCombinationResourceRepositoryBase(INow now, ICurrentUnitOfWork currentUnitOfWork,
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
			
			using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.CheckConstraints, transaction))
			{
				sqlBulkCopy.DestinationTableName = "[ReadModel].[SkillCombination]";
				sqlBulkCopy.WriteToServer(dt);
			}

			return combinationId;
		}

		private Dictionary<GuidCombinationKey, Guid> loadSkillCombination(SqlConnection connection, SqlTransaction transaction)
		{
			var result = new List<internalSkillCombination>();
			using (var command = new SqlCommand("select Id, SkillId from [ReadModel].[SkillCombination]", connection, transaction))
			{

				if (transaction == null)
					_currentUnitOfWork.Current().Session().Transaction.Enlist(command);
				
				using (var reader = command.ExecuteReader())
				{
					if (!reader.HasRows) return new Dictionary<GuidCombinationKey, Guid>();
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

			return result.GroupBy(x => x.Id).GroupBy(x => keyFor(x.Select(y => y.SkillId)))
				.ToDictionary(k => k.Key, v => v.Select(y => y.Key).First());
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
		    if (!skillCombinationResources.Any()) return;
			_retryPolicy.ExecuteAction(() =>
			{
				tryPersistSkillCombinationResource(dataLoaded,  skillCombinationResources);
			});
		}

		public abstract IEnumerable<SkillCombinationResource> LoadSkillCombinationResources(DateTimePeriod period,
			bool useBpoExchange = true);

		public Dictionary<Guid, string> LoadSourceBpo(SqlConnection connection)
		{
			var bpoList = new Dictionary<Guid, string>();
			using (var command = new SqlCommand("select Id, Source from [BusinessProcessOutsourcer]", connection))
			{
				using (var reader = command.ExecuteReader())
				{
					if (reader.HasRows)
						while (reader.Read())
							bpoList.Add(reader.GetGuid(0), reader.GetString(1));
				}
			}
			return bpoList;
		}

		

		public void PersistSkillCombinationResourceBpo(List<ImportSkillCombinationResourceBpo> combinationResources)
		{
			var connectionString = _currentUnitOfWork.Current().Session().Connection.ConnectionString;
			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				var bpoList = LoadSourceBpo(connection);

				var dt = new DataTable();
				dt.Columns.Add("SkillCombinationId", typeof(Guid));
				dt.Columns.Add("StartDateTime", typeof(DateTime));
				dt.Columns.Add("EndDateTime", typeof(DateTime));
				dt.Columns.Add("InsertedOn", typeof(DateTime));
				dt.Columns.Add("Resources", typeof(double));
				dt.Columns.Add("SourceId", typeof(Guid));
				dt.Columns.Add("BusinessUnit", typeof(Guid));
				var insertedOn = _now.UtcDateTime();

				using (var transaction = connection.BeginTransaction())
				{ 
					var skillCombinations = loadSkillCombination(connection, transaction);
					foreach (var skillCombinationResourceBpo in combinationResources)
					{
						var bpoCreated = false;
						var skillCombCreated = false;
						var key = keyFor(skillCombinationResourceBpo.SkillIds);
						Guid id;

						if (!skillCombinations.TryGetValue(key, out id))
						{
							skillCombCreated = true;
							id = persistSkillCombination(skillCombinationResourceBpo.SkillIds, connection, transaction);
							skillCombinations.Add(key, id);
						}
						var row = dt.NewRow();

						row["SkillCombinationId"] = id;
						row["StartDateTime"] = skillCombinationResourceBpo.StartDateTime;
						row["EndDateTime"] = skillCombinationResourceBpo.EndDateTime;
						row["InsertedOn"] = insertedOn;
						row["Resources"] = skillCombinationResourceBpo.Resources;

						var bpoId = Guid.NewGuid();
						if (bpoList.ContainsValue(skillCombinationResourceBpo.Source))
							bpoId = bpoList.First(x => x.Value == skillCombinationResourceBpo.Source).Key;
						else
						{
							bpoCreated = true;
							using (var insertCommand = new SqlCommand(@"insert into [BusinessProcessOutsourcer] (Id, Source) Values (@id,@source)", connection, transaction))
							{
								insertCommand.Parameters.AddWithValue("@id", bpoId);
								insertCommand.Parameters.AddWithValue("@source", skillCombinationResourceBpo.Source);
								//set lock time to null
								insertCommand.ExecuteNonQuery();
							}
							bpoList.Add(bpoId, skillCombinationResourceBpo.Source);
						}
						row["SourceId"] = bpoId;
						row["BusinessUnit"] = bu;

						if(!bpoCreated && !skillCombCreated)
						{
							removeExistingBpoInterval(id, bpoId, skillCombinationResourceBpo.StartDateTime);
						}
						dt.Rows.Add(row);
					}
					transaction.Commit();
				}

				using (var transaction = connection.BeginTransaction())
				{
					using (var deleteCommand = new SqlCommand(@"DELETE FROM [ReadModel].[SkillCombinationResourceBpo] 
						WHERE StartDateTime <= @8DaysAgo", connection, transaction))
					{
						deleteCommand.Parameters.AddWithValue("@buid", bu);
						deleteCommand.Parameters.AddWithValue("@8DaysAgo", _now.UtcDateTime().AddDays(-8));
						deleteCommand.ExecuteNonQuery();
					}

					using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
					{
						sqlBulkCopy.DestinationTableName = "[ReadModel].[SkillCombinationResourceBpo]";
						sqlBulkCopy.WriteToServer(dt);
					}
					transaction.Commit();
				}
			}
		}

		private void removeExistingBpoInterval(Guid id, Guid bpoId, DateTime startDateTime)
		{
			var connectionString = _currentUnitOfWork.Current().Session().Connection.ConnectionString;
			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction())
				{
					using (var deleteCommand = new SqlCommand(@"DELETE  FROM ReadModel.SkillCombinationResourceBpo 
							WHERE SkillCombinationId = @id
								AND StartDateTime = @startDateTime
								AND SourceId = @bpoId ", connection, transaction))
					{
						deleteCommand.Parameters.AddWithValue("@id", id);
						deleteCommand.Parameters.AddWithValue("@StartDateTime", startDateTime);
						deleteCommand.Parameters.AddWithValue("@bpoId", bpoId);
						deleteCommand.ExecuteNonQuery();
					}
					
					transaction.Commit();
				}
			}
			
		}

		private void tryPersistSkillCombinationResource(DateTime dataLoaded,
			IEnumerable<SkillCombinationResource> skillCombinationResources)
		{
			_stardustJobFeedback.SendProgress($"Data loaded on {dataLoaded}");
			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			_stardustJobFeedback.SendProgress(
				$"Start persist {skillCombinationResources.Count()} skillCombinationResources for bu {bu}.");

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

					var minStartDateTime = skillCombinationResources.Min(x => x.StartDateTime);
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
							WHERE d.InsertedOn < @dataLoaded and s.businessunit = @buid 
								and (StartDateTime >= @minNewResourceStartDateTime OR StartDateTime <= @8DaysAgo)", connection, transaction))
						{
							deleteCommand.Parameters.AddWithValue("@buid", bu);
							deleteCommand.Parameters.AddWithValue("@dataLoaded", dataLoaded);
							deleteCommand.Parameters.AddWithValue("@minNewResourceStartDateTime", minStartDateTime);
							deleteCommand.Parameters.AddWithValue("@8DaysAgo", dataLoaded.AddDays(-8));
							deleteCommand.ExecuteNonQuery();
						}
						_stardustJobFeedback.SendProgress($"Removing historical resources that is older than {dataLoaded.AddDays(-8)} or later than {minStartDateTime}");
						using (var deleteCommand = new SqlCommand(@"DELETE FROM [ReadModel].[SkillCombinationResource] 
						WHERE businessunit = @buid AND (StartDateTime >= @minNewResourceStartDateTime OR StartDateTime <= @8DaysAgo)", connection, transaction))
						{
							deleteCommand.Parameters.AddWithValue("@buid", bu);
							deleteCommand.Parameters.AddWithValue("@minNewResourceStartDateTime", minStartDateTime);
							deleteCommand.Parameters.AddWithValue("@8DaysAgo", dataLoaded.AddDays(-8));
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

		public IEnumerable<SkillCombinationResource> Execute(DateTimePeriod period)
		{
			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			var result = _currentUnitOfWork.Current().Session()
				.CreateSQLQuery(@"select SkillCombinationId,StartDateTime,EndDateTime,sum(Resources) as Resource, c.SkillId
								FROM ReadModel.SkillCombinationResourceBpo scrb 
								INNER JOIN [ReadModel].[SkillCombination] c ON c.Id = scrb.SkillCombinationId 
								where  scrb.StartDateTime < :endDateTime 
									AND scrb.EndDateTime > :startDateTime
									AND scrb.BusinessUnit = :bu
								group by SkillCombinationId,StartDateTime,EndDateTime,c.skillId")
				.SetDateTime("startDateTime", period.StartDateTime)
				.SetDateTime("endDateTime", period.EndDateTime)
				.SetParameter("bu", bu)
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(RawSkillCombinationResource)))
				.List<RawSkillCombinationResource>();

			var mergedResult =
				result.GroupBy(x => new { x.SkillCombinationId, x.StartDateTime, x.EndDateTime, x.Resource })
					.Select(
						x =>
							new SkillCombinationResourceWithCombinationId
							{
								StartDateTime = x.Key.StartDateTime.Utc(),
								EndDateTime = x.Key.EndDateTime.Utc(),
								Resource = x.Key.Resource,
								SkillCombinationId = x.Key.SkillCombinationId,
								SkillCombination = x.Select(s => s.SkillId).OrderBy(s => s).ToArray()
							});

			return mergedResult.ToArray();
		}
		
		protected IEnumerable<SkillCombinationResource> skillCombinationResourcesWithBpo(DateTimePeriod period)
		{
			var combinationResources = skillCombinationResourcesWithoutBpo(period).ToArray();
			
			var bpoResources = Execute(period).ToArray();
			if (!bpoResources.Any())
			{
				return combinationResources;
			}

			return combinationResources.Concat(bpoResources)
				.GroupBy(g => new {g.StartDateTime, g.EndDateTime, c = keyFor(g.SkillCombination)}).Select(x =>
					new SkillCombinationResource
					{
						StartDateTime = x.Key.StartDateTime.Utc(),
						EndDateTime = x.Key.EndDateTime.Utc(),
						Resource = x.Sum(y => y.Resource),
						SkillCombination = x.Key.c.First
					}).ToArray();
		}
		
		protected IEnumerable<SkillCombinationResource> skillCombinationResourcesWithoutBpo(DateTimePeriod period)
		{
			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			var result = _currentUnitOfWork.Current().Session()
				.CreateSQLQuery(
					@"

 SELECT  SkillCombinationId, StartDateTime, EndDateTime, Resource, c.SkillId
 FROM
(SELECT  SkillCombinationId, StartDateTime, EndDateTime, SUM(Resource) AS Resource FROM 
 (SELECT r.SkillCombinationId, r.StartDateTime, r.EndDateTime, r.Resource
 from [ReadModel].[SkillCombinationResource] r WHERE r.BusinessUnit = :bu
AND r.StartDateTime < :endDateTime AND r.EndDateTime > :startDateTime
 UNION ALL
 SELECT d.SkillCombinationId, d.StartDateTime, d.EndDateTime, d.DeltaResource AS Resource
 from [ReadModel].[SkillCombinationResourceDelta] d WHERE d.BusinessUnit = :bu
AND d.StartDateTime < :endDateTime AND d.EndDateTime > :startDateTime)
 AS tmp
 GROUP BY tmp.SkillCombinationId, tmp.StartDateTime, tmp.EndDateTime) AS summary
 INNER JOIN [ReadModel].[SkillCombination] c ON summary.SkillCombinationId = c.Id")

				.SetDateTime("startDateTime", period.StartDateTime)
				.SetDateTime("endDateTime", period.EndDateTime)
				.SetParameter("bu", bu)
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(RawSkillCombinationResource)))
				.List<RawSkillCombinationResource>();

			var mergedResult =
				result.GroupBy(x => new { x.SkillCombinationId, x.StartDateTime, x.EndDateTime, x.Resource })
					.Select(
						x =>
							new SkillCombinationResourceWithCombinationId
							{
								StartDateTime = x.Key.StartDateTime.Utc(),
								EndDateTime = x.Key.EndDateTime.Utc(),
								Resource = x.Key.Resource,
								SkillCombinationId = x.Key.SkillCombinationId,
								SkillCombination = x.Select(s => s.SkillId).OrderBy(s => s).ToArray()
							});

			return mergedResult;
		}
		
		public void PersistChanges(IEnumerable<SkillCombinationResource> deltas)
		{
			if (!deltas.Any()) return;
			_retryPolicy.ExecuteAction(() => { tryPersistChanges(deltas); });
		}

	    private void tryPersistChanges(IEnumerable<SkillCombinationResource> deltas)
	    {
	        var reliableConnection = (ReliableSqlDbConnection) _currentUnitOfWork.Current().Session().Connection;
	        var connection = reliableConnection.ReliableConnection.Current;
		    var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			var dt = new DataTable();
			dt.Columns.Add("SkillCombinationId", typeof(Guid));
			dt.Columns.Add("StartDateTime", typeof(DateTime));
			dt.Columns.Add("EndDateTime", typeof(DateTime));
			dt.Columns.Add("InsertedOn", typeof(DateTime));
			dt.Columns.Add("DeltaResource", typeof(double));
		    dt.Columns.Add("Id", typeof(Guid));
			dt.Columns.Add("BusinessUnit", typeof(Guid));

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
		        row["Id"] = Guid.NewGuid();
				row["BusinessUnit"] = bu;

				dt.Rows.Add(row);
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
	}

	public class RawSkillCombinationResource
	{
		public Guid SkillCombinationId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public double Resource { get; set; }
		public Guid SkillId { get; set; }
	}


	public class SkillCombinationResourceWithCombinationId : SkillCombinationResource
	{
		public Guid SkillCombinationId { get; set; }
	}

}
 