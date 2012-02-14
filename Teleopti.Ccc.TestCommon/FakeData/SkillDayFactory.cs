using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creates SkillDay test date
    /// </summary>
    public static class SkillDayFactory
    {
        public static ISkillDay CreateSkillDay(DateTime dt)
        {
            ISkill skill = new Skill("skill1", "skill1", Color.FromArgb(255), 15, SkillTypeFactory.CreateSkillType());
            return CreateSkillDay(skill, dt);
        }

        public static ISkillDay CreateSkillDay(DateTime dt, IList<TimePeriod> openHours)
        {
            ISkill skill = new Skill("skill1", "skill1", Color.FromArgb(255), 15, SkillTypeFactory.CreateSkillType());
            IWorkload workload = new Workload(skill);
            IWorkloadDay workloadDay = new WorkloadDay();
            var date = new DateOnly(TimeZoneHelper.ConvertFromUtc(dt, skill.TimeZone));
            workloadDay.Create(date, workload, openHours);
            IList<IWorkloadDay> workloadDays = new List<IWorkloadDay>();
            workloadDays.Add(workloadDay);
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            if (!skill.Id.HasValue) skill.SetId(Guid.NewGuid());
            return new SkillDay(date, skill, scenario, workloadDays, new List<ISkillDataPeriod>());
        }

        public static ISkillDay CreateSkillDay(ISkill skill, DateTime dt, IScenario scenario)
        {
            IList<ISkillDataPeriod> skillDataPeriods = new List<ISkillDataPeriod>();
            skillDataPeriods.Add(
                new SkillDataPeriod(
                    new ServiceAgreement(
                        new ServiceLevel(
                            new Percent(0.8), 20),
                                new Percent(0.5),
                                new Percent(0.7)),
                                new SkillPersonData(),
                                TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(dt.Add(TimeSpan.FromHours(4)), dt.Add(TimeSpan.FromHours(19)),skill.TimeZone)));

            if (!skill.Id.HasValue) skill.SetId(Guid.NewGuid());

            var date = new DateOnly(TimeZoneHelper.ConvertFromUtc(dt, skill.TimeZone));
            return new SkillDay(date, skill, scenario, WorkloadDayFactory.GetWorkloadDaysForTest(dt, skill), skillDataPeriods);
        }

        public static ISkillDay CreateSkillDay(ISkill skill, DateTime dt)
        {
            return CreateSkillDay(skill, dt, ScenarioFactory.CreateScenarioAggregate());
        }

        public static ISkillDay CreateSkillDay(ISkill skill, DateTime dt, IWorkload workload, IWorkload workload2)
        {
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate();
            IList<ISkillDataPeriod> skillDataPeriods = new List<ISkillDataPeriod>();
            skillDataPeriods.Add(
                new SkillDataPeriod(
                    new ServiceAgreement(
                        new ServiceLevel(
                            new Percent(0.8), 20),
                                new Percent(0.5),
                                new Percent(0.7)),
                                new SkillPersonData(),
                                new DateTimePeriod(dt.Add(TimeSpan.FromHours(4)), dt.Add(TimeSpan.FromHours(19)))));

            if(!skill.Id.HasValue) skill.SetId(Guid.NewGuid());

            var date = new DateOnly(TimeZoneHelper.ConvertFromUtc(dt, skill.TimeZone));
            ISkillDay skillDay = new SkillDay(date, skill, scenario,
                                             WorkloadDayFactory.GetWorkloadDaysForTest(dt, workload, workload2),
                                             skillDataPeriods);
            return skillDay;
        }


        /// <summary>
        /// Creates skill days for activity divider test.
        /// </summary>
        /// <param name="skills">The contained skills.</param>
        /// <returns></returns>
        public static ISkillSkillStaffPeriodExtendedDictionary CreateSkillDaysForActivityDividerTest(IDictionary<string, ISkill> skills)
        {
            DateTime dateTime = new DateTime(2008, 1, 2, 10, 00, 00, DateTimeKind.Utc);

            return getSkillStaffDictionary(skills, dateTime);
        }

        public static ISkillSkillStaffPeriodExtendedDictionary CreateSkillDaysForActivityDividerTest(IDictionary<string, ISkill> skills, DateTime skillDayDate)
        {
            return getSkillStaffDictionary(skills, skillDayDate);
        }

        private static ISkillSkillStaffPeriodExtendedDictionary getSkillStaffDictionary(IDictionary<string, ISkill> skills, DateTime skillDayDate)
        {
            ISkillSkillStaffPeriodExtendedDictionary retDic = new SkillSkillStaffPeriodExtendedDictionary();

            retDic.Add(skills["PhoneA"], prepareSkillDay(skills["PhoneA"], skillDayDate, 5));
            retDic.Add(skills["PhoneB"], prepareSkillDay(skills["PhoneB"], skillDayDate, 10));
            retDic.Add(skills["PhoneC"], prepareSkillDay(skills["PhoneC"], skillDayDate, 0));

            return retDic;
        }

        private static ISkillStaffPeriodDictionary prepareSkillDay(ISkill skill,DateTime dateTime, double forecastValue)
        {
            ISkillStaffPeriod skillStaffPeriod = SkillStaffPeriodFactory.CreateSkillStaffPeriod(skill, dateTime, forecastValue, 0d);

            return new SkillStaffPeriodDictionary(skill) { { skillStaffPeriod.Period, skillStaffPeriod } };
        }

        public static ISkillSkillStaffPeriodExtendedDictionary CreateSkillDaysForActivityDividerTest2(IDictionary<string, ISkill> skills)
        {
            DateTime dateTime = new DateTime(2008, 1, 2, 10, 00, 00, DateTimeKind.Utc);

            ISkillSkillStaffPeriodExtendedDictionary retList = new SkillSkillStaffPeriodExtendedDictionary();

            retList.Add(skills["PhoneA"], prepareSkillDay(skills["PhoneA"], dateTime, 5));
            var secondPeriodForPhoneA = prepareSkillDay(skills["PhoneA"], dateTime.AddDays(1), 5);
            retList[skills["PhoneA"]].Add(secondPeriodForPhoneA.First().Key, secondPeriodForPhoneA.First().Value);
            retList.Add(skills["PhoneB"], prepareSkillDay(skills["PhoneB"], dateTime, 10));
            retList.Add(skills["PhoneC"], prepareSkillDay(skills["PhoneC"], dateTime, 0));

            return retList;
        }

    	public static TaskOwnerHelper GenerateStatistics(DateTime startDate, IWorkload workload)
        {
            IList<IStatisticTask> statisticTasks = new List<IStatisticTask>();
            IList<IWorkloadDay> workloadDays = new List<IWorkloadDay>();
            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);

            for (int i = 0; i < 24; i++)
            {
                DateTime date = startDate.AddMonths(i);

                StatisticTask statisticTask1 = new StatisticTask();
                if (i > 6)
                {
                    statisticTask1.Interval = date.AddHours(8);
                    statisticTask1.StatAbandonedTasks = 0;
                    statisticTask1.StatAnsweredTasks = 40;
                    statisticTask1.StatAverageAfterTaskTimeSeconds = 100;
                    statisticTask1.StatAverageTaskTimeSeconds = 10;
                    statisticTask1.StatOfferedTasks = 40;
                    statisticTask1.StatCalculatedTasks = 40;
                }

                StatisticTask statisticTask2 = new StatisticTask();
                statisticTask2.Interval = date.AddHours(9);
                statisticTask2.StatAbandonedTasks = 0;
                statisticTask2.StatAnsweredTasks = 60;
                statisticTask2.StatAverageAfterTaskTimeSeconds = 50;
                statisticTask2.StatAverageTaskTimeSeconds = 20;
                statisticTask2.StatOfferedTasks = 60;
                statisticTask2.StatCalculatedTasks = 60;

                StatisticTask statisticTask3 = new StatisticTask();
                statisticTask3.Interval = date.AddHours(10);
                statisticTask3.StatAbandonedTasks = 0;
                statisticTask3.StatAnsweredTasks = 50;
                statisticTask3.StatAverageAfterTaskTimeSeconds = 50;
                statisticTask3.StatAverageTaskTimeSeconds = 20;
                statisticTask3.StatOfferedTasks = 50;
                statisticTask3.StatCalculatedTasks = 50;

                statisticTasks.Add(statisticTask1);
                statisticTasks.Add(statisticTask2);
                statisticTasks.Add(statisticTask3);

                IWorkloadDay workloadDay = WorkloadDayFactory.GetWorkloadDaysForTest(date, date, workload)[0];
                //workloadDay.MergeTemplateTaskPeriods(workloadDay.TaskPeriodList.ToList());
                workloadDays.Add(workloadDay);
            }
            TaskOwnerHelper period = new TaskOwnerHelper(workloadDays);
            period.BeginUpdate();
            new Statistic(workload).Match(workloadDays.OfType<IWorkloadDayBase>(), statisticTasks);
            period.EndUpdate();

            return period;
        }

        public static TaskOwnerHelper GenerateMockedStatistics(DateTime startDate, IWorkload workload)
        {
            IList<IWorkloadDay> workloadDays = new List<IWorkloadDay>();
            startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);

            for (int i = 0; i < 24; i++)
            {
                DateTime date = startDate.AddMonths(i);
            
                IWorkloadDay workloadDay = new WorkloadDay();
                workloadDay.Create(new DateOnly(date),workload,new List<TimePeriod>{new TimePeriod(8,0,8,15)});
                
                workloadDay.Tasks = 30;
                workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(3);
                workloadDay.AverageTaskTime=TimeSpan.FromSeconds(2);

                if (i > 6)
                {
                    workloadDay.TaskPeriodList[0].StatisticTask.StatAbandonedTasks = 0;
                    workloadDay.TaskPeriodList[0].StatisticTask.StatAnsweredTasks = 110;
                    workloadDay.TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 10;
                    workloadDay.TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds=20;
                    workloadDay.TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 110;
                    workloadDay.TaskPeriodList[0].StatisticTask.StatOfferedTasks = 110;
                }
                else
                {
                    workloadDay.TaskPeriodList[0].StatisticTask.StatAbandonedTasks = 0;
                    workloadDay.TaskPeriodList[0].StatisticTask.StatAnsweredTasks = 80;
                    workloadDay.TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 20;
                    workloadDay.TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 30;
                    workloadDay.TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 80;
                    workloadDay.TaskPeriodList[0].StatisticTask.StatOfferedTasks = 80;
                }

                workloadDay.Initialize();
                workloadDays.Add(workloadDay);
            }
            return new TaskOwnerHelper(workloadDays);
        }


    	public static TaskOwnerHelper GenerateStatisticsForWeekTests(DateTime startDate, IWorkload workload)
        {
            IList<IWorkloadDay> workloadDays = new List<IWorkloadDay>();
            IList<IStatisticTask> statisticTasks = new List<IStatisticTask>();
            DateTime date = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            
            for (int i = 0; i < 10; i++)
            {
                if (i == 4 || i==9)
                    date = date.AddDays(2);
                else
                    date = date.AddDays(7);

                StatisticTask statisticTask1 = new StatisticTask();
                if (i > 6)
                {
                    statisticTask1.Interval = date.AddHours(8);
                    statisticTask1.StatAbandonedTasks = 0;
                    statisticTask1.StatAnsweredTasks = 40;
                    statisticTask1.StatAverageAfterTaskTimeSeconds = 100;
                    statisticTask1.StatAverageTaskTimeSeconds = 10;
                    statisticTask1.StatCalculatedTasks = 40;
                    statisticTask1.StatOfferedTasks = 40;
                }

                StatisticTask statisticTask2 = new StatisticTask();
                statisticTask2.Interval = date.AddHours(9);
                statisticTask2.StatAbandonedTasks = 0;
                statisticTask2.StatAnsweredTasks = 60;
                statisticTask2.StatAverageAfterTaskTimeSeconds = 50;
                statisticTask2.StatAverageTaskTimeSeconds = 20;
                statisticTask2.StatCalculatedTasks = 60;
                statisticTask2.StatOfferedTasks = 60;

                StatisticTask statisticTask3 = new StatisticTask();
                statisticTask3.Interval = date.AddHours(10);
                statisticTask3.StatAbandonedTasks = 0;
                statisticTask3.StatAnsweredTasks = 50;
                statisticTask3.StatAverageAfterTaskTimeSeconds = 50;
                statisticTask3.StatAverageTaskTimeSeconds = 20;
                statisticTask3.StatCalculatedTasks = 50;
                statisticTask3.StatOfferedTasks = 50;

                statisticTasks.Add(statisticTask1);
                statisticTasks.Add(statisticTask2);
                statisticTasks.Add(statisticTask3);

                IWorkloadDay workloadDay = WorkloadDayFactory.GetWorkloadDaysForTest(date, date, workload)[0];
                //workloadDay.MergeTemplateTaskPeriods(workloadDay.TaskPeriodList.ToList());
                workloadDays.Add(workloadDay);

            }
            TaskOwnerHelper period = new TaskOwnerHelper(workloadDays);
            period.BeginUpdate();
            new Statistic(workload).Match(workloadDays.OfType<IWorkloadDayBase>(), statisticTasks);
            period.EndUpdate();

            return period;
        }

        public static TaskOwnerHelper GenerateMockedStatisticsForWeekTests(MockRepository mocks, DateTime startDate, IWorkload workload, TimeZoneInfo timeZone)
        {
            IList<IWorkloadDay> workloadDays = new List<IWorkloadDay>();
            DateTime date = startDate;

            for (int i = 0; i < 10; i++)
            {
                if (i == 4 || i == 9)
                    date = date.AddDays(2);
                else
                    date = date.AddDays(7);

                IWorkloadDay workloadDay = mocks.StrictMock<IWorkloadDay>();

                Expect.Call(workloadDay.CurrentDate).Return(new DateOnly(date)).Repeat.Any();
                Expect.Call(workloadDay.Workload).Return(workload).Repeat.Any();
                Expect.Call(workloadDay.IsLocked).Return(false).Repeat.Any();
                Expect.Call(workloadDay.IsClosed).Return(false).Repeat.Any();
                Expect.Call(workloadDay.TotalTasks).Return(30).Repeat.Any();
                Expect.Call(workloadDay.TotalAverageAfterTaskTime).Return(TimeSpan.FromSeconds(3)).Repeat.Any();
                Expect.Call(workloadDay.TotalAverageTaskTime).Return(TimeSpan.FromSeconds(2)).Repeat.Any();
                Expect.Call(workloadDay.Tasks).Return(30).Repeat.Any();
                Expect.Call(workloadDay.AverageAfterTaskTime).Return(TimeSpan.FromSeconds(3)).Repeat.Any();
                Expect.Call(workloadDay.AverageTaskTime).Return(TimeSpan.FromSeconds(2)).Repeat.Any();

                workloadDay.AddParent(null);
                LastCall.IgnoreArguments().Repeat.Any();

                if (i > 6)
                {
                    Expect.Call(workloadDay.TotalStatisticAbandonedTasks).Return(0).Repeat.Any();
                    Expect.Call(workloadDay.TotalStatisticAnsweredTasks).Return(110).Repeat.Any();
                    Expect.Call(workloadDay.TotalStatisticAverageAfterTaskTime).Return(TimeSpan.FromSeconds(10)).Repeat.Any();
                    Expect.Call(workloadDay.TotalStatisticAverageTaskTime).Return(TimeSpan.FromSeconds(20)).Repeat.Any();
                    Expect.Call(workloadDay.TotalStatisticCalculatedTasks).Return(110).Repeat.Any();
                }
                else
                {
                    Expect.Call(workloadDay.TotalStatisticAbandonedTasks).Return(0).Repeat.Any();
                    Expect.Call(workloadDay.TotalStatisticAnsweredTasks).Return(80).Repeat.Any();
                    Expect.Call(workloadDay.TotalStatisticAverageAfterTaskTime).Return(TimeSpan.FromSeconds(20)).Repeat.Any();
                    Expect.Call(workloadDay.TotalStatisticAverageTaskTime).Return(TimeSpan.FromSeconds(30)).Repeat.Any();
                    Expect.Call(workloadDay.TotalStatisticCalculatedTasks).Return(80).Repeat.Any();
                }

                workloadDays.Add(workloadDay);

            }
            return new TaskOwnerHelper(workloadDays);
        }

    	public static TaskOwnerHelper GenerateStatisticsForDayTests(DateTime startDate, IWorkload workload)
        {
            IList<ITaskOwner> workloadDays = new List<ITaskOwner>();
            IList<IStatisticTask> statisticTasks = new List<IStatisticTask>();
            DateTime date = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);

            for (int i = 0; i < 14; i++)
            {
                date = date.AddDays(1);

                StatisticTask statisticTask1 = new StatisticTask();
                if (i == 0 )
                {
                    statisticTask1.Interval = date.AddHours(8);
                    statisticTask1.StatAbandonedTasks = 0;
                    statisticTask1.StatAnsweredTasks = 40;
                    statisticTask1.StatAverageAfterTaskTimeSeconds = 100;
                    statisticTask1.StatAverageTaskTimeSeconds = 10;
                    statisticTask1.StatCalculatedTasks = 40;
                    statisticTask1.StatOfferedTasks = 40;
                }

                StatisticTask statisticTask2 = new StatisticTask();
                statisticTask2.Interval = date.AddHours(9);
                statisticTask2.StatAbandonedTasks = 0;
                statisticTask2.StatAnsweredTasks = 60;
                statisticTask2.StatAverageAfterTaskTimeSeconds = 50;
                statisticTask2.StatAverageTaskTimeSeconds = 20;
                statisticTask2.StatCalculatedTasks = 60;
                statisticTask2.StatOfferedTasks = 60;

                StatisticTask statisticTask3 = new StatisticTask();
                statisticTask3.Interval = date.AddHours(10);
                statisticTask3.StatAbandonedTasks = 0;
                statisticTask3.StatAnsweredTasks = 50;
                statisticTask3.StatAverageAfterTaskTimeSeconds = 50;
                statisticTask3.StatAverageTaskTimeSeconds = 20;
                statisticTask3.StatCalculatedTasks = 50;
                statisticTask3.StatOfferedTasks= 50;

                statisticTasks.Add(statisticTask1);
                statisticTasks.Add(statisticTask2);
                statisticTasks.Add(statisticTask3);

                IWorkloadDay workloadDay = WorkloadDayFactory.GetWorkloadDaysForTest(date, date, workload)[0];
                //workloadDay.MergeTemplateTaskPeriods(workloadDay.TaskPeriodList.ToList());
                workloadDays.Add(workloadDay);

            }
            TaskOwnerHelper period = new TaskOwnerHelper(workloadDays);
            period.BeginUpdate();
            new Statistic(workload).Match(workloadDays.OfType<IWorkloadDayBase>(), statisticTasks);
            period.EndUpdate();

            return period;
        }

        public static TaskOwnerHelper GenerateMockedStatisticsForDayTests(MockRepository mocks,DateTime startDate, IWorkload workload, TimeZoneInfo timeZone)
        {
            IList<ITaskOwner> workloadDays = new List<ITaskOwner>();
            DateTime date = startDate;

            for (int i = 0; i < 14; i++)
            {
                date = date.AddDays(1);

                IWorkloadDay workloadDay = mocks.StrictMock<IWorkloadDay>();

                Expect.Call(workloadDay.CurrentDate).Return(new DateOnly(date)).Repeat.Any();
                Expect.Call(workloadDay.Workload).Return(workload).Repeat.Any();
                Expect.Call(workloadDay.IsLocked).Return(false).Repeat.Any();
                Expect.Call(workloadDay.IsClosed).Return(false).Repeat.Any();
                Expect.Call(workloadDay.TotalTasks).Return(30).Repeat.Any();
                Expect.Call(workloadDay.TotalAverageAfterTaskTime).Return(TimeSpan.FromSeconds(3)).Repeat.Any();
                Expect.Call(workloadDay.TotalAverageTaskTime).Return(TimeSpan.FromSeconds(2)).Repeat.Any();
                Expect.Call(workloadDay.Tasks).Return(30).Repeat.Any();
                Expect.Call(workloadDay.AverageAfterTaskTime).Return(TimeSpan.FromSeconds(3)).Repeat.Any();
                Expect.Call(workloadDay.AverageTaskTime).Return(TimeSpan.FromSeconds(2)).Repeat.Any();

                workloadDay.AddParent(null);
                LastCall.IgnoreArguments().Repeat.Any();

                if (i == 0)
                {
                    Expect.Call(workloadDay.TotalStatisticAbandonedTasks).Return(40).Repeat.Any();
                    Expect.Call(workloadDay.TotalStatisticAnsweredTasks).Return(110).Repeat.Any();
                    Expect.Call(workloadDay.TotalStatisticAverageAfterTaskTime).Return(TimeSpan.FromSeconds(10)).Repeat.Any();
                    Expect.Call(workloadDay.TotalStatisticAverageTaskTime).Return(TimeSpan.FromSeconds(20)).Repeat.Any();
                    Expect.Call(workloadDay.TotalStatisticCalculatedTasks).Return(110).Repeat.Any();
                }
                else
                {
                    Expect.Call(workloadDay.TotalStatisticAbandonedTasks).Return(30).Repeat.Any();
                    Expect.Call(workloadDay.TotalStatisticAnsweredTasks).Return(80).Repeat.Any();
                    Expect.Call(workloadDay.TotalStatisticAverageAfterTaskTime).Return(TimeSpan.FromSeconds(20)).Repeat.Any();
                    Expect.Call(workloadDay.TotalStatisticAverageTaskTime).Return(TimeSpan.FromSeconds(30)).Repeat.Any();
                    Expect.Call(workloadDay.TotalStatisticCalculatedTasks).Return(80).Repeat.Any();
                }

                workloadDays.Add(workloadDay);

            }
            return new TaskOwnerHelper(workloadDays);
        }
    }
}
