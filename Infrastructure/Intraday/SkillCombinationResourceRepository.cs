using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using NHibernate.Util;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Intraday
{
	public class SkillCombinationResourceRepository : ISkillCombinationResourceRepository
	{
		private readonly INow _now;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public SkillCombinationResourceRepository(INow now, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_now = now;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
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
			var result = ((NHibernateUnitOfWork) _currentUnitOfWorkFactory.Current().CurrentUnitOfWork()).Session.CreateSQLQuery(
					@"select Id, SkillId from [ReadModel].[SkillCombination]")
					.SetResultTransformer(Transformers.AliasToBean<internalSkillCombination>())
				.List<internalSkillCombination>();

			var dictionary = result.GroupBy(x => x.Id).ToDictionary(k => string.Join("_", k.Select(x => x.SkillId)), v => v.Key);
			return dictionary;
		}

		private class internalSkillCombination
		{
			public Guid Id { get; set; }
			public Guid SkillId { get; set; }
		}
		
		public virtual void PersistSkillCombinationResource(IEnumerable<SkillCombinationResource> skillCombinationResources)
		{
			var insertedOn = _now.UtcDateTime();

			var skillCombinations = loadSkillCombination();
			var dt = new DataTable();
			dt.Columns.Add("SkillCombinationId", typeof(Guid));
			dt.Columns.Add("StartDateTime", typeof(DateTime));
			dt.Columns.Add("EndDateTime", typeof(DateTime));
			dt.Columns.Add("Resource", typeof(double));
			dt.Columns.Add("InsertedOn", typeof(DateTime));

			foreach (var skillCombinationResource in skillCombinationResources)
			{
				Guid id;
				if (!skillCombinations.TryGetValue(string.Join("_", skillCombinationResource.SkillCombination), out id))
				{
					id = persistSkillCombination(skillCombinationResource.SkillCombination);
					skillCombinations.Add(string.Join("_", skillCombinationResource.SkillCombination), id);
				}
				
				
				var row = dt.NewRow();
				row["SkillCombinationId"] = id;
				row["StartDateTime"] = skillCombinationResource.StartDateTime;
				row["EndDateTime"] = skillCombinationResource.EndDateTime;
				row["Resource"] = skillCombinationResource.Resource;
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
						sqlBulkCopy.DestinationTableName = "[ReadModel].[SkillCombinationResource]";
						sqlBulkCopy.WriteToServer(dt);
					}

					var skillCombinationIds = new HashSet<Guid>();
					foreach (var skillCombinationResource in skillCombinationResources)
					{
						Guid id;
						if (skillCombinations.TryGetValue(string.Join("_", skillCombinationResource.SkillCombination), out id))
						{
							skillCombinationIds.Add(id);
						}
					}

					var deleteCommandForChanges = new SqlCommand();
					deleteCommandForChanges.CommandText = $@"
DELETE FROM ReadModel.DeltaSkillCombinationResource
WHERE InsertedOn < @timeWhenResourceCalcDataLoaded
AND SkillCombinationId IN ({AddArrayParameters(deleteCommandForChanges, skillCombinationIds.ToArray(), "ids")})";
					deleteCommandForChanges.Connection = connection;
					deleteCommandForChanges.Transaction = transaction;
					deleteCommandForChanges.Parameters.AddWithValue("@timeWhenResourceCalcDataLoaded", insertedOn);
					deleteCommandForChanges.ExecuteNonQuery();

					transaction.Commit();
				}

			}
		}

		public IEnumerable<SkillCombinationResource> LoadSkillCombinationResources(DateTimePeriod period)
		{
			var result = ((NHibernateUnitOfWork) _currentUnitOfWorkFactory.Current().CurrentUnitOfWork()).Session.CreateSQLQuery(
				@"SELECT r.SkillCombinationId, r.StartDateTime, r.EndDateTime, r.Resource, c.SkillId from 
					[ReadModel].[SkillCombinationResource] r INNER JOIN [ReadModel].[SkillCombination] c ON c.Id = r.SkillCombinationId WHERE StartDateTime < :endDateTime AND EndDateTime > :startDateTime") // ORDER BY r.SkillCombinationId,r.StartDateTime,c.SkillId
					.SetDateTime("startDateTime",period.StartDateTime)
					.SetDateTime("endDateTime",period.EndDateTime)
					.SetResultTransformer(new AliasToBeanResultTransformer(typeof(RawSkillCombinationResource)))
					.List<RawSkillCombinationResource>();
			foreach (var rawSkillCombinationResource in result)
			{
				var delta = ((NHibernateUnitOfWork) _currentUnitOfWorkFactory.Current().CurrentUnitOfWork()).Session
					.CreateSQLQuery("SELECT COUNT(*) FROM [ReadModel].[DeltaSkillCombinationResource] WHERE SkillCombinationId = :skillCombinationId AND StartDateTime = :start AND EndDateTime = :end")
					.SetParameter("skillCombinationId", rawSkillCombinationResource.SkillCombinationId)
					.SetParameter("start", rawSkillCombinationResource.StartDateTime)
					.SetParameter("end", rawSkillCombinationResource.EndDateTime)
					.UniqueResult<int>();
				rawSkillCombinationResource.Resource -= delta;
			}

			return result.GroupBy(x => new {x.SkillCombinationId, x.StartDateTime, x.EndDateTime, x.Resource}).Select(x => new SkillCombinationResource {StartDateTime = x.Key.StartDateTime, EndDateTime = x.Key.EndDateTime, Resource = x.Key.Resource, SkillCombination = x.Select(s => s.SkillId).OrderBy(s => s)});
		}

		public void PersistChange(SkillCombinationResource skillCombinationResource)
		{
			var skillCombinations = loadSkillCombination();
			Guid id;
			if (skillCombinations.TryGetValue(string.Join("_", skillCombinationResource.SkillCombination), out id))
			{
				((NHibernateUnitOfWork) _currentUnitOfWorkFactory.Current().CurrentUnitOfWork()).Session
					.CreateSQLQuery("INSERT INTO [ReadModel].[DeltaSkillCombinationResource] (SkillCombinationId, StartDateTime, EndDateTime, InsertedOn) VALUES (:SkillCombinationId, :StartDateTime, :EndDateTime, CURRENT_TIMESTAMP)")
					.SetParameter("SkillCombinationId", id)
					.SetParameter("StartDateTime", skillCombinationResource.StartDateTime)
					.SetParameter("EndDateTime", skillCombinationResource.EndDateTime)
					.ExecuteUpdate();
			}

		}

		protected string AddArrayParameters(SqlCommand sqlCommand, Guid[] array, string paramName)
		{
			var parameters = new string[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				parameters[i] = string.Format("@{0}{1}", paramName, i);
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


	public class SkillCombinationResourceRepositoryEmpty : SkillCombinationResourceRepository
	{
		public SkillCombinationResourceRepositoryEmpty(INow now, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory) : base(now, currentUnitOfWorkFactory)
		{
		}

		public override void PersistSkillCombinationResource(IEnumerable<SkillCombinationResource> skillCombinationResources)
		{
			
		}
	}

}