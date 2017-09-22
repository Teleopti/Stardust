using System;
using System.Collections.Generic;
using System.Configuration;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading
{

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances")]
	public delegate void RowsUpdatedEventHandler(object sender, RowsUpdatedEventArgs e);

	public class RowsUpdatedEventArgs : EventArgs
	{
		public RowsUpdatedEventArgs(int affectedRows)
		{
			AffectedRows = affectedRows;
		}

		public int AffectedRows { set; get; }
	}

	public class ScheduleTransformer : IScheduleTransformer
	{
		public static IPersonAssignment GetPersonAssignmentForLayer(IScheduleDay schedule, ILayer<IPayload> layer)
		{
			var ass = schedule.PersonAssignment();
			if (ass != null && ass.Period.Intersect(layer.Period))
			{
				return ass;
			}
			return null;
		}


		public event RowsUpdatedEventHandler RowsUpdatedEvent;

		public void Transform(IList<IScheduleDay> scheduleList, DateTime insertDateTime, IJobParameters jobParameters, IThreadPool threadPool)
		{
			IList<ScheduleProjection> scheduleProjectionServiceList = ProjectionsForAllAgentSchedulesFactory.CreateProjectionsForAllAgentSchedules(scheduleList);

			if (scheduleProjectionServiceList.Count > 0)
			{
				int chunkTimeSpan;
				if (!int.TryParse(ConfigurationManager.AppSettings["chunkTimeSpan"], out chunkTimeSpan))
				{
					chunkTimeSpan = 7;
				}
				List<ITaskParameters> objs2 = ThreadObjects.GetThreadObjectsSplitByPeriod(scheduleProjectionServiceList,
																												  insertDateTime, chunkTimeSpan,
																												  jobParameters);

				threadPool.Load(objs2, WorkThreadClass.WorkThread);

				threadPool.RowsUpdatedEvent += threadPool_RowsUpdatedEvent;

				threadPool.Start();

				if (threadPool.ThreadError != null)
				{
					throw threadPool.ThreadError;
				}
			}
		}

		void threadPool_RowsUpdatedEvent(object sender, Common.Infrastructure.RowsUpdatedEventArgs e)
		{
			if (RowsUpdatedEvent != null)
			{
				RowsUpdatedEvent(this, new RowsUpdatedEventArgs(e.AffectedRows));
			}
		}

	}
}