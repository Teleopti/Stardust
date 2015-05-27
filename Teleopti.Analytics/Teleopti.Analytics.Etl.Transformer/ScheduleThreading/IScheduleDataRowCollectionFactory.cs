using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer.ScheduleThreading
{
	public interface IScheduleDataRowCollectionFactory
	{
		ICollection<DataRow> CreateScheduleDataRowCollection(DataTable dataTable
																	, IScheduleProjection scheduleProjection
																	, IntervalBase interval
																	, DateTimePeriod intervalPeriod
																	, DateTime insertDateTime
																	, int intervalsPerDay
																	, IScheduleDataRowFactory scheduleDataRowFactory);
		
	}
}