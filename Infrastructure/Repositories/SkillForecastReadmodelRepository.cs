using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday.To_Staffing;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SkillForecastReadModelRepository : ISkillForecastReadModelRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly INow _now;

		public SkillForecastReadModelRepository(ICurrentUnitOfWork currentUnitOfWork, INow now)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_now = now;
		}

		public void PersistSkillForecast(List<SkillForecast> listOfIntervals)
		{
			var connectionString = _currentUnitOfWork.Current().Session().Connection.ConnectionString;
			
				
				var dt = new DataTable();
				dt.Columns.Add("SkillId", typeof(Guid));
				dt.Columns.Add("StartDateTime", typeof(DateTime));
				dt.Columns.Add("EndDateTime", typeof(DateTime));
				dt.Columns.Add("Agents", typeof(double));
				dt.Columns.Add("Calls", typeof(double));
				dt.Columns.Add("AverageHandleTime", typeof(double));
				dt.Columns.Add("InsertedOn", typeof(DateTime));
				
				var insertedOn = _now.UtcDateTime();

				foreach (var intervals in listOfIntervals)
				{
					var row = dt.NewRow();

					row["SkillId"] = intervals.SkillId;
					row["StartDateTime"] =intervals.StartDateTime;
					row["EndDateTime"] =intervals.EndDateTime;
					row["Agents"] = intervals.Agents;
					row["Calls"] = intervals.Calls;
					row["AverageHandleTime"] = intervals.AverageHandleTime;
					row["InsertedOn"] = insertedOn;
					dt.Rows.Add(row);
				}

			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction())
				{
					//using (var deleteCommand = new SqlCommand(@"DELETE FROM [ReadModel].[SkillCombinationResourceBpo] 
					//	WHERE StartDateTime <= @8DaysAgo", connection, transaction))
					//{
					//	deleteCommand.Parameters.AddWithValue("@buid", bu);
					//	deleteCommand.Parameters.AddWithValue("@8DaysAgo", _now.UtcDateTime().AddDays(-8));
					//	deleteCommand.ExecuteNonQuery();
					//}

					using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.CheckConstraints, transaction))
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
			var result = new List<SkillForecast>();
			var connectionString = _currentUnitOfWork.Current().Session().Connection.ConnectionString;
			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction())
				{
					using (var command =
						new SqlCommand(
							$"select SkillId, StartDateTime,EndDateTime, Agents, Calls, AverageHandleTime from [ReadModel].[SkillForecast] " +
							$"Where SkillId In ({getInValues(skills)}) AND StartDateTime >= '{period.StartDateTime}' AND StartDateTime <= '{period.EndDateTime}'",
							connection, transaction))
					{

						using (var reader = command.ExecuteReader())
						{
							if (!reader.HasRows) return new List<SkillForecast>();
							while (reader.Read())
							{
								var skillForecastInterval = new SkillForecast
								{
									SkillId = reader.GetGuid(0),
									StartDateTime = reader.GetDateTime(1),
									EndDateTime = reader.GetDateTime(2),
									Agents = reader.GetDouble(3),
									Calls = reader.GetDouble(4),
									AverageHandleTime = reader.GetDouble(5)
								};
								result.Add(skillForecastInterval);
							}
						}
					}
					transaction.Commit();
					return result;
				}
			}
		}

		private string getInValues(Guid[] values)
		{
			return "'" + string.Join("','", values) + "'";
		}
	}

	
}