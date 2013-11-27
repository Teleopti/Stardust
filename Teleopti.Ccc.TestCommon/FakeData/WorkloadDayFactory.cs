using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class WorkloadDayFactory
    {
			public static IList<IWorkloadDay> GetWorkloadDaysForTest(DateTime dt, ISkill skill, bool setIdOnWorkLoad)
			{
				IList<IWorkloadDay> workloadDays = new List<IWorkloadDay>();
				IWorkload workload1 = new Workload(skill);
				if(setIdOnWorkLoad)
					workload1.SetId(Guid.NewGuid());

				IList<ITemplateTaskPeriod> taskPeriods = new List<ITemplateTaskPeriod>();
				ITemplateTaskPeriod templateTaskPeriod =
						new TemplateTaskPeriod(new Task(100, TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(20)),
																	 TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
																			 dt.Add(TimeSpan.FromHours(11)), dt.Add(TimeSpan.FromHours(14)),
																			 skill.TimeZone));

				taskPeriods.Add(templateTaskPeriod);

				WorkloadDay workloadDay = new WorkloadDay();
				workloadDay.Create(new DateOnly(TimeZoneHelper.ConvertFromUtc(dt, skill.TimeZone)), workload1, new List<TimePeriod>());
				workloadDay.Lock();
				workloadDay.MakeOpen24Hours();

				//Add some tasks between 11 - 14
				foreach (ITemplateTaskPeriod taskPeriod in workloadDay.SortedTaskPeriodList)
				{
					if (taskPeriod.Period.StartDateTime >= dt.Add(new TimeSpan(11, 0, 0)) && taskPeriod.Period.EndDateTime <= dt.Add(new TimeSpan(14, 0, 0)))
					{
						taskPeriod.SetTasks(100d / 12d);
						taskPeriod.AverageTaskTime = TimeSpan.FromSeconds(120);
						taskPeriod.AverageAfterTaskTime = TimeSpan.FromSeconds(20);
					}
				}

				workloadDay.Release();
				workloadDays.Add(workloadDay);

				IWorkload workload2 = new Workload(skill);
				if(setIdOnWorkLoad)
					workload2.SetId(Guid.NewGuid());

				taskPeriods = new List<ITemplateTaskPeriod>();
				templateTaskPeriod = new TemplateTaskPeriod(new Task(300, TimeSpan.FromSeconds(240), TimeSpan.FromSeconds(40)),
								TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
																			 dt.Add(TimeSpan.FromHours(13)), dt.Add(TimeSpan.FromHours(17)),
																			 skill.TimeZone));
				taskPeriods.Add(templateTaskPeriod);
				workloadDay = new WorkloadDay();
				workloadDay.Create(new DateOnly(TimeZoneHelper.ConvertFromUtc(dt, skill.TimeZone)), workload2, new List<TimePeriod>());
				workloadDay.Lock();
				workloadDay.MakeOpen24Hours();
				foreach (ITemplateTaskPeriod taskPeriod in workloadDay.SortedTaskPeriodList)
				{
					if (taskPeriod.Period.StartDateTime >= dt.Add(new TimeSpan(13, 0, 0)) && taskPeriod.Period.EndDateTime <= dt.Add(new TimeSpan(17, 0, 0)))
					{
						taskPeriod.SetTasks(300d / 16d);
						taskPeriod.AverageTaskTime = TimeSpan.FromSeconds(240);
						taskPeriod.AverageAfterTaskTime = TimeSpan.FromSeconds(40);
					}
				}
				workloadDay.Release();
				workloadDays.Add(workloadDay);
				return workloadDays;
			}

        public static IList<IWorkloadDay> GetWorkloadDaysForTest(DateTime dt, ISkill skill)
        {
	        return GetWorkloadDaysForTest(dt, skill, false);
        }

        //Creates a list of 2 Workload days 1 with sent in Workload and one with random guid
        public static IList<IWorkloadDay> GetWorkloadDaysForTest(DateTime dt, IWorkload workload, IWorkload workload2)
        {
            IList<IWorkloadDay> workloadDays = new List<IWorkloadDay>();

            IList<ITemplateTaskPeriod> taskPeriods = new List<ITemplateTaskPeriod>();
            IList<TimePeriod> openHours = new List<TimePeriod>
                                              {
                                                  new TimePeriod(workload.Skill.MidnightBreakOffset,
                                                                 workload.Skill.MidnightBreakOffset.Add(
                                                                     TimeSpan.FromDays(1)))
                                              };

            ITemplateTaskPeriod templateTaskPeriod = new TemplateTaskPeriod(new Task(100, TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(20)),
                    new DateTimePeriod(dt.Add(TimeSpan.FromHours(11)), dt.Add(TimeSpan.FromHours(14))));

            taskPeriods.Add(templateTaskPeriod);

            WorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Lock();
            workloadDay.Create(new DateOnly(TimeZoneHelper.ConvertFromUtc(dt, workload.Skill.TimeZone)), workload, openHours);
            
            //Add some tasks between 11 - 14
            foreach (ITemplateTaskPeriod taskPeriod in workloadDay.SortedTaskPeriodList)
            {
                if (taskPeriod.Period.StartDateTime >= dt.Add(new TimeSpan(11, 0, 0)) && taskPeriod.Period.EndDateTime <= dt.Add(new TimeSpan(14, 0, 0)))
                {
                    taskPeriod.SetTasks(100d / 12d);
                    taskPeriod.AverageTaskTime = TimeSpan.FromSeconds(120);
                    taskPeriod.AverageAfterTaskTime = TimeSpan.FromSeconds(20);
                }
            }

            workloadDay.Release();
            workloadDays.Add(workloadDay);

            taskPeriods = new List<ITemplateTaskPeriod>();
            openHours = new List<TimePeriod>();
            templateTaskPeriod = new TemplateTaskPeriod(new Task(300, TimeSpan.FromSeconds(240), TimeSpan.FromSeconds(40)),
                    new DateTimePeriod(dt.Add(TimeSpan.FromHours(13)), dt.Add(TimeSpan.FromHours(17))));
            taskPeriods.Add(templateTaskPeriod);
            openHours.Add(new TimePeriod(workload.Skill.MidnightBreakOffset,
                                         workload.Skill.MidnightBreakOffset.Add(
                                             TimeSpan.FromDays(1))));
            workloadDay = new WorkloadDay();
            workloadDay.Lock();
            workloadDay.Create(new DateOnly(TimeZoneHelper.ConvertFromUtc(dt, workload2.Skill.TimeZone)), workload2, openHours);
            
            foreach (ITemplateTaskPeriod taskPeriod in workloadDay.SortedTaskPeriodList)
            {
                if (taskPeriod.Period.StartDateTime >= dt.Add(new TimeSpan(13, 0, 0)) && taskPeriod.Period.EndDateTime <= dt.Add(new TimeSpan(17, 0, 0)))
                {
                    taskPeriod.SetTasks(300d / 16d);
                    taskPeriod.AverageTaskTime = TimeSpan.FromSeconds(240);
                    taskPeriod.AverageAfterTaskTime = TimeSpan.FromSeconds(40);
                }
            }

            workloadDay.Release();
            workloadDays.Add(workloadDay);

            return workloadDays;
        }

        /// <summary>
        /// Gets the workload days for test.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="workload">The workload.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-02
        /// </remarks>
        public static IList<IWorkloadDay> GetWorkloadDaysForTest(DateTime startDate, DateTime endDate, IWorkload workload)
        {
            IList<IWorkloadDay> workloadDays = new List<IWorkloadDay>();

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                var template = ((IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, day));
                template.Lock();
                template.MakeOpen24Hours();
                template.Release();
            }

            DateTime currentDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            int i = 0;
            do
            {
                WorkloadDay workloadDay = new WorkloadDay();
                workloadDay.Create(new DateOnly(TimeZoneHelper.ConvertFromUtc(currentDate, workload.Skill.TimeZone)), workload, new List<TimePeriod>());
                workloadDay.Lock();
                workloadDay.MakeOpen24Hours();
                foreach (ITemplateTaskPeriod taskPeriod in workloadDay.TaskPeriodList)
                {
                    taskPeriod.AverageAfterTaskTime = TimeSpan.FromSeconds((i % 4) + 3);
                    taskPeriod.AverageTaskTime = TimeSpan.FromSeconds((i % 3) + 2);
                    taskPeriod.SetTasks(((i % 2) + 30d) / 96d);
                }
                workloadDay.Release();

                workloadDays.Add(workloadDay);
                currentDate = currentDate.AddDays(1);
                i++;
            } while (currentDate <= endDate) ;

            return workloadDays;
        }

        public static IList<IWorkloadDay> GetWorkloadDaysWithoutContentForTest(DateTime startDate, DateTime endDate, IWorkload workload)
        {
            IList<IWorkloadDay> workloadDays = new List<IWorkloadDay>();

            DateTime currentDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            do
            {
                WorkloadDay workloadDay = new WorkloadDay();
                workloadDay.Create(new DateOnly(TimeZoneHelper.ConvertFromUtc(currentDate, workload.Skill.TimeZone)), workload, new List<TimePeriod>());
                workloadDays.Add(workloadDay);
                currentDate = currentDate.AddDays(1);
            } while (currentDate <= endDate);

            return workloadDays;
        }
    }
}
