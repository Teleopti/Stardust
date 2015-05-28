using System;
using System.Collections.Generic;
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

		public IList<ScheduleProjection> ScheduleProjectionServiceList { private set; get; }
		public DateTime InsertDateTime { private set; get; }
		public IJobParameters JobParameters { private set; get; }
		public IScheduleDataRowCollectionFactory ScheduleDataRowFactory { set; get; }

	}
}