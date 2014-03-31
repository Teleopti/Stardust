using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer.ScheduleThreading
{
	public static class WorkThreadClass
	{
		public static int WorkThread(object parameter)
		{
			var threadObj = (ThreadObj)parameter;

			IList<ScheduleProjection> scheduleProjectionServiceList = threadObj.ScheduleProjectionServiceList;
			DateTime insertDateTime = threadObj.InsertDateTime;
			IJobParameters jobParameters = threadObj.JobParameters;

			int affectedRows;
			using (var scheduleDataTable = new DataTable())
			{
				scheduleDataTable.Locale = Thread.CurrentThread.CurrentCulture;
				ScheduleInfrastructure.AddColumnsToDataTable(scheduleDataTable);

				using (var absenceDayCountDataTable = new DataTable())
				{
					absenceDayCountDataTable.Locale = Thread.CurrentThread.CurrentCulture;
					ScheduleDayAbsenceCountInfrastructure.AddColumnsToDataTable(absenceDayCountDataTable);

					int minutesPerInterval = 1440 / jobParameters.IntervalsPerDay;

					foreach (ScheduleProjection scheduleProjectionService in scheduleProjectionServiceList)
					{
						var significantPart = scheduleProjectionService.SchedulePart.SignificantPart();

						if (!scheduleProjectionService.SchedulePartProjection.HasLayers || significantPart == SchedulePartView.ContractDayOff)
							continue;

						if (significantPart == SchedulePartView.DayOff)
						{
							bool hasNoOvertime = scheduleProjectionService.SchedulePartProjection.Overtime().Equals(TimeSpan.Zero);
							if (hasNoOvertime)
								continue;
						}

						if (significantPart == SchedulePartView.FullDayAbsence)
						{
							// We got a whole day absence - add it to absence day count table
							absenceDayCountDataTable.Rows.Add(
								DayAbsenceDataRowFactory.CreateDayAbsenceDataRow(absenceDayCountDataTable,
																				 scheduleProjectionService));
						}

						DateTimePeriod? layerCollectionPeriod = scheduleProjectionService.SchedulePartProjection.Period();
						if (layerCollectionPeriod.HasValue)
						{
							//Use two variables to get shiftStart and shiftEnd in "sync" with Analytics intervals
							var shiftStart = getDateFromInterval(layerCollectionPeriod.Value.StartDateTime, minutesPerInterval, true);
							var shiftEnd = getDateFromInterval(layerCollectionPeriod.Value.EndDateTime, minutesPerInterval, false);

							// We have a shift - break it down into intervals
							for (DateTime date = shiftStart;
										  date <= shiftEnd;
										  date = date.AddMinutes(minutesPerInterval))
							{
								// Loop through every interval of this day´s projected layers
								var currentInterval = new IntervalBase(date, jobParameters.IntervalsPerDay);

								var intervalPeriod = new DateTimePeriod(date, date.AddMinutes(minutesPerInterval));

								// Get list of schedule data rows for current interval
								ICollection<DataRow> rows =
									threadObj.ScheduleDataRowFactory.CreateScheduleDataRowCollection(scheduleDataTable,
																									 scheduleProjectionService,
																									 currentInterval,
																									 intervalPeriod,
																									 insertDateTime,
																									 jobParameters.IntervalsPerDay,
																									 new ScheduleDataRowFactory());

								//Fill the bulk insert table - for schedule
								foreach (DataRow dataRow in rows)
								{
									scheduleDataTable.Rows.Add(dataRow);
								}
							}
						}

					}

					affectedRows = jobParameters.Helper.Repository.PersistSchedule(scheduleDataTable, absenceDayCountDataTable);
				}
			}

			return affectedRows;
		}

		private static DateTime getDateFromInterval(DateTime date, int minutesPerInterval, bool shiftStart)
		{
			if (date.Minute % minutesPerInterval == 0)
				return date;

			//move shiftstart back to previous intervalStart
			if (shiftStart)
				return date.AddMinutes(-1 * (date.Minute % minutesPerInterval));
			
			return date.AddMinutes(minutesPerInterval + (date.Minute % minutesPerInterval));
		}
	}
}
