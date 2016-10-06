using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
		private readonly INow _now;

		public ScheduleForecastSkillReadModelRepository(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, INow now)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_now = now;
		}

		public void Persist(IEnumerable<SkillStaffingInterval> intervals, DateTime timeWhenResourceCalcDataLoaded)
		{
			var skills = intervals.Select(x => x.SkillId).Distinct();
			var dt = new DataTable();
			dt.Columns.Add("SkillId",typeof(Guid));
			dt.Columns.Add("BelongsToDate",typeof(DateTime));
			dt.Columns.Add("StartDateTime", typeof(DateTime));
			dt.Columns.Add("EndDateTime", typeof(DateTime));
            dt.Columns.Add("Forecast", typeof(double));
            dt.Columns.Add("StaffingLevel", typeof(double));
            dt.Columns.Add("InsertedOn", typeof(DateTime));
            dt.Columns.Add("ForecastWithShrinkage", typeof(double));

			var insertedOn = _now.UtcDateTime();

			foreach (var interval in intervals)
			{
				var row = dt.NewRow();
				row["SkillId"] = interval.SkillId;
				row["BelongsToDate"] = interval.StartDateTime.Date;
				row["StartDateTime"] = interval.StartDateTime;
				row["EndDateTime"] = interval.EndDateTime;
				row["Forecast"] = interval.Forecast;
				row["StaffingLevel"] = interval.StaffingLevel;
				row["InsertedOn"] = insertedOn;
				row["ForecastWithShrinkage"] = interval.ForecastWithShrinkage;
				dt.Rows.Add(row);
			}

			var connectionString = _currentUnitOfWorkFactory.Current().ConnectionString;
		    
			
			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
					var deleteCommand = new SqlCommand( );
					var deleteCommandstring = string.Format(@"DELETE from ReadModel.ScheduleForecastSkill where skillid in({0})" , AddArrayParameters(deleteCommand, skills.ToArray(), "ids")) ;
	                deleteCommand.CommandText = deleteCommandstring;
	                deleteCommand.Connection = connection;
                    deleteCommand.Transaction = transaction;
                    deleteCommand.ExecuteNonQuery();
					
					using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                    {
                        sqlBulkCopy.DestinationTableName = "[ReadModel].[ScheduleForecastSkill]";
                        sqlBulkCopy.WriteToServer(dt);
                    }

					var deleteCommandForChanges = new SqlCommand();
	                deleteCommandForChanges.CommandText =
		                string.Format(
			                @"DELETE from ReadModel.ScheduleForecastSkillChange where insertedOn < @timeWhenResourceCalcDataLoaded and skillid in ({0})",
			                AddArrayParameters(deleteCommandForChanges, skills.ToArray(), "ids"));
	                deleteCommandForChanges.Connection = connection;
					deleteCommandForChanges.Transaction = transaction;
					deleteCommandForChanges.Parameters.AddWithValue("@timeWhenResourceCalcDataLoaded", timeWhenResourceCalcDataLoaded);
					deleteCommandForChanges.ExecuteNonQuery();

					transaction.Commit();
                }
			    
            }
		}
		

		public IEnumerable<SkillStaffingInterval> GetBySkill(Guid skillId, DateTime startDateTime, DateTime endDateTime)
		{
			var result = ((NHibernateUnitOfWork) _currentUnitOfWorkFactory.Current().CurrentUnitOfWork()).Session.CreateSQLQuery(
				@"SELECT 
				[SkillId]
			,[StartDateTime]
			,[EndDateTime]
			,[Forecast]
			,[StaffingLevel],
            [ForecastWithShrinkage]
				 FROM [ReadModel].[ScheduleForecastSkill]
				where (( [StartDateTime] < :startDateTime  and   [EndDateTime] > :startDateTime) 
					or ( [StartDateTime] >= :startDateTime  and :endDateTime > [StartDateTime])  )
				and SkillId = :skillId")
				.AddScalar("StartDateTime", NHibernateUtil.DateTime)
				.AddScalar("EndDateTime", NHibernateUtil.DateTime)
				.AddScalar("Forecast", NHibernateUtil.Double)
				.AddScalar("StaffingLevel", NHibernateUtil.Double)
                .AddScalar("ForecastWithShrinkage", NHibernateUtil.Double)
				.AddScalar("SkillId", NHibernateUtil.Guid)
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
					StartDateTime,EndDateTime,Sum(Forecast) as Forecast,Sum(StaffingLevel) as StaffingLevel, ForecastWithShrinkage
				 FROM [ReadModel].[ScheduleForecastSkill]
				inner join SkillAreaSkillCollection on SkillId = Skill
				where [startDateTime] between :startDateTime and :endDateTime
				and SkillArea = :id
				group by  StartDateTime, EndDateTime")
				.AddScalar("StartDateTime", NHibernateUtil.DateTime)
				.AddScalar("EndDateTime", NHibernateUtil.DateTime)
				.AddScalar("Forecast", NHibernateUtil.Double)
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
					max(InsertedOn) as InsertedOn
				 FROM [ReadModel].[ScheduleForecastSkill]")
	                .UniqueResult<DateTime?>();
            
            return result.GetValueOrDefault();
        }

	    public void PersistChange(StaffingIntervalChange staffingIntervalChanges)
	    {
            ((NHibernateUnitOfWork)_currentUnitOfWorkFactory.Current().CurrentUnitOfWork()).Session.CreateSQLQuery(@"
						INSERT INTO [ReadModel].[ScheduleForecastSkillChange]
						(
							[SkillId]
                            ,[StartDateTime]
                            ,[EndDateTime]
                            ,[StaffingLevel]
                            ,[InsertedOn]
						)
						VALUES
						(
							:SkillId,
							:StartDateTime,
							:EndDateTime,
							:StaffingLevel,
							:InsertedOn
						)
					")
                    .SetParameter("SkillId", staffingIntervalChanges.SkillId)
                    .SetParameter("StartDateTime", staffingIntervalChanges.StartDateTime)
                    .SetParameter("EndDateTime", staffingIntervalChanges.EndDateTime)
                    .SetParameter("StaffingLevel", staffingIntervalChanges.StaffingLevel)
                    .SetParameter("InsertedOn", _now.UtcDateTime())
                    .ExecuteUpdate();
        }

	    public IEnumerable<StaffingIntervalChange> GetReadModelChanges(DateTimePeriod dateTimePeriod)
	    {
            var result = ((NHibernateUnitOfWork)_currentUnitOfWorkFactory.Current().CurrentUnitOfWork()).Session.CreateSQLQuery(
				@"SELECT 
					[SkillId]
                            ,[StartDateTime]
                            ,[EndDateTime]
                            ,[StaffingLevel]
				 FROM [ReadModel].[ScheduleForecastSkillChange]
                    where (( [StartDateTime] < :startDateTime  and   [EndDateTime] > :startDateTime) 
					or ( [StartDateTime] >= :startDateTime  and :endDateTime > [StartDateTime])  )")
                .AddScalar("StartDateTime", NHibernateUtil.DateTime)
                .AddScalar("EndDateTime", NHibernateUtil.DateTime)
                .AddScalar("SkillId", NHibernateUtil.Guid)
                .AddScalar("StaffingLevel", NHibernateUtil.Double)
                .SetDateTime("startDateTime", dateTimePeriod.StartDateTime)
                .SetDateTime("endDateTime", dateTimePeriod.EndDateTime)
                .SetResultTransformer(Transformers.AliasToBean(typeof(StaffingIntervalChange)))
                .List<StaffingIntervalChange>();

            return result;
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

	
}
