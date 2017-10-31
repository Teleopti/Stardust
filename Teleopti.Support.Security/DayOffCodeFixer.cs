using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using log4net;

namespace Teleopti.Support.Security
{
	internal class DayOffCodeFixer : CommandLineCommandWithFixChecker
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(DayOffCodeFixer));

		public DayOffCodeFixer() : base(455)
		{
		}
		
		public override int Execute(IDatabaseArguments databaseArguments)
		{
			using (var appConnection = new SqlConnection(databaseArguments.ApplicationDbConnectionString))
			using (var analyticsConnection = new SqlConnection(databaseArguments.AnalyticsDbConnectionString))
			{
				analyticsConnection.Open();

				if (isFixApplied(analyticsConnection))
					return 1;
				appConnection.Open();

				var applicationDayOffs = loadApplicationDayOffs(appConnection);
				var businessUnits = loadBusinessUnits(analyticsConnection);
				var correct = new Dictionary<Guid, AnalyticsDayOff>();

				//Open transaction
				var transaction = analyticsConnection.BeginTransaction(IsolationLevel.ReadCommitted);

				log.Debug("Updating dim_day_off...");
				using (var daDayOff = new SqlDataAdapter("SELECT * FROM [mart].[dim_day_off]", analyticsConnection))
				{
					daDayOff.SelectCommand.Transaction = transaction;
					using (var builder = new SqlCommandBuilder(daDayOff))
					{
						daDayOff.UpdateCommand = builder.GetUpdateCommand();
						daDayOff.DeleteCommand = builder.GetDeleteCommand();
						daDayOff.InsertCommand = builder.GetInsertCommand();

						using (var ds = new DataSet())
						{
							ds.Locale = CultureInfo.InvariantCulture;
							daDayOff.Fill(ds);

							// Everything we already have a code for we should try not to change
							foreach (var dayOff in ds.Tables[0].Rows.Cast<DataRow>()
								.Select(row => new AnalyticsDayOff(row))
								.OrderByDescending(x => x.DayOffId)
								.Where(x => x.DayOffCode.HasValue))
							{
								correct.Add(dayOff.DayOffCode.Value, dayOff);
							}
							foreach (var dayOff in ds.Tables[0].Rows.Cast<DataRow>().Select(row => new AnalyticsDayOff(row)))
							{
								if (dayOff.DayOffId == -1) // Not defined, just assign empty guid and move on
								{
									dayOff.DayOffCode = Guid.Empty;
									continue;
								}

								if (dayOff.DayOffCode.HasValue)
								{
									var correctDayOffId = correct[dayOff.DayOffCode.Value].DayOffId;
									if (correctDayOffId == dayOff.DayOffId)
									{
										// This is the entry we already selected as new unique for duplicates, do nothing
										continue;
									}
									// This is a duplicate that should be removed and reconnected to correctDayOffId
									deleteAndReconnect(analyticsConnection, transaction, dayOff, correctDayOffId);
								}
								else
								{
									var businessUnit = businessUnits.Single(x => x.BusinessUnitId == dayOff.BusinessUnitId);
									var applicationDayOffsMatching = applicationDayOffs.Where(a => a.Name == dayOff.DayOffName && a.BusinessUnit == businessUnit.BusinessUnitCode).ToList();
									if (!applicationDayOffsMatching.Any())
									{
										// The connection to this dayoff doesn't exist anymore, assign a random guid and leave it
										dayOff.DayOffCode = Guid.NewGuid();
										correct.Add(dayOff.DayOffCode.Value, dayOff);
										continue;
									}
									if (applicationDayOffsMatching.Any(x => !correct.ContainsKey(x.Id)))
									{
										// We have atleast one match in application, try to find a not used guid to connect it to if possible
										var match = applicationDayOffsMatching.First(x => !correct.ContainsKey(x.Id));
										dayOff.DayOffCode = match.Id;
										correct.Add(dayOff.DayOffCode.Value, dayOff);
									}
									else
									{
										// All duplicates based on name already have a corrected entry in analytics, delete this one and reconnect to one of them
										var match = applicationDayOffsMatching.First();
										deleteAndReconnect(analyticsConnection, transaction, dayOff, correct[match.Id].DayOffId);
									}

								}
							}
							daDayOff.Update(ds);
						}
					}
				}

				// After we have made sure all existing entries has a code, set constraint!
				updateTable(analyticsConnection, transaction);

				setApplied(analyticsConnection, transaction);

				//Commit!
				log.Debug("Committing transaction...");
				transaction.Commit();
				log.Debug("Transaction successfully committed! Done.");
			}

			return 0;
		}

		private static List<AnalyticsBusinessUnit> loadBusinessUnits(SqlConnection analyticsConnection)
		{
			var businessUnits = new List<AnalyticsBusinessUnit>();
			using (var daDayOff = new SqlDataAdapter("SELECT * FROM [mart].[dim_business_unit]", analyticsConnection))
			{
				using (var ds = new DataSet())
				{
					ds.Locale = CultureInfo.InvariantCulture;
					daDayOff.Fill(ds);

					businessUnits.AddRange(ds.Tables[0].Rows.Cast<DataRow>().Select(row => new AnalyticsBusinessUnit(row)));
				}
			}
			return businessUnits;
		}

		private static void updateTable(SqlConnection connection, SqlTransaction transaction)
		{
			
			executeNonQuery(connection, transaction, "ALTER TABLE [mart].[dim_day_off] ALTER COLUMN [day_off_code] UNIQUEIDENTIFIER NOT NULL");
			executeNonQuery(connection, transaction, @"IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME ='AK_day_off_code')
				BEGIN
					ALTER TABLE [mart].[dim_day_off] ADD CONSTRAINT AK_day_off_code UNIQUE (day_off_code)
				END");
		}

		private static void deleteAndReconnect(SqlConnection connection, SqlTransaction transaction, AnalyticsDayOff dayOffToRemove, int dayOffIdToReconnectTo)
		{
			executeNonQuery(connection, transaction,
				$"UPDATE [mart].[fact_schedule_day_count] set day_off_id={dayOffIdToReconnectTo} where day_off_id={dayOffToRemove.DayOffId}");
			executeNonQuery(connection, transaction,
				$"UPDATE [mart].[fact_schedule_preference] set day_off_id={dayOffIdToReconnectTo} where day_off_id={dayOffToRemove.DayOffId}");

			dayOffToRemove.Delete();
		}

		private static List<AppDayOff> loadApplicationDayOffs(SqlConnection connection)
		{
			log.Debug("Load day off template from application");
			var dayOffTemplates = new List<AppDayOff>();
			using (var daDayOff = new SqlDataAdapter("SELECT * FROM [dbo].[DayOffTemplate]", connection))
			{
				using (var ds = new DataSet())
				{
					ds.Locale = CultureInfo.InvariantCulture;
					daDayOff.Fill(ds);

					dayOffTemplates.AddRange(ds.Tables[0].Rows.Cast<DataRow>().Select(row => new AppDayOff(row)));
				}
			}
			return dayOffTemplates;
		}

		internal class AppDayOff
		{
			public AppDayOff(DataRow row)
			{
				Id = (Guid)row["Id"];
				Name = (string)row["Name"];
				BusinessUnit = (Guid)row["BusinessUnit"];
			}

			public Guid BusinessUnit { get; set; }
			public Guid Id { get; set; }
			public string Name { get; set; }
		}

		internal class AnalyticsDayOff
		{
			private readonly DataRow row;

			public AnalyticsDayOff(DataRow row)
			{
				this.row = row;
			}

			public int DayOffId { get { return (int)row["day_off_id"]; } set { row["day_off_id"] = value; } }
			public Guid? DayOffCode { get { return row["day_off_code"] == DBNull.Value ? (Guid?)null : (Guid)row["day_off_code"]; } set { row["day_off_code"] = value; } }
			public string DayOffName { get { return (string)row["day_off_name"]; } set { row["day_off_name"] = value; } }
			public int BusinessUnitId { get { return (int)row["business_unit_id"]; } set { row["business_unit_id"] = value; } }

			public void Delete()
			{
				row.Delete();
			}
		}

		internal class AnalyticsBusinessUnit
		{
			public AnalyticsBusinessUnit(DataRow row)
			{
				BusinessUnitCode = row["business_unit_code"] == DBNull.Value ? (Guid?)null : (Guid)row["business_unit_code"];
				BusinessUnitId = (int)row["business_unit_id"];
			}

			public Guid? BusinessUnitCode { get; set; }
			public int BusinessUnitId { get; set; }
		}
	}
}