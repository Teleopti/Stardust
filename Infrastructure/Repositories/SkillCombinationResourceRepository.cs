using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Polly;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration.TransientErrorHandling;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SkillCombinationResourceRepository : ISkillCombinationResourceRepository, ISkillCombinationResourceReader
	{
		private readonly INow _now;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly Policy _retryPolicy;
		private readonly IStardustJobFeedback _stardustJobFeedback;
		private readonly UpdateStaffingLevelReadModelStartDate _updateStaffingLevelReadModelStartDate;

		public SkillCombinationResourceRepository(INow now, ICurrentUnitOfWork currentUnitOfWork,
			ICurrentBusinessUnit currentBusinessUnit, IStardustJobFeedback stardustJobFeedback, UpdateStaffingLevelReadModelStartDate updateStaffingLevelReadModelStartDate)
		{
			_now = now;
			_currentUnitOfWork = currentUnitOfWork;
			_currentBusinessUnit = currentBusinessUnit;
			_stardustJobFeedback = stardustJobFeedback;
			_updateStaffingLevelReadModelStartDate = updateStaffingLevelReadModelStartDate;
			//_removeDeletedStaffingHeads = removeDeletedStaffingHeads;
			_retryPolicy = Policy.Handle<TimeoutException>()
				.Or<SqlException>(DetectTransientSqlException.IsTransient)
				.OrInner<SqlException>(DetectTransientSqlException.IsTransient)
				.WaitAndRetry(5, i => TimeSpan.FromMilliseconds(100));
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
			_retryPolicy.Execute(() =>
			{
				tryPersistSkillCombinationResource(dataLoaded,  skillCombinationResources);
			});
		}

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

		public string GetSourceBpoByGuid(Guid bpoGuid)
		{
			var connectionString = _currentUnitOfWork.Current().Session().Connection.ConnectionString;
			string bpoSource = "";
			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (var command = new SqlCommand("select Source from [BusinessProcessOutsourcer] where Id = @guid ", connection))
				{
					command.Parameters.AddWithValue("guid", bpoGuid);
					using (var reader = command.ExecuteReader())
					{
						if (reader.HasRows)
							while (reader.Read())
								bpoSource = reader.GetString(0);
					}
				}
				connection.Close();
			}
			return bpoSource;
		}

		public void PersistSkillCombinationResourceBpo(List<ImportSkillCombinationResourceBpo> combinationResources)
		{
			var connectionString = _currentUnitOfWork.Current().Session().Connection.ConnectionString;
			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				var bpoDictionary = LoadSourceBpo(connection);

				var dt = new DataTable();
				dt.Columns.Add("SkillCombinationId", typeof(Guid));
				dt.Columns.Add("StartDateTime", typeof(DateTime));
				dt.Columns.Add("EndDateTime", typeof(DateTime));
				dt.Columns.Add("InsertedOn", typeof(DateTime));
				dt.Columns.Add("Resources", typeof(double));
				dt.Columns.Add("SourceId", typeof(Guid));
				dt.Columns.Add("BusinessUnit", typeof(Guid));
				dt.Columns.Add("ImportFilename", typeof(string));
				dt.Columns.Add("PersonId", typeof(Guid));
				var insertedOn = _now.UtcDateTime();

				using (var transaction = connection.BeginTransaction())
				{ 
					var skillCombinations = loadSkillCombination(connection, transaction);
					foreach (var skillCombinationResourceBpo in combinationResources)
					{
						skillCombinationResourceBpo.Source = skillCombinationResourceBpo.Source.Trim();
						var bpoCreated = false;
						var skillCombCreated = false;
						var key = keyFor(skillCombinationResourceBpo.SkillIds);

						if (!skillCombinations.TryGetValue(key, out var id))
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
						row["ImportFilename"] = skillCombinationResourceBpo.ImportFileName;
						row["PersonId"] = skillCombinationResourceBpo.PersonId;

						var bpoId = Guid.NewGuid();

						var tuple = bpoDictionary.FirstOrDefault(x =>
							string.Equals(x.Value, skillCombinationResourceBpo.Source, StringComparison.OrdinalIgnoreCase));
						
						if (tuple.Value != null)
							bpoId = tuple.Key;
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
							bpoDictionary.Add(bpoId, skillCombinationResourceBpo.Source);
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
					_retryPolicy.Execute(connection.Open);
					var dt = new DataTable();
					dt.Columns.Add("SkillCombinationId", typeof(Guid));
					dt.Columns.Add("StartDateTime", typeof(DateTime));
					dt.Columns.Add("EndDateTime", typeof(DateTime));
					dt.Columns.Add("Resource", typeof(double));
					dt.Columns.Add("InsertedOn", typeof(DateTime));
					dt.Columns.Add("BusinessUnit", typeof(Guid));
					var insertedOn = _now.UtcDateTime();

					//var minStartDateTime = _removeDeletedStaffingHeads.GetStartDate(skillCombinationResources.Min(x => x.StartDateTime));
					var minStartDateTime = _updateStaffingLevelReadModelStartDate.StartDateTime;
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

		public IEnumerable<ScheduledHeads> ScheduledHeadsForSkill(Guid skillId, DateOnlyPeriod period)
		{
			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			var result = _currentUnitOfWork.Current().Session()
				.CreateSQLQuery(@"select StartDateTime,EndDateTime, SUM(Resource) Heads
			FROM ReadModel.SkillCombinationResource scrb 
			INNER JOIN [ReadModel].[SkillCombination] c ON c.Id = scrb.SkillCombinationId 
								WHERE c.SkillId = :skillId
								AND scrb.StartDateTime <= :endDateTime 
									AND scrb.EndDateTime >= :startDateTime
									AND scrb.BusinessUnit = :bu
								GROUP BY StartDateTime,EndDateTime")
				.SetDateTime("startDateTime", period.StartDate.Date)
				.SetDateTime("endDateTime", period.EndDate.Date)
				.SetParameter("bu", bu)
				.SetGuid("skillId", skillId)
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(ScheduledHeads)))
				.List<ScheduledHeads>();

			return result.ToArray();
		}

		public IEnumerable<ActiveBpoModel> LoadActiveBpos()
		{
			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			var result = _currentUnitOfWork.Current().Session()
				.CreateSQLQuery(@"SELECT Id,Source FROM BusinessProcessOutsourcer WHERE Id IN 
								(SELECT SourceId FROM ReadModel.SkillCombinationResourceBpo WHERE BusinessUnit = :bu)")
				.SetGuid("bu",bu)
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(ActiveBpoModel)))
				.List<ActiveBpoModel>();
			return result;
		}

		public int ClearBpoResources(Guid bpoGuid, DateTimePeriod dateTimePeriod)
		{
			return _currentUnitOfWork.Current().Session()
				.CreateSQLQuery(@"delete from ReadModel.SkillCombinationResourceBpo where sourceid = :bpoID and StartDateTime >= :st and EndDateTime <= :end")
				.SetGuid("bpoID", bpoGuid)
				.SetDateTime("st",dateTimePeriod.StartDateTime)
				.SetDateTime("end",dateTimePeriod.EndDateTime)
				.ExecuteUpdate();
		}

		public BpoResourceRangeRaw GetRangeForBpo(Guid bpoId)
		{
			return _currentUnitOfWork.Current().Session()
				.CreateSQLQuery(@"select MIN(StartDateTime) AS StartDate, MAX(EndDateTime)  AS EndDate  from ReadModel.SkillCombinationResourceBpo where SourceId  = :id")
				.SetGuid("id", bpoId)
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(BpoResourceRangeRaw))).UniqueResult<BpoResourceRangeRaw>();
		}

		public IEnumerable<SkillCombinationResourceForBpo> BpoResourcesForSkill(Guid skillId, DateOnlyPeriod period)
		{
			var extendedEndDate = period.EndDate.Date.AddDays(1).AddMinutes(-1);
			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			var result = _currentUnitOfWork.Current().Session()
				.CreateSQLQuery(@"select b.[Source],StartDateTime,EndDateTime, SUM(Resources) Resource
								FROM ReadModel.SkillCombinationResourceBpo scrb 
								INNER JOIN [ReadModel].[SkillCombination] c ON c.Id = scrb.SkillCombinationId 
								INNER JOIN BusinessProcessOutsourcer b ON b.Id = scrb.SourceId
								WHERE c.SkillId = :skillId
								AND scrb.StartDateTime <= :endDateTime 
									AND scrb.EndDateTime >= :startDateTime
									AND scrb.BusinessUnit = :bu
								GROUP BY StartDateTime,EndDateTime, b.[Source]")
				.SetDateTime("startDateTime", period.StartDate.Date)
				.SetDateTime("endDateTime", extendedEndDate)
				.SetParameter("bu", bu)
				.SetGuid("skillId", skillId)
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(SkillCombinationResourceForBpo)))
				.List<SkillCombinationResourceForBpo>();

			return result.ToArray();
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
								Resource = x.Key.Resource < 0 ? 0 : x.Key.Resource,
								SkillCombinationId = x.Key.SkillCombinationId,
								SkillCombination = x.Select(s => s.SkillId).OrderBy(s => s).ToArray()
							});

			return mergedResult;
		}
		
		public void PersistChanges(IEnumerable<SkillCombinationResource> deltas)
		{
			if (!deltas.Any()) return;
			_retryPolicy.Execute(() => { tryPersistChanges(deltas); });
		}

	    private void tryPersistChanges(IEnumerable<SkillCombinationResource> deltas)
		{
			var session = _currentUnitOfWork.Current().Session();
			var conn = session.Connection.Unwrap();

			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			var dt = new DataTable();
			dt.Columns.Add("SkillCombinationId", typeof(Guid));
			dt.Columns.Add("StartDateTime", typeof(DateTime));
			dt.Columns.Add("EndDateTime", typeof(DateTime));
			dt.Columns.Add("InsertedOn", typeof(DateTime));
			dt.Columns.Add("DeltaResource", typeof(double));
		    dt.Columns.Add("Id", typeof(Guid));
			dt.Columns.Add("BusinessUnit", typeof(Guid));

			var skillCombinations = loadSkillCombination(conn, null);

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
	            session.Transaction.Enlist(cmd);

	            using (
	                var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, cmd.Transaction))
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

		public IEnumerable<SkillCombinationResource> LoadSkillCombinationResources(DateTimePeriod period, bool useBpoExchange = true)
		{
			return useBpoExchange ? skillCombinationResourcesWithBpo(period) : skillCombinationResourcesWithoutBpo(period);
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
 