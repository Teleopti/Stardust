using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Intraday
{
	public class ScheduleForecastSkillReadModelRepository : IScheduleForecastSkillReadModelRepository
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public ScheduleForecastSkillReadModelRepository(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public void Persist(IEnumerable<ResourcesDataModel> items, DateOnly date)
		{
			var dt = new DataTable();
			dt.Columns.Add("SkillId",typeof(Guid));
			dt.Columns.Add("BelongsToDate",typeof(DateTime));
			dt.Columns.Add("StartDateTime", typeof(DateTime));
			dt.Columns.Add("EndDateTime", typeof(DateTime));
			dt.Columns.Add("Forecast", typeof(double));
			dt.Columns.Add("StaffingLevel", typeof(double));


			foreach (var item in items)
			{
				foreach (var skillStaffingInterval in item.Intervals)
				{
					var row = dt.NewRow();
					row["SkillId"] = item.Id;
					row["BelongsToDate"] =  date.Date;
					row["StartDateTime"] = skillStaffingInterval.StartDateTime;
					row["EndDateTime"] = skillStaffingInterval.EndDateTime;
					row["Forecast"] = skillStaffingInterval.Forecast;
					row["StaffingLevel"] = skillStaffingInterval.StaffingLevel;
					dt.Rows.Add(row);
				}
			}

			var connectionString = _currentUnitOfWorkFactory.Current().ConnectionString;
			
			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();

				var deleteCommandstring = @"DELETE ReadModel.ScheduleForecastSkill  
										 WHERE BelongsToDate = @date";
				var deleteCommand = new SqlCommand(deleteCommandstring,connection);
				deleteCommand.Parameters.AddWithValue("@date", date.Date);
				deleteCommand.ExecuteNonQuery();

				using (var sqlBulkCopy = new SqlBulkCopy(connection))
				{
					sqlBulkCopy.DestinationTableName = "[ReadModel].[ScheduleForecastSkill]";
					sqlBulkCopy.WriteToServer(dt);
				}
			}
			

		}
		

		public IEnumerable<SkillStaffingInterval> GetBySkill(Guid skillId, DateTime startDateTime, DateTime endDateTime)
		{
			var result = ((NHibernateUnitOfWork) _currentUnitOfWorkFactory.Current().CurrentUnitOfWork()).Session.CreateSQLQuery(
				@"SELECT 
			[StartDateTime]
			,[EndDateTime]
			,[Forecast]
			,[StaffingLevel]
				 FROM [ReadModel].[ScheduleForecastSkill]
				where [startDateTime] between :startDateTime and :endDateTime
				and SkillId = :skillId")
				.AddScalar("StartDateTime", NHibernateUtil.DateTime)
				.AddScalar("EndDateTime", NHibernateUtil.DateTime)
				.AddScalar("Forecast", NHibernateUtil.Double)
				.AddScalar("StaffingLevel", NHibernateUtil.Double)
				.SetDateTime("startDateTime", startDateTime)
				.SetDateTime("endDateTime", endDateTime)
				.SetGuid("skillId", skillId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (SkillStaffingInterval)))
				.List<SkillStaffingInterval>();

			return result;
		}

		public IEnumerable<SkillStaffingInterval> GetBySkillArea(Guid id, DateTime startDateTime, DateTime endDateTime)
		{
			var result = ((NHibernateUnitOfWork)_currentUnitOfWorkFactory.Current().CurrentUnitOfWork()).Session.CreateSQLQuery(
				@"SELECT 
					StartDateTime,EndDateTime,Sum(Forecast) as Forecast,Sum(StaffingLevel) as StaffingLevel
				 FROM [ReadModel].[ScheduleForecastSkill]
				inner join SkillAreaSkillCollection on SkillId = Skill
				where [startDateTime] between :startDateTime and :endDateTime
				and SkillArea = :id
				group by  StartDateTime, EndDateTime")
				.AddScalar("StartDateTime", NHibernateUtil.DateTime)
				.AddScalar("EndDateTime", NHibernateUtil.DateTime)
				.AddScalar("Forecast", NHibernateUtil.Double)
				.AddScalar("StaffingLevel", NHibernateUtil.Double)
				.SetDateTime("startDateTime", startDateTime)
				.SetDateTime("endDateTime", endDateTime)
				.SetGuid("id", id)
				.SetResultTransformer(Transformers.AliasToBean(typeof(SkillStaffingInterval)))
				.List<SkillStaffingInterval>();

			return result;
		}
	}
}
