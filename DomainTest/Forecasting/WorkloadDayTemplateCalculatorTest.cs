using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class WorkloadDayTemplateCalculatorTest
    {
        private MockRepository _mocks;
        private ISkillType _skillType;
        private ISkill _skill;
        private IStatisticHelper _statisticHelper;
        private IOutlierRepository _outlierRepMock;
        private WorkloadDayTemplateCalculator _target;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _skillType = SkillTypeFactory.CreateSkillType();
            _skill = SkillFactory.CreateSkill("Skill - Name", _skillType, 15);
            _skill.MidnightBreakOffset = TimeSpan.FromHours(2);

            _outlierRepMock = _mocks.StrictMock<IOutlierRepository>();
            _statisticHelper = _mocks.StrictMock<IStatisticHelper>();
            
            _target = new WorkloadDayTemplateCalculator(_statisticHelper, _outlierRepMock);
        }

        //These test are kind of hard and mabe to big to understand, but allt the metods 
        //used in this test are also tested, but what these tests do
        //is to create skill days and mach them with data from statistics and in the end
        //we compare and make sure that the templates holds the average data for the skilldays sent in
        //Sample: Friday 1 has 20 tasks and Friday 2 has 30 task -> The Template Friday will have 25 tasks
        // Peter, Zoe
        [Test]
        public void CanLoadTemplateDays()
        {
            IWorkload workload = LoadWorkload();
            var scenario = ScenarioFactory.CreateScenarioAggregate();

            DateOnly startDate = new DateOnly(2008, 2, 8);
            DateOnly endDate = new DateOnly(2008, 2, 15);
            DateTime startDateUtc = new DateTime(2008, 2, 8, 0, 0, 0, DateTimeKind.Utc);

            IList<IWorkloadDay> workloadDays1 = WorkloadDayFactory.GetWorkloadDaysForTest(startDateUtc, startDateUtc, workload);
            IList<IWorkloadDay> workloadDays2 = WorkloadDayFactory.GetWorkloadDaysForTest(startDateUtc.AddDays(7), startDateUtc.AddDays(7), workload);

            //Fredag
            SkillDay skillDay1 = new SkillDay(startDate, _skill, scenario, workloadDays1, new List<ISkillDataPeriod>());
            //Fredag
            SkillDay skillDay2 = new SkillDay(startDate.AddDays(7), _skill, scenario, workloadDays2, new List<ISkillDataPeriod>());

            skillDay1.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 20;
            skillDay1.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 20;
            skillDay1.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 10;

            skillDay2.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 30;
            skillDay2.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 30;
            skillDay2.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 20;

            var period = new DateOnlyPeriod(startDate, endDate);

            using (_mocks.Record())
            {
                Expect.Call(_statisticHelper.LoadStatisticData(period, workload)).Return(new List<IWorkloadDayBase>
                                                                                            {
                                                                                                skillDay1.WorkloadDayCollection[0],
                                                                                                skillDay2.WorkloadDayCollection[0]
                                                                                            });
                Expect.Call(_outlierRepMock.FindByWorkload(workload))
                    .Return(new List<IOutlier>());
            }
            using (_mocks.Playback())
            {
                _target.LoadWorkloadDayTemplates(new List<DateOnlyPeriod> { new DateOnlyPeriod(startDate, endDate) }, workload);

            }

            Assert.AreEqual(7, workload.TemplateWeekCollection.Count);

            double task1 = skillDay1.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatCalculatedTasks;
            double task2 = skillDay2.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatCalculatedTasks;
            double averageTask = (task1 + task2) / 2;

            double averageTask1 =
                skillDay1.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds;
            double averageTask2 =
                skillDay2.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds;
            double averageTaskTime = (averageTask1 * task1 + averageTask2 * task2) / (task1 + task2);


            double averageAfterTask1 = skillDay1.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds;
            double averageAfterTask2 = skillDay2.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds;

            double averageAfterTaskTime = (averageAfterTask1 * task1 + averageAfterTask2 * task2) / (task1 + task2);

            WorkloadDayTemplate template = (WorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, DayOfWeek.Friday);
            Assert.AreEqual(averageTask, template.TotalTasks);
            Assert.AreEqual(TimeSpan.FromSeconds(averageTaskTime), template.AverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(averageAfterTaskTime), template.AverageAfterTaskTime);

        }

        [Test]
        public void CanAddCustomTemplateFromStatistics()
        {
            IWorkload workload = LoadWorkload();
            workload.Skill.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            
            IList<TimePeriod> openHours = new List<TimePeriod>();
            openHours.Add(new TimePeriod("02:00-1:02:00"));
            WorkloadDayTemplate template = new WorkloadDayTemplate();
            template.Create("<CUSTOM>", new DateTime(2008, 12, 26, 23, 0, 0, DateTimeKind.Utc), workload, openHours);

            workload.AddTemplate(template);
            DateOnly startDate = new DateOnly(2008, 2, 8);

            var statisticTask1 = new StatisticTask
            {
                Interval = startDate.Date,
                StatCalculatedTasks = 20,
                StatAverageTaskTimeSeconds = 20,
                StatAverageAfterTaskTimeSeconds = 10
            };
            var statisticTask2 = new StatisticTask
            {
                Interval = startDate.Date.AddDays(3),
                StatCalculatedTasks = 30,
                StatAverageTaskTimeSeconds = 30,
                StatAverageAfterTaskTimeSeconds = 20
            };

            IList<DateOnlyPeriod> periods = new List<DateOnlyPeriod>
                                                {
                                                    new DateOnlyPeriod(startDate, startDate),
                                                    new DateOnlyPeriod(startDate.AddDays(3), startDate.AddDays(3))
                                                };

            IWorkloadDay workloadDay1 = _mocks.StrictMock<IWorkloadDay>();
            ITemplateTaskPeriod templateTaskPeriod1 = _mocks.StrictMock<ITemplateTaskPeriod>();
            IWorkloadDay workloadDay2 = _mocks.StrictMock<IWorkloadDay>();
            ITemplateTaskPeriod templateTaskPeriod2 = _mocks.StrictMock<ITemplateTaskPeriod>();

            using (_mocks.Record())
            {
                Expect.Call(_statisticHelper.LoadStatisticData(periods[0], workload)).Return(new List<IWorkloadDayBase> { workloadDay1 });
                Expect.Call(_statisticHelper.LoadStatisticData(periods[1], workload)).Return(new List<IWorkloadDayBase> { workloadDay2 });
                Expect.Call(workloadDay1.CurrentDate).Return(new DateOnly());
                Expect.Call(workloadDay2.CurrentDate).Return(new DateOnly());
                Expect.Call(workloadDay2.Workload).Return(workload);
                Expect.Call(workloadDay1.Workload).Return(workload);
                Expect.Call(workloadDay1.SortedTaskPeriodList).Return(
                    new ReadOnlyCollection<ITemplateTaskPeriod>(new List<ITemplateTaskPeriod> { templateTaskPeriod1 }));
                Expect.Call(workloadDay2.SortedTaskPeriodList).Return(
                    new ReadOnlyCollection<ITemplateTaskPeriod>(new List<ITemplateTaskPeriod> { templateTaskPeriod2 }));
                Expect.Call(templateTaskPeriod1.StatisticTask).Return(statisticTask1).Repeat.AtLeastOnce();
                Expect.Call(templateTaskPeriod2.StatisticTask).Return(statisticTask2).Repeat.AtLeastOnce();
                Expect.Call(templateTaskPeriod1.Task).Return(new Task());
                Expect.Call(templateTaskPeriod2.Task).Return(new Task());
                Expect.Call(templateTaskPeriod1.Period).Return(new DateTimePeriod().MovePeriod(TimeSpan.FromHours(12))).Repeat.AtLeastOnce();
                Expect.Call(templateTaskPeriod2.Period).Return(new DateTimePeriod().MovePeriod(TimeSpan.FromHours(12))).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                _target.RecalculateWorkloadDayTemplate(periods, workload, 7);
            }

            Assert.AreEqual(8, workload.TemplateWeekCollection.Count);

            double task1 = 20;
            double task2 = 30;
            double averageTask = (task1 + task2) / 2;

            double averageTask1 = 20;
            double averageTask2 = 30;
            double averageTaskTime = (averageTask1 * task1 + averageTask2 * task2) / (task1 + task2);

            double averageAfterTask1 = 10;
            double averageAfterTask2 = 20;

            double averageAfterTaskTime = (averageAfterTask1 * task1 + averageAfterTask2 * task2) / (task1 + task2);

            WorkloadDayTemplate loadedTemplate = (WorkloadDayTemplate)workload.GetTemplateAt(TemplateTarget.Workload, 7);
            Assert.AreEqual(averageTask, loadedTemplate.TotalTasks);
            Assert.AreEqual(TimeSpan.FromSeconds(averageTaskTime), loadedTemplate.AverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(averageAfterTaskTime), loadedTemplate.AverageAfterTaskTime);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void CanAddCustomTemplateFromStatisticsWithMidnightBreak()
        {
            IWorkload workload = LoadWorkload();
            workload.Skill.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            workload.Skill.MidnightBreakOffset = TimeSpan.FromHours(8);
            
            WorkloadDayTemplate template = new WorkloadDayTemplate();
            template.Create("<CUSTOM>", new DateTime(2008, 12, 26, 23, 0, 0, DateTimeKind.Utc), workload, new List<TimePeriod>());
            template.MakeOpen24Hours();

            workload.AddTemplate(template);
            DateOnly startDate = new DateOnly(2008, 2, 8);

            var statisticTask1 = new StatisticTask
            {
                Interval = startDate.Date.AddHours(8),
                StatCalculatedTasks = 20,
                StatAverageTaskTimeSeconds = 20,
                StatAverageAfterTaskTimeSeconds = 10
            };
            var statisticTask2 = new StatisticTask
            {
                Interval = startDate.Date.AddHours(27),
                StatCalculatedTasks = 30,
                StatAverageTaskTimeSeconds = 30,
                StatAverageAfterTaskTimeSeconds = 20
            };

            DateOnlyPeriod period = new DateOnlyPeriod(startDate, startDate);

            IWorkloadDay workloadDay1 = _mocks.StrictMock<IWorkloadDay>();
            ITemplateTaskPeriod templateTaskPeriod1 = _mocks.StrictMock<ITemplateTaskPeriod>();
            ITemplateTaskPeriod templateTaskPeriod2 = _mocks.StrictMock<ITemplateTaskPeriod>();

            using (_mocks.Record())
            {
                Expect.Call(_statisticHelper.LoadStatisticData(period, workload)).Return(new List<IWorkloadDayBase> { workloadDay1 });
                Expect.Call(workloadDay1.Workload).Return(workload);
                Expect.Call(workloadDay1.CurrentDate).Return(new DateOnly()).Repeat.AtLeastOnce();
                Expect.Call(workloadDay1.SortedTaskPeriodList).Return(
                    new ReadOnlyCollection<ITemplateTaskPeriod>(new List<ITemplateTaskPeriod> { templateTaskPeriod1, templateTaskPeriod2 }));
                Expect.Call(templateTaskPeriod1.StatisticTask).Return(statisticTask1).Repeat.AtLeastOnce();
                Expect.Call(templateTaskPeriod2.StatisticTask).Return(statisticTask2).Repeat.AtLeastOnce();
                Expect.Call(templateTaskPeriod1.Task).Return(new Task());
                Expect.Call(templateTaskPeriod2.Task).Return(new Task());
                Expect.Call(templateTaskPeriod1.Period).Return(new DateTimePeriod().MovePeriod(TimeSpan.FromHours(8))).Repeat.AtLeastOnce();
                Expect.Call(templateTaskPeriod2.Period).Return(new DateTimePeriod().MovePeriod(TimeSpan.FromHours(27))).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                _target.RecalculateWorkloadDayTemplate(new List<DateOnlyPeriod> { period }, workload, 7);
            }

            Assert.AreEqual(8, workload.TemplateWeekCollection.Count);

            double averageTask1 = 20;
            double averageTask2 = 30;
            double averageTaskTime = (averageTask1 * 20 + averageTask2 * 30) / 50;

            double averageAfterTask1 = 10;
            double averageAfterTask2 = 20;

            double averageAfterTaskTime = (averageAfterTask1 * 20 + averageAfterTask2 * 30) / 50;

            IWorkloadDayTemplate loadedTemplate = (IWorkloadDayTemplate)workload.GetTemplateAt(TemplateTarget.Workload, 7);
            Assert.AreEqual(50, loadedTemplate.TotalTasks);
            Assert.AreEqual(TimeSpan.FromSeconds(averageTaskTime), loadedTemplate.AverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(averageAfterTaskTime), loadedTemplate.AverageAfterTaskTime);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifyCanCreateWorkloadDaysForStatisticsInMemory()
        {
            IWorkload workload = LoadWorkload();
            workload.Skill.TimeZone = TimeZoneInfoFactory.UtcTimeZoneInfo();
            DateOnly startDate = new DateOnly(2008, 2, 8);
            DateOnly endDate = new DateOnly(2008, 2, 15);

            var statisticTask1 = new StatisticTask
            {
                Interval = startDate.Date,
                StatCalculatedTasks = 20,
                StatAverageTaskTimeSeconds = 20,
                StatAverageAfterTaskTimeSeconds = 10
            };
            var statisticTask2 = new StatisticTask
            {
                Interval = startDate.Date.AddDays(7),
                StatCalculatedTasks = 30,
                StatAverageTaskTimeSeconds = 30,
                StatAverageAfterTaskTimeSeconds = 20
            };

            var period = new DateOnlyPeriod(startDate, endDate);

            IWorkloadDay workloadDay1 = _mocks.StrictMock<IWorkloadDay>();
            ITemplateTaskPeriod templateTaskPeriod1 = _mocks.StrictMock<ITemplateTaskPeriod>();
            ITemplateTaskPeriod templateTaskPeriod2 = _mocks.StrictMock<ITemplateTaskPeriod>();
            var taskPeriod =
                TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startDate.Date.AddHours(12),
                                                                     startDate.Date.Date.AddHours(36),
                                                                     _skill.TimeZone);
            using (_mocks.Record())
            {
                Expect.Call(_statisticHelper.LoadStatisticData(period, workload)).Return(new List<IWorkloadDayBase> { workloadDay1 });
                Expect.Call(workloadDay1.Workload).Return(workload);
                Expect.Call(workloadDay1.CurrentDate).Return(startDate).Repeat.AtLeastOnce();
                Expect.Call(workloadDay1.SortedTaskPeriodList).Return(
                    new ReadOnlyCollection<ITemplateTaskPeriod>(new List<ITemplateTaskPeriod> { templateTaskPeriod1, templateTaskPeriod2 }));
                Expect.Call(templateTaskPeriod1.StatisticTask).Return(statisticTask1).Repeat.AtLeastOnce();
                Expect.Call(templateTaskPeriod2.StatisticTask).Return(statisticTask2).Repeat.AtLeastOnce();
                Expect.Call(templateTaskPeriod1.Task).Return(new Task());
                Expect.Call(templateTaskPeriod2.Task).Return(new Task());
                Expect.Call(templateTaskPeriod1.Period).Return(taskPeriod).Repeat.AtLeastOnce();
                Expect.Call(templateTaskPeriod2.Period).Return(taskPeriod).Repeat.AtLeastOnce();
                Expect.Call(_outlierRepMock.FindByWorkload(workload))
                    .Return(new List<IOutlier>());
            }
            using (_mocks.Playback())
            {
                foreach (var workloadDayTemplate in workload.TemplateWeekCollection.Values)
                {
                    workloadDayTemplate.MakeOpen24Hours();
                }
                _target.LoadWorkloadDayTemplates(new List<DateOnlyPeriod> { period }, workload);
            }

            Assert.AreEqual(7, workload.TemplateWeekCollection.Count);

            double task1 = 20;
            double task2 = 30;
            double averageTask = (task1 + task2) / 2;

            double averageTask1 = statisticTask1.StatAverageTaskTimeSeconds;
            double averageTask2 = statisticTask2.StatAverageTaskTimeSeconds;
            double averageTaskTime = (averageTask1 * task1 + averageTask2 * task2) / (task1 + task2);

            double averageAfterTask1 = statisticTask1.StatAverageAfterTaskTimeSeconds;
            double averageAfterTask2 = statisticTask2.StatAverageAfterTaskTimeSeconds;

            double averageAfterTaskTime = (averageAfterTask1 * task1 + averageAfterTask2 * task2) / (task1 + task2);

            IWorkloadDayTemplate template = (IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, DayOfWeek.Friday);
            Assert.AreEqual(averageTask, template.TotalTasks);
            Assert.AreEqual(TimeSpan.FromSeconds(averageTaskTime), template.AverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(averageAfterTaskTime), template.AverageAfterTaskTime);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void CanLoadFilteredTemplateDays()
        {
            var workload = LoadWorkload();
            var scenario = ScenarioFactory.CreateScenarioAggregate();

            var startDate = new DateOnly(2008, 2, 8);
            var endDate = new DateOnly(2008, 2, 15);
            var startDateUtc = new DateTime(2008, 2, 8, 0, 0, 0, DateTimeKind.Utc);

            var workloadDays1 = WorkloadDayFactory.GetWorkloadDaysForTest(startDateUtc, startDateUtc, workload);
            var workloadDays2 = WorkloadDayFactory.GetWorkloadDaysForTest(startDateUtc.AddDays(7), startDateUtc.AddDays(7), workload);

            //Fredag filtered
            var skillDay1 = new SkillDay(startDate, _skill, scenario, workloadDays1, new List<ISkillDataPeriod>());
            //Fredag
            var skillDay2 = new SkillDay(startDate.AddDays(7), _skill, scenario, workloadDays2, new List<ISkillDataPeriod>());

            skillDay1.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 20;
            skillDay1.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 20;
            skillDay1.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 10;

            skillDay2.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 30;
            skillDay2.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 30;
            skillDay2.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 20;

            var period = new DateOnlyPeriod(startDate, endDate);
            
            var filteredDates = new List<DateOnly> { new DateOnly(2008, 2, 8) };
            using (_mocks.Record())
            {
                Expect.Call(_statisticHelper.LoadStatisticData(period, workload)).Return(new List<IWorkloadDayBase>
                                                                                            {
                                                                                                skillDay1.
                                                                                                    WorkloadDayCollection
                                                                                                    [0],
                                                                                                skillDay2.
                                                                                                    WorkloadDayCollection
                                                                                                    [0]
                                                                                            });
                Expect.Call(_outlierRepMock.FindByWorkload(workload))
                    .Return(new List<IOutlier>());
            }
            using (_mocks.Playback())
            {
                _target.LoadFilteredWorkloadDayTemplates(new List<DateOnlyPeriod> { new DateOnlyPeriod(startDate, endDate) }, workload, filteredDates, 5);

            }

            Assert.AreEqual(7, workload.TemplateWeekCollection.Count);

            var task2 = skillDay2.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatCalculatedTasks;

            var averageTask2 = skillDay2.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds;
            var averageTaskTime = (averageTask2 * task2) / (task2);
            var averageAfterTask2 = skillDay2.WorkloadDayCollection[0].TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds;

            var averageAfterTaskTime = (averageAfterTask2 * task2) / (task2);

            var template = (WorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, DayOfWeek.Friday);
            Assert.AreEqual(task2, template.TotalTasks);
            Assert.AreEqual(TimeSpan.FromSeconds(averageTaskTime), template.AverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(averageAfterTaskTime), template.AverageAfterTaskTime);
        }

        private IWorkload LoadWorkload()
        {
            IQueueSource q = new QueueSource("Q1", "Queue1", 1);
            IWorkload workload = WorkloadFactory.CreateWorkload(_skill);
            workload.AddQueueSource(q);
            return workload;
        }
    }
}
