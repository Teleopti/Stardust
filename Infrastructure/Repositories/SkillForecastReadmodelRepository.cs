using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday.To_Staffing;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SkillForecastReadModelRepository : ISkillForecastReadModelRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly INow _now;
		private readonly IStardustJobFeedback _stardustJobFeedback;
		private readonly SkillForecastSettingsReader _skillForecastSettingsReader;

		public SkillForecastReadModelRepository(ICurrentUnitOfWork currentUnitOfWork, INow now, IStardustJobFeedback stardustJobFeedback, SkillForecastSettingsReader skillForecastSettingsReader)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_now = now;
			_stardustJobFeedback = stardustJobFeedback;
			_skillForecastSettingsReader = skillForecastSettingsReader;
		}

		public void PersistSkillForecast(List<SkillForecast> listOfIntervals)
		{
			var connectionString = _currentUnitOfWork.Current().Session().Connection.ConnectionString;



			var dt = new DataTable();
			dt.Columns.Add("SkillId", typeof(Guid));
			dt.Columns.Add("StartDateTime", typeof(DateTime));
			dt.Columns.Add("EndDateTime", typeof(DateTime));
			dt.Columns.Add("Agents", typeof(double));
			dt.Columns.Add("AgentsWithShrinkage", typeof(double));
			dt.Columns.Add("Calls", typeof(double));
			dt.Columns.Add("AverageHandleTime", typeof(double));
			dt.Columns.Add("IsBackOffice", typeof(bool));
			dt.Columns.Add("PercentAnswered", typeof(double));
			dt.Columns.Add("AnsweredWithinSeconds", typeof(double));
			dt.Columns.Add("InsertedOn", typeof(DateTime));

			var insertedOn = _now.UtcDateTime();

			foreach (var intervals in listOfIntervals)
			{
				var row = dt.NewRow();

				row["SkillId"] = intervals.SkillId;
				row["StartDateTime"] = intervals.StartDateTime;
				row["EndDateTime"] = intervals.EndDateTime;
				row["Agents"] = intervals.Agents;
				row["AgentsWithShrinkage"] = intervals.AgentsWithShrinkage;
				row["Calls"] = intervals.Calls;
				row["AverageHandleTime"] = intervals.AverageHandleTime;
				row["IsBackOffice"] = intervals.IsBackOffice;
				row["PercentAnswered"] = intervals.PercentAnswered;
				row["AnsweredWithinSeconds"] = intervals.AnsweredWithinSeconds;
				row["InsertedOn"] = insertedOn;

				dt.Rows.Add(row);
			}

			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction())
				{
					removeHistoricalData(transaction, connection);

					var groupOfIntervalsPerSkill = listOfIntervals.GroupBy(x => x.SkillId)
						.ToDictionary(g => g.Key, g => g.ToList().Select(x => x.StartDateTime.Date).Distinct());
					foreach (var intervalsForSkill in groupOfIntervalsPerSkill)
					{
						var skillId = intervalsForSkill.Key;
						foreach (var skillDay in intervalsForSkill.Value)
						{
							removeAllExistingSkillForecastOnDay(skillId, skillDay, transaction, connection);
						}

					}

					using (var sqlBulkCopy =
						new SqlBulkCopy(connection, SqlBulkCopyOptions.CheckConstraints, transaction))
					{
						sqlBulkCopy.DestinationTableName = "[ReadModel].[SkillForecast]";
						sqlBulkCopy.WriteToServer(dt);
					}

					transaction.Commit();
				}
			}
		}

		public IList<SkillForecast> LoadSkillForecast(Guid[] skills, DateTimePeriod period)
		{
			const string sql =
				"select SkillId, StartDateTime,EndDateTime, Agents,AgentsWithShrinkage, Calls, AverageHandleTime,IsBackOffice, PercentAnswered, AnsweredWithinSeconds from [ReadModel].[SkillForecast] " +
				"Where SkillId In (:skillIds) AND StartDateTime >= :startDate AND StartDateTime <= :endDate";
			var result = new List<SkillForecast>();
			result.AddRange(_currentUnitOfWork.Session().CreateSQLQuery(sql)
				.SetDateTime("startDate", period.StartDateTime)
				.SetDateTime("endDate", period.EndDateTime)
				.SetParameterList("skillIds", skills.ToArray())
				.SetResultTransformer(Transformers.AliasToBean(typeof(SkillForecast)))
				.SetReadOnly(true)
				.List<SkillForecast>());

			return result;
		}

		private void removeAllExistingSkillForecastOnDay(Guid skillId, DateTime day, SqlTransaction transaction,
			SqlConnection connection)
		{
			using (var deleteCommand = new SqlCommand(@"DELETE  FROM ReadModel.SkillForecast
					WHERE SkillId = @skillId
						AND StartDateTime >= @startOfDay
						AND StartDateTime < @endOfDay", connection, transaction))
			{
				deleteCommand.Parameters.AddWithValue("@skillId", skillId);
				deleteCommand.Parameters.AddWithValue("@startOfDay", day.Date);
				deleteCommand.Parameters.AddWithValue("@endOfDay", day.Date.AddDays(1));
				deleteCommand.ExecuteNonQuery();
			}
		}

		private void removeHistoricalData(SqlTransaction transaction,
			SqlConnection connection)
		{
			var purgeDate = _now.UtcDateTime().AddDays(-_skillForecastSettingsReader.NumberOfDaysInPast);
			_stardustJobFeedback.SendProgress(
				$"Removing historical resources that is older than {purgeDate.Date}");
			using (var deleteCommand = new SqlCommand(@"DELETE FROM ReadModel.SkillForecast
						WHERE StartDateTime < @purgeDate",
				connection, transaction))
			{
				deleteCommand.Parameters.AddWithValue("@purgeDate", purgeDate.Date.AddDays(1));
				deleteCommand.ExecuteNonQuery();
			}
		}
	}
}