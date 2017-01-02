using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Intraday
{
	public class SkillCombinationResourceRepository : ISkillCombinationResourceRepository
	{
		private readonly INow _now;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		public SkillCombinationResourceRepository(INow now, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, ICurrentBusinessUnit currentBusinessUnit)
		{
			_now = now;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_currentBusinessUnit = currentBusinessUnit;
		}

		private Guid persistSkillCombination(IEnumerable<Guid> skillCombination)
		{

			var dt = new DataTable();
			dt.Columns.Add("Id", typeof(Guid));
			dt.Columns.Add("SkillId", typeof(Guid));
			dt.Columns.Add("InsertedOn", typeof(DateTime));

			var insertedOn = _now.UtcDateTime();

			var combinationId = Guid.NewGuid();
			foreach (var skill in skillCombination)
			{
				var row = dt.NewRow();
				row["SkillId"] = skill;
				row["Id"] = combinationId;
				row["InsertedOn"] = insertedOn;
				dt.Rows.Add(row);
			}


			var connectionString = _currentUnitOfWorkFactory.Current().ConnectionString;

			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction())
				{
					using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
					{
						sqlBulkCopy.DestinationTableName = "[ReadModel].[SkillCombination]";
						sqlBulkCopy.WriteToServer(dt);
					}
					transaction.Commit();
				}

			}
			return combinationId;
		}

		private Dictionary<string, Guid> loadSkillCombination()
		{
			var result = _currentUnitOfWorkFactory.Current().CurrentUnitOfWork().Session()
				.CreateSQLQuery("select Id, SkillId from [ReadModel].[SkillCombination]")
				.SetResultTransformer(Transformers.AliasToBean<internalSkillCombination>())
				.List<internalSkillCombination>();

			var dictionary = result.GroupBy(x => x.Id).ToDictionary(k => keyFor(k.Select(x => x.SkillId)), v => v.Key);
			return dictionary;
		}

		private static string keyFor(IEnumerable<Guid> skillIds)
		{
			return string.Join("_", skillIds.OrderBy(x => x));
		}

		private class internalSkillCombination
		{
			public Guid Id { get; set; }
			public Guid SkillId { get; set; }
		}

		public virtual void PersistSkillCombinationResource(IEnumerable<SkillCombinationResource> skillCombinationResources)
		{
			var insertedOn = _now.UtcDateTime();
			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			var skillCombinations = loadSkillCombination();

			var session = _currentUnitOfWorkFactory.Current().CurrentUnitOfWork().Session();
			foreach (var skillCombinationResource in skillCombinationResources)
			{
				var key = keyFor(skillCombinationResource.SkillCombination);
				Guid id;
				if (!skillCombinations.TryGetValue(key, out id))
				{
					id = persistSkillCombination(skillCombinationResource.SkillCombination);
					skillCombinations.Add(key, id);
				}

				session.Transaction.Begin();
				var updated = session.CreateSQLQuery(@"
						UPDATE [ReadModel].[SkillCombinationResource]
						SET Resource = Resource + :Resource, InsertedOn = :InsertedOn
						WHERE SkillCombinationId = :SkillCombinationId
						AND StartDateTime = :StartDateTime")
					.SetParameter("SkillCombinationId", id)
					.SetParameter("InsertedOn", insertedOn)
					.SetParameter("StartDateTime", skillCombinationResource.StartDateTime)
					.SetParameter("Resource", skillCombinationResource.Resource)
					.ExecuteUpdate();

				if (updated == 0)
				{
					session.CreateSQLQuery(@"
							INSERT INTO [ReadModel].[SkillCombinationResource] (SkillCombinationId, StartDateTime, EndDateTime, Resource, InsertedOn, BusinessUnit)
							VALUES (:SkillCombinationId, :StartDateTime, :EndDateTime, :Resource, :InsertedOn, :BusinessUnit)")
						.SetParameter("SkillCombinationId", id)
						.SetParameter("StartDateTime", skillCombinationResource.StartDateTime)
						.SetParameter("EndDateTime", skillCombinationResource.EndDateTime)
						.SetParameter("Resource", skillCombinationResource.Resource)
						.SetParameter("InsertedOn", insertedOn)
						.SetParameter("BusinessUnit", bu)
						.ExecuteUpdate();
				}
			}

			var skillCombinationIds = new HashSet<Guid>();
			foreach (var skillCombinationResource in skillCombinationResources)
			{
				Guid id;
				if (skillCombinations.TryGetValue(keyFor(skillCombinationResource.SkillCombination), out id))
				{
					skillCombinationIds.Add(id);
				}
			}

			foreach (var ids in skillCombinationIds.Batch(1000))
			{
				if (skillCombinationIds.Any())
				{
					session.CreateSQLQuery(@"
						DELETE FROM ReadModel.SkillCombinationResourceDelta
						WHERE InsertedOn < :InsertedOn
						AND SkillCombinationId IN (:SkillCombinationIds)")
						.SetParameter("InsertedOn", insertedOn)
						.SetParameterList("SkillCombinationIds", ids)
						.ExecuteUpdate();
				}
			}
		}

		public IEnumerable<SkillCombinationResource> LoadSkillCombinationResources(DateTimePeriod period)
		{
			var result = _currentUnitOfWorkFactory.Current().CurrentUnitOfWork().Session()
				.CreateSQLQuery(
					@"SELECT r.SkillCombinationId, r.StartDateTime, r.EndDateTime, r.Resource, c.SkillId from 
					[ReadModel].[SkillCombinationResource] r INNER JOIN [ReadModel].[SkillCombination] c ON c.Id = r.SkillCombinationId WHERE StartDateTime < :endDateTime AND EndDateTime > :startDateTime") // ORDER BY r.SkillCombinationId,r.StartDateTime,c.SkillId
				.SetDateTime("startDateTime", period.StartDateTime)
				.SetDateTime("endDateTime", period.EndDateTime)
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(RawSkillCombinationResource)))
				.List<RawSkillCombinationResource>();

			var mergedResult = result.GroupBy(x => new { x.SkillCombinationId, x.StartDateTime, x.EndDateTime, x.Resource }).Select(x => new SkillCombinationResourceWithCombinationId { StartDateTime = x.Key.StartDateTime, EndDateTime = x.Key.EndDateTime, Resource = x.Key.Resource, SkillCombinationId = x.Key.SkillCombinationId, SkillCombination = x.Select(s => s.SkillId).OrderBy(s => s) });
			var outputResult = new List<SkillCombinationResource>();
			foreach (var skillCombinationResource in mergedResult)
			{
				var delta = _currentUnitOfWorkFactory.Current().CurrentUnitOfWork().Session()
					.CreateSQLQuery("SELECT COUNT(*) FROM [ReadModel].[SkillCombinationResourceDelta] WHERE SkillCombinationId = :skillCombinationId AND StartDateTime = :start AND EndDateTime = :end")
					.SetParameter("skillCombinationId", skillCombinationResource.SkillCombinationId)
					.SetParameter("start", skillCombinationResource.StartDateTime)
					.SetParameter("end", skillCombinationResource.EndDateTime)
					.UniqueResult<int>();
				skillCombinationResource.Resource -= delta;
				outputResult.Add(skillCombinationResource);
			}

			return outputResult;
		}

        public IEnumerable<SkillCombinationResource> LoadSkillCombinationResourcesInOneQuery(DateTimePeriod period)
        {
            var result = _currentUnitOfWorkFactory.Current().CurrentUnitOfWork().Session()
                .CreateSQLQuery(
                    @"SELECT r.SkillCombinationId, r.StartDateTime, r.EndDateTime, r.Resource - ISNULL(COUNT(d.SkillCombinationId), 0) as Resource, c.SkillId from 
[ReadModel].[SkillCombinationResource] r INNER JOIN [ReadModel].[SkillCombination] c ON c.Id = r.SkillCombinationId 
LEFT JOIN [ReadModel].[SkillCombinationResourceDelta] d ON d.SkillCombinationId = r.SkillCombinationId AND d.StartDateTime = r.StartDateTime AND d.EndDateTime = r.EndDateTime
 WHERE r.StartDateTime < :endDateTime AND r.EndDateTime > :startDateTime GROUP BY r.SkillCombinationId, r.StartDateTime, r.EndDateTime, r.Resource, c.SkillId")
                .SetDateTime("startDateTime", period.StartDateTime)
                .SetDateTime("endDateTime", period.EndDateTime)
                .SetResultTransformer(new AliasToBeanResultTransformer(typeof(RawSkillCombinationResource)))
                .List<RawSkillCombinationResource>();

            var mergedResult =
                result.GroupBy(x => new {x.SkillCombinationId, x.StartDateTime, x.EndDateTime, x.Resource})
                    .Select(
                        x =>
                            new SkillCombinationResourceWithCombinationId
                            {
                                StartDateTime = x.Key.StartDateTime,
                                EndDateTime = x.Key.EndDateTime,
                                Resource = x.Key.Resource,
                                SkillCombinationId = x.Key.SkillCombinationId,
                                SkillCombination = x.Select(s => s.SkillId).OrderBy(s => s)
                            });

            return mergedResult;
        }

        public void PersistChange(SkillCombinationResource skillCombinationResource)
		{
			var skillCombinations = loadSkillCombination();
			Guid id;
			if (skillCombinations.TryGetValue(keyFor(skillCombinationResource.SkillCombination), out id))
			{
				_currentUnitOfWorkFactory.Current().CurrentUnitOfWork().Session()
					.CreateSQLQuery("INSERT INTO [ReadModel].[SkillCombinationResourceDelta] (SkillCombinationId, StartDateTime, EndDateTime, InsertedOn) VALUES (:SkillCombinationId, :StartDateTime, :EndDateTime, CURRENT_TIMESTAMP)")
					.SetParameter("SkillCombinationId", id)
					.SetParameter("StartDateTime", skillCombinationResource.StartDateTime)
					.SetParameter("EndDateTime", skillCombinationResource.EndDateTime)
					.ExecuteUpdate();
			}
		}

		public DateTime GetLastCalculatedTime(Guid buId)
		{
			var latest =_currentUnitOfWorkFactory.Current().CurrentUnitOfWork().Session()
							.CreateSQLQuery("SELECT top(1) InsertedOn from [ReadModel].SkillCombinationResource Where BusinessUnit = :bu order by InsertedOn desc ")
							.SetParameter("bu", buId)
							.UniqueResult<DateTime>();

			return latest;
		}

		protected string AddArrayParameters(SqlCommand sqlCommand, Guid[] array, string paramName)
		{
			var parameters = new string[array.Length];
			for (var i = 0; i < array.Length; i++)
			{
				parameters[i] = $"@{paramName}{i}";
				sqlCommand.Parameters.AddWithValue(parameters[i], array[i]);
			}

			return string.Join(", ", parameters);
		}
	}

	public class RawSkillCombinationResource
	{
		public Guid SkillCombinationId;
		public DateTime StartDateTime;
		public DateTime EndDateTime;
		public double Resource;
		public Guid SkillId;
	}

	public class SkillCombinationResourceWithCombinationId : SkillCombinationResource
	{
		public Guid SkillCombinationId;
	}


	public class SkillCombinationResourceRepositoryEmpty : SkillCombinationResourceRepository
	{
		public override void PersistSkillCombinationResource(IEnumerable<SkillCombinationResource> skillCombinationResources)
		{

		}

		public SkillCombinationResourceRepositoryEmpty(INow now, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, ICurrentBusinessUnit currentBusinessUnit) : base(now, currentUnitOfWorkFactory, currentBusinessUnit)
		{
		}
	}

}