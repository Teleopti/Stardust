using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Expression;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Time;
using System.Linq;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Ropository for WorkloadDays
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2007-12-03
    /// </remarks>
    public class WorkloadDayRepository : Repository<WorkloadDayTemplate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkloadRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unitofwork</param>
        public WorkloadDayRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        //TODO: Fix this class!!!
        /// <summary>
        /// Finds the range.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="workload">The workload.</param>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-12-11
        /// </remarks>
        public ICollection<WorkloadDay> FindRange(DateTimePeriod period, Workload workload, Scenario scenario)
        {
            
            ICollection<WorkloadDay> nhibernateDays =
                Session.CreateCriteria(typeof (WorkloadDay))
                .Add(Expression.Between("Date", period.StartDateTime, period.EndDateTime))
                .Add(Expression.Eq("Workload",workload))
                .Add(Expression.Eq("Scenario", scenario))
                .SetFetchMode("OpenHourList", FetchMode.Join)
                .SetFetchMode("TaskPeriodList", FetchMode.Join)
                .SetResultTransformer(CriteriaUtil.DistinctRootEntity)
                .AddOrder(new Order("Date",true))
                .List<WorkloadDay>();

            IList<DateTime> criteriaDates = period.DateCollection();
            int numberOfDaysInCreateria = criteriaDates.Count;


            //Do we need to createTemplates?
            if(numberOfDaysInCreateria!=nhibernateDays.Count)
            { 
                foreach (DateTime criteriaDate in criteriaDates)
                {
                    bool dayFound = false;
                    foreach (WorkloadDay day in nhibernateDays)
                    {
                        
                        if(criteriaDate==day.WorkloadDate)
                        {
                            dayFound = true;
                            break;
                        }
                    }
                    if(!dayFound)
                    {
                         //Skapa Wlday

                        //WorkloadTemplateWeekday workloadTemplateWeekday = workload.WorkloadTemplateWeekCollection[criteriaDate.DayOfWeek];
                        //IList<TimePeriod> timePeriods;
                        //timePeriods = workload.WorkloadTemplateWeekCollection[criteriaDate.DayOfWeek].OpenHours;
                        //WorkloadDay workloadDay = new WorkloadDay();
                        //    (criteriaDate, workload, scenario, workloadTemplateWeekday.TemplateTaskPeriods, timePeriods);

                        ////Add
                        //nhibernateDays.Add(workloadDay);
                    }
                  
                }
            }
            //Borde sorteras, eftersom jag addar i denna, Linq?
            //IEnumerable<WorkloadDay> sorted = from workloadDay in nhibernateDays
            //    orderby workloadDay.WorkloadDate
            //    select workloadDay;
            nhibernateDays = (ICollection<WorkloadDay>)nhibernateDays.OrderBy(wd => wd.WorkloadDate).ToList();
            return nhibernateDays;
        }
    }
}