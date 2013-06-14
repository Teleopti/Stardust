using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Analytics.Etl.Transformer.ScheduleThreading
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
        public static IPersonAssignment GetPersonAssignmentForLayer(ISchedulePart schedule, ILayer<IPayload> layer)
        {
        	IList<IPersonAssignment> pAssignCollection = schedule.PersonAssignmentCollection();

        	return pAssignCollection.FirstOrDefault(personAssignment => personAssignment.Period.Intersect(layer.Period));
        }


        public event RowsUpdatedEventHandler RowsUpdatedEvent;

        public void Transform(IList<IScheduleDay> scheduleList, DateTime insertDateTime, IJobParameters jobParameters, IThreadPool threadPool)
        {
            //jobParameters.Helper.Repository.TruncateSchedule();

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

        void threadPool_RowsUpdatedEvent(object sender, TransformerInfrastructure.RowsUpdatedEventArgs e)
        {
            if (RowsUpdatedEvent != null)
            {
                RowsUpdatedEvent(this, new RowsUpdatedEventArgs(e.AffectedRows));
            }
        }

    }
}