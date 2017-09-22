using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading
{
	public interface IScheduleTransformer
	{
		event RowsUpdatedEventHandler RowsUpdatedEvent;
		void Transform(IList<IScheduleDay> scheduleList, DateTime insertDateTime, IJobParameters jobParameters, IThreadPool threadPool);
	}
}