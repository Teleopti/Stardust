using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ScheduleForecastSkillReadModelRepository : IScheduleForecastSkillReadModelRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly INow _now;

		public ScheduleForecastSkillReadModelRepository(ICurrentUnitOfWork currentUnitOfWork, INow now)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_now = now;
		}

		public void Persist(IEnumerable<SkillStaffingInterval> intervals, DateTime timeWhenResourceCalcDataLoaded)
		{
			var skills = intervals.Select(x => x.SkillId).Distinct();
			var dt = new DataTable();
			dt.Columns.Add("SkillId", typeof(Guid));
			dt.Columns.Add("BelongsToDate", typeof(DateTime));
			dt.Columns.Add("StartDateTime", typeof(DateTime));
			dt.Columns.Add("EndDateTime", typeof(DateTime));
			dt.Columns.Add("Forecast", typeof(double));
			dt.Columns.Add("StaffingLevel", typeof(double));
			dt.Columns.Add("InsertedOn", typeof(DateTime));
			dt.Columns.Add("ForecastWithShrinkage", typeof(double));
			dt.Columns.Add("StaffingLevelWithShrinkage", typeof(double));

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
				row["StaffingLevelWithShrinkage"] = interval.StaffingLevelWithShrinkage;
				dt.Rows.Add(row);
			}

			var connectionString = _currentUnitOfWork.Session().Connection.ConnectionString;


			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
				{
					using (var deleteCommand = new SqlCommand())
					{
						deleteCommand.CommandText = $@"DELETE from ReadModel.ScheduleForecastSkill where skillid in({AddArrayParameters(deleteCommand, skills.ToArray(), "ids")})";
						deleteCommand.Connection = connection;
						deleteCommand.Transaction = transaction;
						deleteCommand.ExecuteNonQuery();
					}

					using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
					{
						sqlBulkCopy.DestinationTableName = "[ReadModel].[ScheduleForecastSkill]";
						sqlBulkCopy.WriteToServer(dt);
					}

					using (var deleteCommandForChanges = new SqlCommand())
					{
						deleteCommandForChanges.CommandText =
							$@"DELETE from ReadModel.ScheduleForecastSkillChange where insertedOn < @timeWhenResourceCalcDataLoaded and skillid in ({AddArrayParameters(deleteCommandForChanges, skills.ToArray(), "ids")})";
						deleteCommandForChanges.Connection = connection;
						deleteCommandForChanges.Transaction = transaction;
						deleteCommandForChanges.Parameters.AddWithValue("@timeWhenResourceCalcDataLoaded", timeWhenResourceCalcDataLoaded);
						deleteCommandForChanges.ExecuteNonQuery();
					}
					transaction.Commit();
				}

			}
		}

		public IEnumerable<SkillStaffingInterval> ReadMergedStaffingAndChanges(Guid skillId, DateTimePeriod period)
		{
			var skillStaffingIntervals = GetBySkills(new[] { skillId}, period.StartDateTime, period.EndDateTime).ToList();
			var mergedStaffingIntervals = new List<SkillStaffingInterval>();
			var intervalChanges = GetReadModelChanges(period).Where(x => x.SkillId == skillId).ToList();
			if (intervalChanges.Any())
			{
				skillStaffingIntervals.ForEach(interval =>
														 {
															 var changes =
																	  intervalChanges.Where(x => x.StartDateTime == interval.StartDateTime && x.EndDateTime == interval.EndDateTime).ToList();
															 if (changes.Any())
															 {
																 interval.StaffingLevel += changes.Sum((x => x.StaffingLevel));
																 interval.StaffingLevelWithShrinkage += changes.Sum((x => x.StaffingLevel));
															 }
															 mergedStaffingIntervals.Add(interval);
														 });
			}
			else
			{
				mergedStaffingIntervals = skillStaffingIntervals.ToList();
			}
			return mergedStaffingIntervals;
		}

		public IEnumerable<SkillStaffingInterval> GetBySkills(Guid[] guids, DateTime startDateTime, DateTime endDateTime)
		{
			var result = _currentUnitOfWork.Session().CreateSQLQuery(
				@"SELECT 
				[SkillId], [StartDateTime], [EndDateTime], [Forecast], [StaffingLevel], [ForecastWithShrinkage], [StaffingLevelWithShrinkage] FROM [ReadModel].[ScheduleForecastSkill]
				where (( [StartDateTime] < :startDateTime  and   [EndDateTime] > :startDateTime) 
				or ( [StartDateTime] >= :startDateTime  and :endDateTime > [StartDateTime])  )
				and SkillId in ( :skillIds)")
				.AddScalar("StartDateTime", NHibernateUtil.DateTime)
				.AddScalar("EndDateTime", NHibernateUtil.DateTime)
				.AddScalar("Forecast", NHibernateUtil.Double)
				.AddScalar("StaffingLevel", NHibernateUtil.Double)
					 .AddScalar("ForecastWithShrinkage", NHibernateUtil.Double)
				.AddScalar("SkillId", NHibernateUtil.Guid)
				.SetDateTime("startDateTime", startDateTime)
				.SetDateTime("endDateTime", endDateTime)
				.SetParameterList("skillIds", guids.ToArray())
				.SetResultTransformer(Transformers.AliasToBean(typeof(SkillStaffingInterval)))
				.List<SkillStaffingInterval>();

			return result;
		
		}

		public void Purge()
		{
			var connectionString = _currentUnitOfWork.Session().Connection.ConnectionString;
			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (var sqlCommand = new SqlCommand(@"delete from ReadModel.ScheduleForecastSkill where SkillId in (select id from skill where IsDeleted = 1)", connection))
				{
					sqlCommand.ExecuteNonQuery();
				}
			}
		}

		public IEnumerable<SkillStaffingInterval> GetBySkill(Guid skillId, DateTime startDateTime, DateTime endDateTime)
		{
			return GetBySkills(new[] { skillId }, startDateTime, endDateTime);
		}

		public IEnumerable<SkillStaffingInterval> ReadMergedStaffingAndChanges(Guid[] ids, DateTimePeriod period)
		{
			var allIntervals = GetBySkills(ids, period.StartDateTime, period.EndDateTime).ToList();
			var mergedStaffingIntervals = new List<SkillStaffingInterval>();
			var allIntervalChanges = GetReadModelChanges(period);
			foreach (var skillId in ids)
			{
				var skillStaffingIntervals = allIntervals.Where(x => x.SkillId == skillId);
				var skillIntervalChanges = allIntervalChanges.Where(x => x.SkillId == skillId).ToList();
				if (skillIntervalChanges.Any())
				{
					skillStaffingIntervals.ForEach(interval =>
					{
						var changes =
								 skillIntervalChanges.Where(x => x.StartDateTime == interval.StartDateTime && x.EndDateTime == interval.EndDateTime).ToList();
						if (changes.Any())
						{
							interval.StaffingLevel += changes.Sum((x => x.StaffingLevel));
							interval.StaffingLevelWithShrinkage += changes.Sum((x => x.StaffingLevel));
						}
						mergedStaffingIntervals.Add(interval);
					});
				}
				else
				{
					mergedStaffingIntervals.AddRange(skillStaffingIntervals.ToList());
				}
			}
			
			
			
			return mergedStaffingIntervals;
		}

		public IDictionary<Guid,DateTime> GetLastCalculatedTime()
		{
			var result =
				 _currentUnitOfWork.Session().CreateSQLQuery(
							@"select s.BusinessUnit, min(insertedon) as InsertedOn from ReadModel.ScheduleForecastSkill sfs, Skill s
							where sfs.SkillId = s.Id
						group by s.BusinessUnit")
				.SetResultTransformer(Transformers.AliasToBean(typeof(MaxIntervalOnBuModel)))
					  .List<MaxIntervalOnBuModel>();

			return result.ToDictionary(x => x.BusinessUnit, y => y.InsertedOn);
		}

		public void PersistChange(StaffingIntervalChange staffingIntervalChanges)
		{
			_currentUnitOfWork.Session().CreateSQLQuery(@"
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
			var result = _currentUnitOfWork.Session().CreateSQLQuery(
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


	internal class MaxIntervalOnBuModel
	{
		public Guid BusinessUnit { get; set; }
		public DateTime InsertedOn { get; set; }
	}
	


}
