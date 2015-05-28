using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer.ScheduleThreading
{
	public interface IScheduleTransformer
	{
		event RowsUpdatedEventHandler RowsUpdatedEvent;
		void Transform(IList<IScheduleDay> scheduleList, DateTime insertDateTime, IJobParameters jobParameters, IThreadPool threadPool);
	}
}