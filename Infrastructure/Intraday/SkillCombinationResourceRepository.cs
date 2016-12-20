﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
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

		private Dictionary<Guid[], Guid> loadSkillCombination()
		{
			var result = ((NHibernateUnitOfWork) _currentUnitOfWorkFactory.Current().CurrentUnitOfWork()).Session.CreateSQLQuery(
					@"select Id, SkillId from [ReadModel].[SkillCombination]")
				.List<Tuple<Guid, Guid>>();

			var dictionary = result.GroupBy(x => x.Item1).ToDictionary(k => k.Select(x => x.Item2).ToArray(), v => v.Key);
			return dictionary;
		}



		public Guid? LoadSkillCombination(IEnumerable<Guid> skillCombination)
		{
			var result = ((NHibernateUnitOfWork) _currentUnitOfWorkFactory.Current().CurrentUnitOfWork()).Session.CreateSQLQuery(
					@"   select  outerTable.Id  from 
					[ReadModel].[SkillCombination]  outerTable
			join	(select Id, count(Id) c from [ReadModel].[SkillCombination] group by Id) as innerTable
			on outerTable.Id = innerTable.Id
			where outerTable.SkillId in (:skillIds)
			and innerTable.c = :skillCount
			group by outerTable.Id
			having count(*)  = :skillCount")
				.AddScalar("Id", NHibernateUtil.Guid)
				.SetInt32("skillCount", skillCombination.ToList().Count())
				.SetParameterList("skillIds", skillCombination.ToArray())
				.List<Guid>();
			//if the result if more than one may be we could delete all  records with those combination ids
			//we found out while writing a test ( it should not happen though)
			if (result.Any())
				return result.FirstOrDefault();
			
			return null;
		}

		public void PersistSkillCombinationResource(IEnumerable<SkillCombinationResource> skillCombinationResources)
		{
			var skillCombinations = loadSkillCombination();
			var dt = new DataTable();
			dt.Columns.Add("SkillCombinationId", typeof(Guid));
			dt.Columns.Add("StartDateTime", typeof(DateTime));
			dt.Columns.Add("EndDateTime", typeof(DateTime));
			dt.Columns.Add("Resource", typeof(double));
			dt.Columns.Add("InsertedOn", typeof(DateTime));
			var insertedOn = _now.UtcDateTime();

			foreach (var skillCombinationResource in skillCombinationResources)
			{
				Guid id;
				if (!skillCombinations.TryGetValue(skillCombinationResource.SkillCombination.ToArray(), out id))
				{
					id = persistSkillCombination(skillCombinationResource.SkillCombination);
					skillCombinations.Add(skillCombinationResource.SkillCombination.ToArray(), id);
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


			return result.GroupBy(x => new {x.SkillCombinationId, x.StartDateTime, x.EndDateTime, x.Resource}).Select(x => new SkillCombinationResource {StartDateTime = x.Key.StartDateTime, EndDateTime = x.Key.EndDateTime, Resource = x.Key.Resource, SkillCombination = x.Select(s => s.SkillId).OrderBy(s => s)});
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
}