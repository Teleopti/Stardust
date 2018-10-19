using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading
{
	public class ThreadObj : ITaskParameters
	{
		public ThreadObj(IList<ScheduleProjection> scheduleProjectionServiceList, DateTime insertDateTime, IJobParameters jobParameters)
		{
			ScheduleProjectionServiceList = scheduleProjectionServiceList;
			InsertDateTime = insertDateTime;
			JobParameters = jobParameters;
			ScheduleDataRowFactory = new ScheduleDataRowCollectionFactory();
		}

		public IList<ScheduleProjection> ScheduleProjectionServiceList { get; }
		public DateTime InsertDateTime { get; }
		public IJobParameters JobParameters { get; }
		public ScheduleDataRowCollectionFactory ScheduleDataRowFactory { set; get; }
		public DataTable ScheduleTable { get; set; }
	}
}