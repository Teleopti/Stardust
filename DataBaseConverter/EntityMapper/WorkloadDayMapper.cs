using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Mapps a workload day
    /// </summary>
    public class WorkloadDayMapper : Mapper<WorkloadDay, global::Domain.ForecastDay>
    {
        private readonly int _intervalLength;


        /// <summary>
        /// Initializes a new instance of the <see cref="WorkloadDayMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="intervalLength">Length of the interval.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 4.12.2007
        /// </remarks>
        public WorkloadDayMapper(MappedObjectPair mappedObjectPair, ICccTimeZoneInfo timeZone,int intervalLength)
            : base(mappedObjectPair, timeZone)
        {
            _intervalLength = intervalLength;
        }

        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 3.12.2007
        /// Changed by: Henry Greijer 2008-10-30, added Annotations.
        /// </remarks>
        public override WorkloadDay Map(global::Domain.ForecastDay oldEntity)
        {
            IList<TimePeriod> openHours = new List<TimePeriod>();
            foreach (global::Domain.TimePeriod oldOpenHour in oldEntity.OpenHours)
            {
                openHours.Add(new TimePeriod(oldOpenHour.StartTime, oldOpenHour.EndTime));
            }

            TaskPeriodMapper tpMapper = new TaskPeriodMapper(MappedObjectPair, TimeZone, oldEntity.ForecastDate, _intervalLength);
            IDictionary<DateTime, ITemplateTaskPeriod> taskPeriods = new Dictionary<DateTime, ITemplateTaskPeriod>();
            foreach (global::Domain.ForecastData data in oldEntity.ForecastDataCollection.Values)
            {
                ITemplateTaskPeriod tp = tpMapper.Map(data);
                if (TaskPeriodIsInsideOpenHours(openHours,tp) &&
                    !taskPeriods.ContainsKey(tp.Period.StartDateTime))
                    taskPeriods.Add(tp.Period.StartDateTime,tp);
            }

            WorkloadDay newWorkloadDay = new WorkloadDay();
            newWorkloadDay.Create(new DateOnly(oldEntity.ForecastDate), 
                MappedObjectPair.Workload.GetPaired(oldEntity.ThisForecast),
                openHours);
            newWorkloadDay.Annotation = oldEntity.Annotation;

            newWorkloadDay.Lock();
            foreach (TemplateTaskPeriod templateTaskPeriod in newWorkloadDay.SortedTaskPeriodList)
            {
                DateTime startDateTime = templateTaskPeriod.Period.StartDateTime;

                //Not certain that source contains all date time periods due to DST switch
                if (taskPeriods.ContainsKey(startDateTime))
                {
                    templateTaskPeriod.SetTasks(taskPeriods[startDateTime].Task.Tasks);
                    templateTaskPeriod.AverageTaskTime = taskPeriods[startDateTime].Task.AverageTaskTime;
                    templateTaskPeriod.AverageAfterTaskTime = taskPeriods[startDateTime].Task.AverageAfterTaskTime;
                }
            }
            newWorkloadDay.Release();

            //Template should always be reset to <NONE> for converted days
            newWorkloadDay.UpdateTemplateName();

            return newWorkloadDay;
        }

        /// <summary>
        /// Checks if the task period is inside open hours.
        /// </summary>
        /// <param name="openHours">The open hours.</param>
        /// <param name="tp">The tp.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-30
        /// </remarks>
        private bool TaskPeriodIsInsideOpenHours(IEnumerable<TimePeriod> openHours, IPeriodized tp)
        {
            foreach (TimePeriod openHour in openHours)
            {
                if (openHour.Contains(tp.Period.TimePeriod(TimeZone))) 
                    return true;
            }
            return false;
        }
    }
}
