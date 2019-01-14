using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
	 [TestFixture]
	 public class StatisticTest
	 {
		  //Fredag
		  private readonly DateTime _startDate = new DateTime(2007, 1, 5, 0, 0, 0, DateTimeKind.Utc);
		  private readonly DateTime _endDate = new DateTime(2007, 1, 5, 0, 0, 0, DateTimeKind.Utc);
		  private IList<IWorkloadDay> _workloadDays;
		  private ISkillType _skillType;
		  private ISkill _skill;
		  private IWorkload _workload;
		  private Statistic _target;

		  [SetUp]
		  public void Setup()
		  {
				_skillType = SkillTypeFactory.CreateSkillTypePhone();
				_skill = SkillFactory.CreateSkill("Inkommande", _skillType, 15);
				_workload = WorkloadFactory.CreateWorkload(_skill);
				_workloadDays = WorkloadDayFactory.GetWorkloadDaysForTest(_startDate, _endDate, _workload);
				_target = new Statistic(_workload);
		  }

		  [Test]
		  public void CanMatchStatistics()
		  {
				IList<IStatisticTask> statisticTasks = new List<IStatisticTask>();

				StatisticTask statisticTask1 = new StatisticTask();
				statisticTask1.Interval = new DateTime(2007,1,5,8,0,0,DateTimeKind.Utc);
				statisticTask1.StatAbandonedTasks = 10;
				statisticTask1.StatAnsweredTasks = 30;
				statisticTask1.StatAverageAfterTaskTimeSeconds = 40;
				statisticTask1.StatAverageTaskTimeSeconds = 20;
				statisticTask1.StatCalculatedTasks = 40;

				StatisticTask statisticTask2 = new StatisticTask();
				statisticTask2.Interval = new DateTime(2007, 1, 5, 8, 15, 0, DateTimeKind.Utc);
				statisticTask2.StatAbandonedTasks = 20;
				statisticTask2.StatAnsweredTasks = 40;
				statisticTask2.StatAverageAfterTaskTimeSeconds = 50;
				statisticTask2.StatAverageTaskTimeSeconds = 30;
				statisticTask2.StatCalculatedTasks = 60;

				StatisticTask statisticTask3 = new StatisticTask();
				statisticTask3.Interval = new DateTime(2007, 1, 5, 8, 30, 0, DateTimeKind.Utc);
				statisticTask3.StatAbandonedTasks = 10;
				statisticTask3.StatAnsweredTasks = 40;
				statisticTask3.StatAverageAfterTaskTimeSeconds = 50;
				statisticTask3.StatAverageTaskTimeSeconds = 30;
				statisticTask3.StatCalculatedTasks = 50;

				statisticTasks.Add(statisticTask1);
				statisticTasks.Add(statisticTask2);
				statisticTasks.Add(statisticTask3);
			
				_target.Match(_workloadDays.OfType<IWorkloadDayBase>(), statisticTasks);

				Assert.AreEqual(statisticTask1, _workloadDays[0].SortedTaskPeriodList[32].StatisticTask);
				Assert.AreEqual(statisticTask2, _workloadDays[0].SortedTaskPeriodList[33].StatisticTask);
				Assert.AreEqual(statisticTask3, _workloadDays[0].SortedTaskPeriodList[34].StatisticTask);
				Assert.AreNotEqual(statisticTask1, _workloadDays[0].SortedTaskPeriodList[34].StatisticTask);
		  }
		  [Test]
		  public void CanHandleSameIntervalAndQueueTime()
		  {
				IList<IStatisticTask> statisticTasks = new List<IStatisticTask>();

				StatisticTask statisticTask1 = new StatisticTask();
				statisticTask1.Interval = new DateTime(2007,1,5,8,0,0,DateTimeKind.Utc);
				statisticTask1.StatAnsweredTasks = 30;
				statisticTask1.StatAverageQueueTimeSeconds = 40;

				StatisticTask statisticTask2 = new StatisticTask();
				statisticTask2.Interval = new DateTime(2007, 1, 5, 8, 0, 0, DateTimeKind.Utc);
				statisticTask2.StatAnsweredTasks = 40;
				statisticTask2.StatAverageQueueTimeSeconds = 100;

				StatisticTask statisticTask3 = new StatisticTask();
				statisticTask3.Interval = new DateTime(2007, 1, 5, 8, 15, 0, DateTimeKind.Utc);
				statisticTask3.StatAnsweredTasks = 40;
				statisticTask3.StatAverageQueueTimeSeconds = 50;

				statisticTasks.Add(statisticTask1);
				statisticTasks.Add(statisticTask2);
				statisticTasks.Add(statisticTask3);
			
				_target.Match(_workloadDays.OfType<IWorkloadDayBase>(), statisticTasks);

				double expectedSeconds = ((statisticTask1.StatAnsweredTasks * statisticTask1.StatAverageQueueTimeSeconds) +
					 (statisticTask2.StatAnsweredTasks * statisticTask2.StatAverageQueueTimeSeconds)) /
					 (statisticTask1.StatAnsweredTasks + statisticTask2.StatAnsweredTasks);

				Assert.AreEqual((int)expectedSeconds, (int)_workloadDays[0].SortedTaskPeriodList[32].StatisticTask.StatAverageQueueTimeSeconds);
		  }

		  [Test]
		  public void CanMatchStatisticsWithMergedIntervals()
		  {
				IList<IStatisticTask> statisticTasks = new List<IStatisticTask>();

				StatisticTask statisticTask1 = new StatisticTask();
				statisticTask1.Interval = new DateTime(2007, 1, 5, 8, 0, 0, DateTimeKind.Utc);
				statisticTask1.StatAbandonedTasks = 10;
				statisticTask1.StatOfferedTasks = 10;
				statisticTask1.StatAnsweredTasks = 40;
				statisticTask1.StatAverageAfterTaskTimeSeconds = 40;
				statisticTask1.StatAverageTaskTimeSeconds = 20;
				statisticTask1.StatCalculatedTasks = 30;

				StatisticTask statisticTask2 = new StatisticTask();
				statisticTask2.Interval = new DateTime(2007, 1, 5, 8, 15, 0, DateTimeKind.Utc);
				statisticTask2.StatAbandonedTasks = 20;
				statisticTask2.StatOfferedTasks = 20;
				statisticTask2.StatAnsweredTasks = 60;
				statisticTask2.StatAverageAfterTaskTimeSeconds = 50;
				statisticTask2.StatAverageTaskTimeSeconds = 30;
				statisticTask2.StatCalculatedTasks = 40;

				StatisticTask statisticTask3 = new StatisticTask();
				statisticTask3.Interval = new DateTime(2007, 1, 5, 8, 30, 0, DateTimeKind.Utc);
				statisticTask3.StatAbandonedTasks = 10;
				statisticTask3.StatOfferedTasks = 10;
				statisticTask3.StatAnsweredTasks = 50;
				statisticTask3.StatAverageAfterTaskTimeSeconds = 50;
				statisticTask3.StatAverageTaskTimeSeconds = 30;
				statisticTask3.StatCalculatedTasks = 40;

				StatisticTask statisticTask4 = new StatisticTask();
				statisticTask4.Interval = new DateTime(2007, 1, 5, 8, 45, 0, DateTimeKind.Utc);
				statisticTask4.StatAbandonedTasks = 10;
				statisticTask4.StatOfferedTasks = 10;
				statisticTask4.StatAnsweredTasks = 50;
				statisticTask4.StatAverageAfterTaskTimeSeconds = 50;
				statisticTask4.StatAverageTaskTimeSeconds = 30;
				statisticTask4.StatCalculatedTasks = 40;

				statisticTasks.Add(statisticTask1);
				statisticTasks.Add(statisticTask2);
				statisticTasks.Add(statisticTask3);
				statisticTasks.Add(statisticTask4);

				IWorkloadDay workloadDay = _workloadDays[0];
				workloadDay.MergeTemplateTaskPeriods(new List<ITemplateTaskPeriod> {
					 workloadDay.SortedTaskPeriodList[32],
					 workloadDay.SortedTaskPeriodList[33],
					 workloadDay.SortedTaskPeriodList[34]});
				
				_target.Match(_workloadDays.OfType<IWorkloadDayBase>(), statisticTasks);

				workloadDay = _workloadDays[0];
				Assert.AreEqual(40d, workloadDay.SortedTaskPeriodList[32].StatisticTask.StatAbandonedTasks);
				Assert.AreEqual(150d, workloadDay.SortedTaskPeriodList[32].StatisticTask.StatAnsweredTasks);
				Assert.AreEqual(40d, workloadDay.SortedTaskPeriodList[32].StatisticTask.StatCalculatedTasks);
				Assert.AreEqual(40d, workloadDay.SortedTaskPeriodList[32].StatisticTask.StatOfferedTasks);
				Assert.AreEqual(47.33D, Math.Round(workloadDay.SortedTaskPeriodList[32].StatisticTask.StatAverageAfterTaskTimeSeconds, 2));
				Assert.AreEqual(27.33D, Math.Round(workloadDay.SortedTaskPeriodList[32].StatisticTask.StatAverageTaskTimeSeconds, 2));
				Assert.AreEqual(statisticTask4, workloadDay.SortedTaskPeriodList[33].StatisticTask);
		  }

		  [Test]
		  public void VerifyCanGetTemplateTaskPeriodsFromSkillStaffPeriods()
		  {
				DateTimePeriod period1 = new DateTimePeriod(_startDate, _startDate.AddMinutes(15));
				DateTimePeriod period2 = period1.MovePeriod(TimeSpan.FromMinutes(15));

				MockRepository mocks = new MockRepository();
				ISkillStaffPeriod skillStaffPeriod1 = mocks.StrictMock<ISkillStaffPeriod>();
				ISkillStaffPeriod skillStaffPeriod2 = mocks.StrictMock<ISkillStaffPeriod>();

            Expect.Call(skillStaffPeriod1.Period).Return(period1).Repeat.AtLeastOnce();
            Expect.Call(skillStaffPeriod2.Period).Return(period2).Repeat.AtLeastOnce();

				mocks.ReplayAll();

            IList<ITemplateTaskPeriod> taskPeriods = new List<ISkillStaffPeriod>
                                                              {skillStaffPeriod1, skillStaffPeriod2}.CreateTaskPeriodsFromPeriodized();

				mocks.VerifyAll();

				Assert.AreEqual(2,taskPeriods.Count);
				Assert.AreEqual(period1, taskPeriods[0].Period);
				Assert.AreEqual(period2, taskPeriods[1].Period);
		  }

		  [Test]
		  public void VerifyCanSetStatisticTasksOnSkillStaffPeriods()
		  {
				DateTimePeriod period1 = new DateTimePeriod(_startDate, _startDate.AddMinutes(15));
				DateTimePeriod period2 = period1.MovePeriod(TimeSpan.FromMinutes(15));

				ITemplateTaskPeriod taskPeriod1 = new TemplateTaskPeriod(new Task(), period1);
				ITemplateTaskPeriod taskPeriod2 = new TemplateTaskPeriod(new Task(), period2);
				
				taskPeriod1.StatisticTask.StatCalculatedTasks = 7;
				taskPeriod2.StatisticTask.StatCalculatedTasks = 8;

				IActiveAgentCount activeAgentCount1 = new ActiveAgentCount();
				IActiveAgentCount activeAgentCount2 = new ActiveAgentCount();
				activeAgentCount1.Interval = period1.StartDateTime;
				activeAgentCount1.ActiveAgents = 2;
				activeAgentCount2.Interval = period2.StartDateTime;
				activeAgentCount2.ActiveAgents = 3;

				MockRepository mocks = new MockRepository();

				ISkillStaffPeriod skillStaffPeriod1 = mocks.StrictMock<ISkillStaffPeriod>();
				ISkillStaffPeriod skillStaffPeriod2 = mocks.StrictMock<ISkillStaffPeriod>();

				ISkillDay skillDay = mocks.StrictMock<ISkillDay>();
				ISkillDayCalculator skillDayCalculator = mocks.StrictMock<ISkillDayCalculator>();

				Expect.Call(skillStaffPeriod1.Period).Return(period1).Repeat.AtLeastOnce();
				Expect.Call(skillStaffPeriod2.Period).Return(period2).Repeat.AtLeastOnce();

				Expect.Call(skillStaffPeriod1.StatisticTask).Return(new StatisticTask()).PropertyBehavior().Repeat.Once();
				Expect.Call(skillStaffPeriod2.StatisticTask).Return(new StatisticTask()).PropertyBehavior().Repeat.Once();
				Expect.Call(skillStaffPeriod1.ActiveAgentCount).Return(new ActiveAgentCount()).PropertyBehavior().Repeat.Once();
				Expect.Call(skillStaffPeriod2.ActiveAgentCount).Return(new ActiveAgentCount()).PropertyBehavior().Repeat.Once();

				Expect.Call(skillStaffPeriod1.SkillDay).Return(skillDay).Repeat.Once();
				Expect.Call(skillStaffPeriod2.SkillDay).Return(skillDay).Repeat.Once();

				Expect.Call(skillDay.Skill).Return(_skill).Repeat.AtLeastOnce();
				Expect.Call(skillDay.SkillDayCalculator).Return(skillDayCalculator).Repeat.Twice();
				Expect.Call(skillDayCalculator.GetPercentageForInterval(null, new DateTimePeriod())).IgnoreArguments().
					 Return(new Percent(1)).Repeat.Twice();

				mocks.ReplayAll();

				new Statistic(null).Match(new List<ISkillStaffPeriod> {skillStaffPeriod1, skillStaffPeriod2},
									 new List<ITemplateTaskPeriod> {taskPeriod1, taskPeriod2},
									 new List<IActiveAgentCount> {activeAgentCount1, activeAgentCount2});

				Assert.AreEqual(7,skillStaffPeriod1.StatisticTask.StatCalculatedTasks);
				Assert.AreEqual(8,skillStaffPeriod2.StatisticTask.StatCalculatedTasks);
				Assert.AreEqual(2,skillStaffPeriod1.ActiveAgentCount.ActiveAgents);
				Assert.AreEqual(3,skillStaffPeriod2.ActiveAgentCount.ActiveAgents);

				mocks.VerifyAll();
		  }

		  [Test]
		  public void VerifyCanSetStatisticTasksOnSkillStaffPeriodsWithPercentage()
		  {
				DateTimePeriod period1 = new DateTimePeriod(_startDate, _startDate.AddMinutes(15));
				DateTimePeriod period2 = period1.MovePeriod(TimeSpan.FromMinutes(15));

				ITemplateTaskPeriod taskPeriod1 = new TemplateTaskPeriod(new Task(), period1);
				ITemplateTaskPeriod taskPeriod2 = new TemplateTaskPeriod(new Task(), period2);

				taskPeriod1.StatisticTask.StatCalculatedTasks = 7;
				taskPeriod2.StatisticTask.StatCalculatedTasks = 8;

				IActiveAgentCount activeAgentCount1 = new ActiveAgentCount();
				IActiveAgentCount activeAgentCount2 = new ActiveAgentCount();
				activeAgentCount1.Interval = period1.StartDateTime;
				activeAgentCount1.ActiveAgents = 2;
				activeAgentCount2.Interval = period2.StartDateTime;
				activeAgentCount2.ActiveAgents = 3;

				MockRepository mocks = new MockRepository();

				ISkillStaffPeriod skillStaffPeriod1 = mocks.StrictMock<ISkillStaffPeriod>();
				ISkillStaffPeriod skillStaffPeriod2 = mocks.StrictMock<ISkillStaffPeriod>();

				ISkillDay skillDay = mocks.StrictMock<ISkillDay>();
				ISkillDayCalculator skillDayCalculator = mocks.StrictMock<ISkillDayCalculator>();

				Expect.Call(skillStaffPeriod1.Period).Return(period1).Repeat.AtLeastOnce();
				Expect.Call(skillStaffPeriod2.Period).Return(period2).Repeat.AtLeastOnce();

				Expect.Call(skillStaffPeriod1.StatisticTask).Return(new StatisticTask()).PropertyBehavior().Repeat.Once();
				Expect.Call(skillStaffPeriod2.StatisticTask).Return(new StatisticTask()).PropertyBehavior().Repeat.Once();
				Expect.Call(skillStaffPeriod1.ActiveAgentCount).Return(new ActiveAgentCount()).PropertyBehavior().Repeat.Once();
				Expect.Call(skillStaffPeriod2.ActiveAgentCount).Return(new ActiveAgentCount()).PropertyBehavior().Repeat.Once();

				Expect.Call(skillStaffPeriod1.SkillDay).Return(skillDay).Repeat.Once();
				Expect.Call(skillStaffPeriod2.SkillDay).Return(skillDay).Repeat.Once();

				Expect.Call(skillDay.Skill).Return(_skill).Repeat.AtLeastOnce();
				Expect.Call(skillDay.SkillDayCalculator).Return(skillDayCalculator).Repeat.Twice();
				Expect.Call(skillDayCalculator.GetPercentageForInterval(null, new DateTimePeriod())).IgnoreArguments().
					 Return(new Percent(0.5)).Repeat.Twice();

				mocks.ReplayAll();

				new Statistic(null).Match(new List<ISkillStaffPeriod> { skillStaffPeriod1, skillStaffPeriod2 },
									 new List<ITemplateTaskPeriod> { taskPeriod1, taskPeriod2 },
									 new List<IActiveAgentCount> { activeAgentCount1, activeAgentCount2 });

				Assert.AreEqual(3.5, skillStaffPeriod1.StatisticTask.StatCalculatedTasks);
				Assert.AreEqual(4, skillStaffPeriod2.StatisticTask.StatCalculatedTasks);
				Assert.AreEqual(2, skillStaffPeriod1.ActiveAgentCount.ActiveAgents);
				Assert.AreEqual(3, skillStaffPeriod2.ActiveAgentCount.ActiveAgents);

				mocks.VerifyAll();
		  }

		  [Test]
		  public void ShouldHandleZeroTasks()
		  {
				IList<IWorkloadDay> workloadDays1 = WorkloadDayFactory.GetWorkloadDaysForTest(_startDate, _startDate, _workload);

				//Fredag
				workloadDays1[0].TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 0;
				workloadDays1[0].TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 20;
				workloadDays1[0].TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 10;

				Statistic statistic = new Statistic(_workload);
				IWorkload workload = statistic.CalculateTemplateDays(workloadDays1.OfType<IWorkloadDayBase>().ToList());

				IWorkloadDayTemplate template =
					 ((IWorkloadDayTemplate)workload.GetTemplateAt(TemplateTarget.Workload, (int)DayOfWeek.Friday));
				Assert.AreEqual(0d, template.TotalTasks);
		  }

		  [Test]
		  public void CanCalculateTemplateDays()
		  {
				IList<IWorkloadDay> workloadDays1 = WorkloadDayFactory.GetWorkloadDaysForTest(_startDate, _startDate, _workload);
				IList<IWorkloadDay> workloadDays2 = WorkloadDayFactory.GetWorkloadDaysForTest(_startDate.AddDays(7), _startDate.AddDays(7), _workload);

				//Fredag
				workloadDays1[0].TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 20;
				workloadDays1[0].TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 20;
				workloadDays1[0].TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 10;

				//Fredag
				workloadDays2[0].TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 30;
				workloadDays2[0].TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 30;
				workloadDays2[0].TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 20;

				Statistic statistic = new Statistic(_workload);
				IWorkload workload = statistic.CalculateTemplateDays(workloadDays1.Concat(workloadDays2).OfType<IWorkloadDayBase>().ToList());

				IWorkloadDayTemplate template =
					 ((IWorkloadDayTemplate) workload.GetTemplateAt(TemplateTarget.Workload, (int) DayOfWeek.Friday));
				Assert.AreEqual(25d, template.TotalTasks);
				Assert.AreEqual(new TimeSpan(0, 0, 0, 26), template.AverageTaskTime);
				Assert.AreEqual(new TimeSpan(0, 0, 0, 16), template.AverageAfterTaskTime);
		  }

		  [Test]
		  public void CanReloadTemplateDay()
		  {
			  IList<IWorkloadDay> workloadDays1 = WorkloadDayFactory.GetWorkloadDaysForTest(_startDate, _startDate, _workload);
			  IList<IWorkloadDay> workloadDays2 = WorkloadDayFactory.GetWorkloadDaysForTest(_startDate.AddDays(7), _startDate.AddDays(7), _workload);

			  //Fredag
			  workloadDays1[0].TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 20;
			  workloadDays1[0].TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 20;
			  workloadDays1[0].TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 10;

			  //Fredag
			  workloadDays2[0].TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 30;
			  workloadDays2[0].TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 30;
			  workloadDays2[0].TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 20;

			  Statistic statistic = new Statistic(_workload);
			  IWorkload workload = statistic.ReloadCustomTemplateDay(workloadDays1.Concat(workloadDays2).OfType<IWorkloadDayBase>().ToList(), 5);

			  IWorkloadDayTemplate template =
				   ((IWorkloadDayTemplate)workload.GetTemplateAt(TemplateTarget.Workload, (int)DayOfWeek.Friday));
			  Assert.AreEqual(25d, template.TotalTasks);
			  Assert.AreEqual(new TimeSpan(0, 0, 0, 26), template.AverageTaskTime);
			  Assert.AreEqual(new TimeSpan(0, 0, 0, 16), template.AverageAfterTaskTime);
		  }

		  [Test]
		  public void CanCalculateTemplateDaysWithMidnightBreak()
		  {
				_skill.MidnightBreakOffset = TimeSpan.FromHours(7);
				IList<IWorkloadDay> workloadDays1 = WorkloadDayFactory.GetWorkloadDaysForTest(_startDate, _startDate, _workload);
				IList<IWorkloadDay> workloadDays2 = WorkloadDayFactory.GetWorkloadDaysForTest(_startDate.AddDays(7), _startDate.AddDays(7), _workload);

				//Fredag
				workloadDays1[0].TaskPeriodList[95].StatisticTask.StatCalculatedTasks = 20;
				workloadDays1[0].TaskPeriodList[95].StatisticTask.StatAverageTaskTimeSeconds = 20;
				workloadDays1[0].TaskPeriodList[95].StatisticTask.StatAverageAfterTaskTimeSeconds = 10;

				//Fredag
				workloadDays2[0].TaskPeriodList[95].StatisticTask.StatCalculatedTasks = 30;
				workloadDays2[0].TaskPeriodList[95].StatisticTask.StatAverageTaskTimeSeconds = 30;
				workloadDays2[0].TaskPeriodList[95].StatisticTask.StatAverageAfterTaskTimeSeconds = 20;

				Statistic statistic = new Statistic(_workload);
				IWorkload workload = statistic.CalculateTemplateDays(workloadDays1.Concat(workloadDays2).OfType<IWorkloadDayBase>().ToList());

				IWorkloadDayTemplate template =
					 ((IWorkloadDayTemplate)workload.GetTemplateAt(TemplateTarget.Workload, (int)DayOfWeek.Friday));
				Assert.AreEqual(25d, template.TotalTasks);
				Assert.AreEqual(new TimeSpan(0, 0, 0, 26), template.AverageTaskTime);
				Assert.AreEqual(new TimeSpan(0, 0, 0, 16), template.AverageAfterTaskTime);
				Assert.AreEqual(25d,template.TaskPeriodList[95].TotalTasks);
				Assert.AreEqual(0d, template.TaskPeriodList[0].TotalTasks);
		  }

		  [Test]
		  public void CanCalculateTemplateDaysForEmailSkill()
		  {
				ISkillType skillType = new SkillTypeEmail(new Description("Test"), ForecastSource.Email);
				ISkill skill = SkillFactory.CreateSkill("Skill", skillType, 60);
				_workload = WorkloadFactory.CreateWorkload(skill);

				IList<IWorkloadDay> workloadDays1 = WorkloadDayFactory.GetWorkloadDaysForTest(_startDate, _startDate, _workload);
				IList<IWorkloadDay> workloadDays2 = WorkloadDayFactory.GetWorkloadDaysForTest(_startDate.AddDays(7), _startDate.AddDays(7), _workload);

				//Fredag
				workloadDays1[0].TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 20;
				workloadDays1[0].TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 20;
				workloadDays1[0].TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 10;

				//Fredag
				workloadDays2[0].TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 30;
				workloadDays2[0].TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 30;
				workloadDays2[0].TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 20;

				Statistic statistic = new Statistic(_workload);
				IWorkload workload = statistic.CalculateTemplateDays(workloadDays1.Concat(workloadDays2).OfType<IWorkloadDayBase>().ToList());

				IWorkloadDayTemplate template =
					 ((IWorkloadDayTemplate)workload.GetTemplateAt(TemplateTarget.Workload, (int)DayOfWeek.Friday));
				Assert.AreEqual(25d, template.TotalTasks);
				Assert.AreEqual(template.TaskPeriodList.First().AverageTaskTime, template.AverageTaskTime);
				Assert.AreEqual(template.TaskPeriodList.First().AverageAfterTaskTime, template.AverageAfterTaskTime);
				Assert.AreEqual(template.TaskPeriodList.Last().AverageTaskTime, template.AverageTaskTime);
				Assert.AreEqual(template.TaskPeriodList.Last().AverageAfterTaskTime, template.AverageAfterTaskTime);
		  }

		[Test]
		public void ShouldMergeStatisticTasks()
		{
			IList<IStatisticTask> tasks = new List<IStatisticTask>();
			tasks.Add(new StatisticTask
			{
				 StatAbandonedTasks = 1,
				StatAbandonedTasksWithinSL= 2,
				StatAbandonedShortTasks = 3,
				StatAnsweredTasks = 4,
				StatAnsweredTasksWithinSL = 5,
				StatCalculatedTasks = 6,
				StatOfferedTasks = 7,
				StatAverageAfterTaskTimeSeconds = 1,
				StatAverageTaskTimeSeconds = 2,
				StatAverageQueueTimeSeconds = 3,
				StatOverflowInTasks = 11,
				StatOverflowOutTasks = 12
			});
			tasks.Add(new StatisticTask
			{
				StatAbandonedTasks = 2,
				StatAbandonedTasksWithinSL = 4,
				StatAbandonedShortTasks = 6,
				StatAnsweredTasks = 8,
				StatAnsweredTasksWithinSL = 10,
				StatCalculatedTasks = 12,
				StatOfferedTasks = 14,
				StatAverageAfterTaskTimeSeconds = 2,
				StatAverageTaskTimeSeconds = 4,
				StatAverageQueueTimeSeconds = 8,
				StatOverflowInTasks = 22,
				StatOverflowOutTasks = 24
			});
			tasks.Add(new StatisticTask
			{
				StatAbandonedTasks = 3,
				StatAbandonedTasksWithinSL = 8,
				StatAbandonedShortTasks = 9,
				StatAnsweredTasks = 12,
				StatAnsweredTasksWithinSL = 15,
				StatCalculatedTasks = 18,
				StatOfferedTasks = 21,
				StatAverageAfterTaskTimeSeconds = 4,
				StatAverageTaskTimeSeconds = 8,
				StatAverageQueueTimeSeconds = 12,
				StatOverflowInTasks = 33,
				StatOverflowOutTasks = 36
			});
			var stat = tasks.MergeStatisticTasks();

			Assert.AreEqual(6, stat.StatAbandonedTasks);
			Assert.AreEqual(14, stat.StatAbandonedTasksWithinSL);
			Assert.AreEqual(18, stat.StatAbandonedShortTasks);
			Assert.AreEqual(24, stat.StatAnsweredTasks);
			Assert.AreEqual(30, stat.StatAnsweredTasksWithinSL);
			Assert.AreEqual(36, stat.StatCalculatedTasks);
			Assert.AreEqual(42, stat.StatOfferedTasks);
			Assert.AreEqual((double)(1 * 4 + 2 * 8 + 4 * 12) / (4 + 8 + 12), stat.StatAverageAfterTaskTimeSeconds);
			Assert.AreEqual((double)(2 * 4 + 4 * 8 + 8 * 12) / (4 + 8 + 12), stat.StatAverageTaskTimeSeconds);
			Assert.AreEqual(Math.Round((double)(3 * 4 + 8 * 8 + 12 * 12) / (4 + 8 + 12),2), Math.Round(stat.StatAverageQueueTimeSeconds, 2));
			Assert.AreEqual(66, stat.StatOverflowInTasks);
			Assert.AreEqual(72, stat.StatOverflowOutTasks);
		}

		[Test]
		public void ShouldMergeHandlingTime()
		{
			IList<IStatisticTask> tasks = new List<IStatisticTask>();
			tasks.Add(new StatisticTask
			{
				StatAnsweredTasks = 4,
				StatAverageHandleTimeSeconds = 3
			});
			tasks.Add(new StatisticTask
			{
				StatAnsweredTasks = 8,
				StatAverageHandleTimeSeconds = 6
			});
			tasks.Add(new StatisticTask
			{
				StatAnsweredTasks = 12,
				StatAverageHandleTimeSeconds = 12
			});
			var stat = tasks.MergeStatisticTasks();
			Assert.That(stat.StatAverageHandleTimeSeconds, Is.EqualTo((double)(3 * 4 + 6 * 8 + 12 * 12) / (4 + 8 + 12)));
		}

		 [Test]
		 public void ShouldNotRoundWhenMerging()
		 {
			 var tasks = new List<IStatisticTask>
			 {
				 new StatisticTask
				 {
					 StatAnsweredTasks = 1,
					 StatAverageAfterTaskTimeSeconds = 1,
					 StatAverageTaskTimeSeconds = 1,
					 StatAverageHandleTimeSeconds = 1,
					 StatAverageQueueTimeSeconds = 1
				 },
				 new StatisticTask
				 {
					 StatAnsweredTasks = 1,
					 StatAverageAfterTaskTimeSeconds = 2,
					 StatAverageTaskTimeSeconds = 2,
					 StatAverageHandleTimeSeconds = 2,
					 StatAverageQueueTimeSeconds = 2 
				 }
			 };

			 var stat = tasks.MergeStatisticTasks();

			 stat.StatAverageAfterTaskTimeSeconds.Should().Be.EqualTo(1.5D);
			 stat.StatAverageTaskTimeSeconds.Should().Be.EqualTo(1.5D);
			 stat.StatAverageQueueTimeSeconds.Should().Be.EqualTo(1.5D);
			 stat.StatAverageHandleTimeSeconds.Should().Be.EqualTo(1.5D);
		 }
	 }
}
