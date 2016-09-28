using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
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

		public void Persist(IEnumerable<ResourcesDataModel> items)
		{
			var dt = new DataTable();
			dt.Columns.Add("SkillId",typeof(Guid));
			dt.Columns.Add("BelongsToDate",typeof(DateTime));
			dt.Columns.Add("StartDateTime", typeof(DateTime));
			dt.Columns.Add("EndDateTime", typeof(DateTime));
            dt.Columns.Add("Forecast", typeof(double));
            dt.Columns.Add("StaffingLevel", typeof(double));
            dt.Columns.Add("CalculatedOn", typeof(DateTime));
            dt.Columns.Add("InsertedOn", typeof(DateTime));
            dt.Columns.Add("ForecastWithShrinkage", typeof(double));


            foreach (var item in items)
			{
				foreach (var skillStaffingInterval in item.Intervals)
				{
					var row = dt.NewRow();
					row["SkillId"] = item.Id;
					row["BelongsToDate"] = skillStaffingInterval.StartDateTime.Date;
					row["StartDateTime"] = skillStaffingInterval.StartDateTime;
					row["EndDateTime"] = skillStaffingInterval.EndDateTime;
                    row["Forecast"] = skillStaffingInterval.Forecast;
                    row["StaffingLevel"] = skillStaffingInterval.StaffingLevel;
                    row["CalculatedOn"] = skillStaffingInterval.CalculatedOn;
                    row["InsertedOn"] = skillStaffingInterval.CalculatedOn;
                    row["ForecastWithShrinkage"] = skillStaffingInterval.ForecastWithShrinkage;
                    dt.Rows.Add(row);
				}
			}

			var connectionString = _currentUnitOfWorkFactory.Current().ConnectionString;
		    
			
			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
				var deleteCommandstring = @"DELETE from ReadModel.ScheduleForecastSkill";
                    var deleteCommand = new SqlCommand(deleteCommandstring, connection);
                    deleteCommand.Transaction = transaction;
                    deleteCommand.ExecuteNonQuery();
                    
                    using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                    {
                        sqlBulkCopy.DestinationTableName = "[ReadModel].[ScheduleForecastSkill]";
                        sqlBulkCopy.WriteToServer(dt);
                    }
                    transaction.Commit();
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
			,[StaffingLevel],
            [CalculatedOn],
            [ForecastWithShrinkage]
				 FROM [ReadModel].[ScheduleForecastSkill]
				where [startDateTime] between :startDateTime and :endDateTime
				and SkillId = :skillId")
				.AddScalar("StartDateTime", NHibernateUtil.DateTime)
				.AddScalar("EndDateTime", NHibernateUtil.DateTime)
				.AddScalar("Forecast", NHibernateUtil.Double)
				.AddScalar("StaffingLevel", NHibernateUtil.Double)
				.AddScalar("CalculatedOn", NHibernateUtil.DateTime)
                .AddScalar("ForecastWithShrinkage", NHibernateUtil.Double)
                .SetDateTime("startDateTime", startDateTime)
				.SetDateTime("endDateTime", endDateTime.AddSeconds(-1)) //-1 to not include next interval
				.SetGuid("skillId", skillId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (SkillStaffingInterval)))
				.List<SkillStaffingInterval>();

			return result;
		}

		public IEnumerable<SkillStaffingInterval> GetBySkillArea(Guid id, DateTime startDateTime, DateTime endDateTime)
		{
			var result = ((NHibernateUnitOfWork)_currentUnitOfWorkFactory.Current().CurrentUnitOfWork()).Session.CreateSQLQuery(
                @"SELECT 
					StartDateTime,EndDateTime,Sum(Forecast) as Forecast,Sum(StaffingLevel) as StaffingLevel, CalculatedOn, ForecastWithShrinkage
				 FROM [ReadModel].[ScheduleForecastSkill]
				inner join SkillAreaSkillCollection on SkillId = Skill
				where [startDateTime] between :startDateTime and :endDateTime
				and SkillArea = :id
				group by  StartDateTime, EndDateTime")
				.AddScalar("StartDateTime", NHibernateUtil.DateTime)
				.AddScalar("EndDateTime", NHibernateUtil.DateTime)
				.AddScalar("Forecast", NHibernateUtil.Double)
				.AddScalar("CalculatedOn", NHibernateUtil.DateTime)
				.AddScalar("StaffingLevel", NHibernateUtil.Double)
                .AddScalar("ForecastWithShrinkage", NHibernateUtil.Double)
                .SetDateTime("startDateTime", startDateTime)
				.SetDateTime("endDateTime", endDateTime)
                .SetGuid("id", id)
				.SetResultTransformer(Transformers.AliasToBean(typeof(SkillStaffingInterval)))
				.List<SkillStaffingInterval>();

			return result;
		}

	    public DateTime GetLastCalculatedTime()
	    {
	        var result =
	            ( (NHibernateUnitOfWork) _currentUnitOfWorkFactory.Current().CurrentUnitOfWork()).Session.CreateSQLQuery(
	                    @"SELECT 
					max(CalculatedOn) as CalculatedOn
				 FROM [ReadModel].[ScheduleForecastSkill]")
	                .UniqueResult<DateTime?>();
            
            return result.GetValueOrDefault();
        }
	}
}
