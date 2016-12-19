using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NHibernate;
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

		public void PersistSkillCombination(IEnumerable<IEnumerable<Guid>> multipleSkillCombination)
		{
			
			var dt = new DataTable();
			dt.Columns.Add("Id", typeof(Guid));
			dt.Columns.Add("SkillId", typeof(Guid));
			dt.Columns.Add("InsertedOn", typeof(DateTime));

			var insertedOn = _now.UtcDateTime();

			foreach (var skillCombination  in multipleSkillCombination)
			{
				var combinationId = LoadSkillCombination(skillCombination);
				if(combinationId.HasValue) continue;
				combinationId = Guid.NewGuid();
				foreach (var skill in skillCombination)
				{
					var row = dt.NewRow();
					row["SkillId"] = skill;
					row["Id"] = combinationId;
					row["InsertedOn"] = insertedOn;
					dt.Rows.Add(row);
				}
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
			if (result.Count() > 1)
				return Guid.Empty; //log ERROR as something was wrong and log the combination ids the that we got from db
			if (result.Any())
				return result.FirstOrDefault();
			
				
			return null;
		}
	}

	public interface ISkillCombinationResourceRepository
	{
		void PersistSkillCombination(IEnumerable<IEnumerable<Guid>> skillCombination);
		Guid? LoadSkillCombination(IEnumerable<Guid> skillCombination);
	}
}