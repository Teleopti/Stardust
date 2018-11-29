using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading
{
	public static class ThreadObjects
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Objs"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
		public static List<ITaskParameters> GetThreadObjectsSplitByPeriod(IList<ScheduleProjection> scheduleProjectionServiceList, DateTime insertDateTime, int span, IJobParameters parameters)
		{
			// Get earliest start and latest end datetime from schedulePartCollection
			var totalPeriodStartDate = scheduleProjectionServiceList.Min(s => s.SchedulePart.DateOnlyAsPeriod.DateOnly);
			var totalPeriodEndDate = scheduleProjectionServiceList.Max(s => s.SchedulePart.DateOnlyAsPeriod.DateOnly);
			var totalDateOnlyPeriod = new DateOnlyPeriod(totalPeriodStartDate, totalPeriodEndDate);

			var taskParameterList = new List<ITaskParameters>();
			var dayCollection = totalDateOnlyPeriod.DayCollection();

			var batchDays = dayCollection.Batch(span);
			foreach (IEnumerable<DateOnly> dateOnlyCollcetion in batchDays)
			{
				var minDate = dateOnlyCollcetion.First();
				var maxDate = dateOnlyCollcetion.Last();
				IList<ScheduleProjection> scheduleProjectionChunkList =
					 scheduleProjectionServiceList.Where(
						  s =>
						  (s.SchedulePart.DateOnlyAsPeriod.DateOnly <= maxDate) &&
						  (s.SchedulePart.DateOnlyAsPeriod.DateOnly >= minDate)).ToArray();

				taskParameterList.Add(new ThreadObj(
								 scheduleProjectionChunkList,
								 insertDateTime,
								 parameters));

			}
			return taskParameterList;
		}
	}
}