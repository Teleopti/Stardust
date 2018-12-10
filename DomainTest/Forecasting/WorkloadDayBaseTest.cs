using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
	[TestFixture]
	public class WorkloadDayBaseTest
	{
		private WorkloadDayBase _workloadDayBase;
		private IWorkload _workload;
		private ISkill _skill;
		private IList<TimePeriod> _openHours;

		[SetUp]
		public void Setup()
		{
			_skill = SkillFactory.CreateSkill("testSkill", SkillTypeFactory.CreateSkillType(), 15);
			_skill.MidnightBreakOffset = TimeSpan.FromHours(2);
			_workload = new Workload(_skill);

			_openHours = new List<TimePeriod>();
			_workloadDayBase = new TestWorkloadDayBase();

			_openHours.Add(new TimePeriod(new TimeSpan(9, 0, 0), new TimeSpan(1, 2, 0, 0)));

			_workloadDayBase.Create(SkillDayTemplate.BaseDate, _workload, _openHours);

			_workloadDayBase.Tasks = 999;
		}

		[Test]
		public void VerifyConstructor()
		{
			Assert.IsNotNull(_workloadDayBase);
		}

		[Test]
		public void VerifyEmptyConstructor()
		{
			Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_workloadDayBase.GetType(), true));
		}

		[Test]
		public void VerifyProperties()
		{
			_workloadDayBase.Tasks = 112;
			Assert.AreEqual(112d, Math.Round(_workloadDayBase.Tasks, 2));
			Assert.AreEqual(_workload, _workloadDayBase.Workload);
			Assert.AreEqual(0, _workloadDayBase.Parents.Count);
		}

		[Test]
		public void VerifyTotalTasksCannotBeSetIfClosed()
		{
			_workloadDayBase.Close();
			Assert.Throws<InvalidOperationException>(() => _workloadDayBase.Tasks = 112);
		}

		[Test]
		public void VerifyAddingTaskPeriodAddsTasksToDailyTotal()
		{
			_workloadDayBase.Close();
			_workloadDayBase.MakeOpen24Hours();

			double tasks1 = 123;
			double tasks2 = 321;

			_workloadDayBase.SortedTaskPeriodList[0].SetTasks(tasks1);

			Assert.AreEqual(tasks1, _workloadDayBase.Tasks);

			_workloadDayBase.SortedTaskPeriodList[1].SetTasks(tasks2);

			Assert.AreEqual(tasks1 + tasks2, _workloadDayBase.Tasks);
		}

		[Test]
		public void VerifyChangingTotalTasksWithoutTemplateTaskPeriods()
		{
			_workloadDayBase.Close();
			_workloadDayBase.MakeOpen24Hours();

			Assert.AreEqual(0, _workloadDayBase.Tasks);

			_workloadDayBase.Tasks = 234;

			Assert.AreEqual(234, _workloadDayBase.Tasks);
		}

		[Test]
		public void VerifyRecalculateStatisticTasks()
		{
			ITask t1 = new Task();
			ITemplateTaskPeriod p1 = new TemplateTaskPeriod(
					t1,
					new DateTimePeriod(
							DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(new TimeSpan(7, 0, 0)), DateTimeKind.Utc),
							DateTime.SpecifyKind(SkillDayTemplate.BaseDate.Date.Add(new TimeSpan(11, 0, 0)), DateTimeKind.Utc)));
			p1.AverageTaskTime = new TimeSpan(0, 0, 120);

			var taskPeriods = new List<ITemplateTaskPeriod> {p1};

			double totalstatisticCalculatedTasks = 0;
			double totalStatisticAnsweredTasks = 0;
			double totalStatisticAbandonedTasks = 0;
			foreach (ITemplateTaskPeriod taskPeriod in taskPeriods)
			{
				totalstatisticCalculatedTasks += taskPeriod.StatisticTask.StatCalculatedTasks;
				totalStatisticAnsweredTasks += taskPeriod.StatisticTask.StatAnsweredTasks;
				totalStatisticAbandonedTasks += taskPeriod.StatisticTask.StatAbandonedTasks;
			}
			_workloadDayBase.RecalculateDailyStatisticTasks();
			Assert.AreEqual(totalstatisticCalculatedTasks, _workloadDayBase.TotalStatisticCalculatedTasks);
			Assert.AreEqual(totalStatisticAnsweredTasks, _workloadDayBase.TotalStatisticAnsweredTasks);
			Assert.AreEqual(totalStatisticAbandonedTasks, _workloadDayBase.TotalStatisticAbandonedTasks);
		}

		[Test]
		public void VerifySetOpenHourList()
		{
			Assert.AreEqual(1, _workloadDayBase.OpenHourList.Count);
			Assert.IsInstanceOf<ReadOnlyCollection<TimePeriod>>(_workloadDayBase.OpenHourList);
		}

		[Test]
		public void VerifySetOpenHourListOutsideMidnightBreak()
		{
			_skill.MidnightBreakOffset = TimeSpan.FromHours(8);
			Assert.Throws<ArgumentOutOfRangeException>(() => _workloadDayBase.ChangeOpenHours(new List<TimePeriod> { new TimePeriod(2, 0, 8, 0) }));
		}

		[Test]
		public void VerifyEmptyOpenHourListClosesDayCorrectly()
		{
			_workloadDayBase.MakeOpen24Hours();
			IList<TimePeriod> list = new List<TimePeriod>();
			_workloadDayBase.ChangeOpenHours(list);
			Assert.IsFalse(_workloadDayBase.OpenForWork.IsOpen);
			Assert.AreEqual(0, _workloadDayBase.TotalTasks);
			Assert.AreEqual(0, _workloadDayBase.TaskPeriodList.Count);
		}

		[Test]
		public void VerifyMakeOpen24HoursWorks()
		{
			_workloadDayBase.Close();
			_workloadDayBase.MakeOpen24Hours();
			Assert.AreEqual(1, _workloadDayBase.OpenHourList.Count);
			Assert.AreEqual(new TimePeriod(TimeSpan.FromHours(2), TimeSpan.FromHours(26)), _workloadDayBase.OpenHourList[0]);
		}

		/// <summary>
		/// Verifies the task period list count after setting new open hour list.
		/// Test for PBI 
		/// </summary>
		/// <remarks>
		/// Created by: henryg
		/// Created date: 2009-11-11
		/// </remarks>
		[Test]
		public void VerifyTaskPeriodListCountAfterSettingNewOpenHourList()
		{
			_workloadDayBase.SetTaskPeriodCollection(new List<ITemplateTaskPeriod>());
			IList<TimePeriod> timePeriods = new List<TimePeriod>();
			timePeriods.Add(new TimePeriod(new TimeSpan(4, 0, 0), new TimeSpan(6, 0, 0)));
			_workloadDayBase.ChangeOpenHours(timePeriods);
			// (18 + 1) * 4 = 76
			Assert.AreEqual(8, _workloadDayBase.TaskPeriodList.Count);
		}

		[Test]
		public void VerifyTaskPeriodListCountAfterSettingNewOpenHourList2()
		{
			_workloadDayBase.SetTaskPeriodCollection(new List<ITemplateTaskPeriod>());
			IList<TimePeriod> timePeriods = new List<TimePeriod>();
			timePeriods.Add(new TimePeriod(new TimeSpan(4, 0, 0), new TimeSpan(7, 0, 0)));

			_workloadDayBase.ChangeOpenHours(timePeriods);
			// (18 + 1) * 4 = 76
			Assert.AreEqual(_workloadDayBase.SortedTaskPeriodList[0].Period.StartDateTime, new DateTime(1800, 1, 1, 4, 0, 0));
			Assert.AreEqual(_workloadDayBase.SortedTaskPeriodList[1].Period.StartDateTime, new DateTime(1800, 1, 1, 4, 15, 0));
			Assert.AreEqual(_workloadDayBase.SortedTaskPeriodList[2].Period.StartDateTime, new DateTime(1800, 1, 1, 4, 30, 0));
			Assert.AreEqual(_workloadDayBase.SortedTaskPeriodList[3].Period.StartDateTime, new DateTime(1800, 1, 1, 4, 45, 0));

			Assert.AreEqual(_workloadDayBase.SortedTaskPeriodList[4].Period.StartDateTime, new DateTime(1800, 1, 1, 5, 0, 0));
			Assert.AreEqual(_workloadDayBase.SortedTaskPeriodList[5].Period.StartDateTime, new DateTime(1800, 1, 1, 5, 15, 0));
			Assert.AreEqual(_workloadDayBase.SortedTaskPeriodList[6].Period.StartDateTime, new DateTime(1800, 1, 1, 5, 30, 0));
			Assert.AreEqual(_workloadDayBase.SortedTaskPeriodList[7].Period.StartDateTime, new DateTime(1800, 1, 1, 5, 45, 0));
		}

		[Test]
		public void VerifyTaskPeriodListCountAfterSettingNewOpenHourList3()
		{
			ITemplateTaskPeriod templateTaskPeriod = _workloadDayBase.SortedTaskPeriodList[0];
			Assert.IsTrue(templateTaskPeriod.TotalTasks > 0);

			_workloadDayBase.SetTaskPeriodCollection(new List<ITemplateTaskPeriod>());
			IList<TimePeriod> timePeriods = new List<TimePeriod>();
			timePeriods.Add(new TimePeriod(new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)));
			_workloadDayBase.ChangeOpenHours(timePeriods);
			// (18 + 1) * 4 = 76
			Assert.AreEqual(new DateTime(1800, 1, 1, 8, 0, 0), _workloadDayBase.SortedTaskPeriodList[0].Period.StartDateTime);
			Assert.AreEqual(new DateTime(1800, 1, 1, 8, 15, 0), _workloadDayBase.SortedTaskPeriodList[1].Period.StartDateTime);
			Assert.AreEqual(new DateTime(1800, 1, 1, 8, 30, 0), _workloadDayBase.SortedTaskPeriodList[2].Period.StartDateTime);
			Assert.AreEqual(new DateTime(1800, 1, 1, 8, 45, 0), _workloadDayBase.SortedTaskPeriodList[3].Period.StartDateTime);

			Assert.AreEqual(new DateTime(1800, 1, 1, 9, 0, 0), _workloadDayBase.SortedTaskPeriodList[4].Period.StartDateTime);
			Assert.AreEqual(new DateTime(1800, 1, 1, 9, 15, 0), _workloadDayBase.SortedTaskPeriodList[5].Period.StartDateTime);
			Assert.AreEqual(new DateTime(1800, 1, 1, 9, 30, 0), _workloadDayBase.SortedTaskPeriodList[6].Period.StartDateTime);
			Assert.AreEqual(new DateTime(1800, 1, 1, 9, 45, 0), _workloadDayBase.SortedTaskPeriodList[7].Period.StartDateTime);

			Assert.AreEqual(8, _workloadDayBase.TaskPeriodList.Count);
			templateTaskPeriod = _workloadDayBase.SortedTaskPeriodList[4];
			Assert.IsTrue(templateTaskPeriod.TotalTasks > 0);
		}

		[Test]
		public void VerifyTaskPeriodListCountAfterSettingNewOpenHourList4()
		{
			_skill.MidnightBreakOffset = TimeSpan.FromHours(3);
			_workloadDayBase.Tasks = 100;
			ITemplateTaskPeriod templateTaskPeriod = _workloadDayBase.SortedTaskPeriodList[0];
			Assert.IsTrue(templateTaskPeriod.TotalTasks > 0);

			IList<TimePeriod> timePeriods = new List<TimePeriod>();
			timePeriods.Add(new TimePeriod(new TimeSpan(6, 30, 0), new TimeSpan(1, 0, 0, 0)));
			_workloadDayBase.ChangeOpenHours(timePeriods);

			//10+60
			Assert.AreEqual(70, _workloadDayBase.TaskPeriodList.Count);
			Assert.IsTrue(_workloadDayBase.TotalTasks > 0);
		}

		[Test]
		public void VerifyCloseWorks()
		{
			_workloadDayBase.Close();
			_workloadDayBase.MakeOpen24Hours();

			Task t1 = new Task(112, new TimeSpan(0, 0, 0, 120, 500), new TimeSpan((1)));
			_workloadDayBase.SortedTaskPeriodList[0].SetTasks(t1.Tasks);
			_workloadDayBase.SortedTaskPeriodList[0].AverageTaskTime = t1.AverageTaskTime;
			_workloadDayBase.SortedTaskPeriodList[0].AverageAfterTaskTime = t1.AverageAfterTaskTime;
			_workloadDayBase.Close();

			Assert.AreEqual(0, _workloadDayBase.TotalTasks);
			Assert.IsFalse(_workloadDayBase.OpenForWork.IsOpen);
			Assert.AreEqual(0, _workloadDayBase.TaskPeriodList.Count);
		}

		[Test]
		public void VerifyCloseWorksWhenSupplyingEmptyOpenHoursTwice()
		{
			_workloadDayBase.MakeOpen24Hours();
			_workloadDayBase.TaskPeriodList[0].SetTasks(10d);
			TaskOwnerPeriod period = new TaskOwnerPeriod(_workloadDayBase.CurrentDate,
				new List<ITaskOwner> {_workloadDayBase},
				TaskOwnerPeriodType.Other);
			Assert.AreNotEqual(0, period.TotalTasks);
			_workloadDayBase.ChangeOpenHours(new List<TimePeriod> { new TimePeriod(TimeSpan.FromHours(2), TimeSpan.FromHours(2)), new TimePeriod(TimeSpan.FromHours(2), TimeSpan.FromHours(2)) });
			_workloadDayBase.ChangeOpenHours(new List<TimePeriod> { new TimePeriod(TimeSpan.FromHours(2), TimeSpan.FromHours(2)) });
			Assert.AreEqual(0, period.TotalTasks);
			Assert.IsFalse(_workloadDayBase.OpenForWork.IsOpen);
		}

		[Test]
		public void VerifyOpenDuringDstSwitch()
		{
			_skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			_skill.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			_workloadDayBase.Create(new DateOnly(2009, 3, 29), _workload, new List<TimePeriod>());
			_workloadDayBase.MakeOpen24Hours();

			Assert.AreEqual(92, _workloadDayBase.TaskPeriodList.Count);

			_workloadDayBase.Create(new DateOnly(2008, 10, 26), _workload, new List<TimePeriod>());
			_workloadDayBase.MakeOpen24Hours();

			Assert.AreEqual(100, _workloadDayBase.TaskPeriodList.Count);
		}

		[Test]
		public void VerifyAddTaskPeriod()
		{
			Assert.AreEqual(68, _workloadDayBase.TaskPeriodList.Count);
		}

		[Test]
		public void VerifyCorrectAverageTimesWhenTasksZero()
		{

			_workloadDayBase = new TestWorkloadDayBase();
			_workloadDayBase.Create(SkillDayTemplate.BaseDate, _workload, _openHours);

			foreach (ITemplateTaskPeriod taskPeriod in _workloadDayBase.SortedTaskPeriodList)
			{
				foreach (TimePeriod openHour in _openHours)
				{
					if (taskPeriod.Period.TimePeriod(_workloadDayBase.Workload.Skill.TimeZone).StartTime >= openHour.StartTime &&
							taskPeriod.Period.TimePeriod(_workloadDayBase.Workload.Skill.TimeZone).EndTime <= openHour.EndTime)
					{
						taskPeriod.SetTasks(100d / 12d);
						taskPeriod.AverageTaskTime = TimeSpan.FromSeconds(120);
						taskPeriod.AverageAfterTaskTime = TimeSpan.FromSeconds(20);
					}

				}
			}

			Assert.AreEqual(TimeSpan.FromSeconds(120), _workloadDayBase.AverageTaskTime);
		}

		[Test]
		public void VerifyAddingTaskPeriodAffectsAverageTaskTime()
		{
			_workloadDayBase.Close();
			_workloadDayBase.MakeOpen24Hours();

			Task t1 = new Task(123, new TimeSpan(0, 0, 0, 120, 500), new TimeSpan((1)));
			_workloadDayBase.SortedTaskPeriodList[0].SetTasks(t1.Tasks);
			_workloadDayBase.SortedTaskPeriodList[0].AverageTaskTime = t1.AverageTaskTime;
			_workloadDayBase.SortedTaskPeriodList[0].AverageAfterTaskTime = t1.AverageAfterTaskTime;
			Assert.AreEqual(t1.AverageTaskTime, _workloadDayBase.AverageTaskTime);

			Task t2 = new Task(321, new TimeSpan(0, 0, 0, 240, 500), new TimeSpan(1));

			_workloadDayBase.SortedTaskPeriodList[1].SetTasks(t2.Tasks);
			_workloadDayBase.SortedTaskPeriodList[1].AverageTaskTime = t2.AverageTaskTime;
			_workloadDayBase.SortedTaskPeriodList[1].AverageAfterTaskTime = t2.AverageAfterTaskTime;
			long p1TotalTicks = (long)t1.Tasks * t1.AverageTaskTime.Ticks;
			long p2TotalTicks = (long)t2.Tasks * t2.AverageTaskTime.Ticks;
			Assert.AreEqual(new TimeSpan((long)((p1TotalTicks + p2TotalTicks) / _workloadDayBase.TotalTasks)), _workloadDayBase.AverageTaskTime);
		}

		[Test]
		public void VerifyAddingTaskPeriodAffectsAverageStatisticTaskTime()
		{
			_workloadDayBase.Close();
			_workloadDayBase.MakeOpen24Hours();

			_workloadDayBase.SortedTaskPeriodList[0].StatisticTask.StatAnsweredTasks = 200;
			_workloadDayBase.SortedTaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 60;
			_workloadDayBase.SortedTaskPeriodList[1].StatisticTask.StatAnsweredTasks = 100;
			_workloadDayBase.SortedTaskPeriodList[1].StatisticTask.StatAverageTaskTimeSeconds = 120;
			double expectedTalkTime = ((200 * 60) + (100 * 120)) / (200 + 100);
			_workloadDayBase.RecalculateDailyAverageStatisticTimes();
			Assert.AreEqual(TimeSpan.FromSeconds(expectedTalkTime), _workloadDayBase.TotalStatisticAverageTaskTime);
			_workloadDayBase.SortedTaskPeriodList[0].StatisticTask.StatAnsweredTasks = 0;
			_workloadDayBase.SortedTaskPeriodList[1].StatisticTask.StatAnsweredTasks = 0;

			_workloadDayBase.RecalculateDailyAverageStatisticTimes();
			expectedTalkTime = (60d + 120d) / 96d;
			Assert.AreEqual(TimeSpan.FromSeconds(expectedTalkTime), _workloadDayBase.TotalStatisticAverageTaskTime);

			_workloadDayBase.Close();
			_workloadDayBase.RecalculateDailyAverageStatisticTimes();
			Assert.AreEqual(TimeSpan.Zero, _workloadDayBase.TotalStatisticAverageTaskTime);
		}

		[Test]
		public void VerifyAddingTaskPeriodAffectsAverageAfterStatisticTaskTime()
		{
			_workloadDayBase.Close();
			_workloadDayBase.MakeOpen24Hours();

			_workloadDayBase.SortedTaskPeriodList[0].StatisticTask.StatAnsweredTasks = 200;
			_workloadDayBase.SortedTaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 60;
			_workloadDayBase.SortedTaskPeriodList[1].StatisticTask.StatAnsweredTasks = 100;
			_workloadDayBase.SortedTaskPeriodList[1].StatisticTask.StatAverageAfterTaskTimeSeconds = 120;
			double expectedAfterTalkTime = ((200 * 60) + (100 * 120)) / (200 + 100);
			_workloadDayBase.RecalculateDailyAverageStatisticTimes();
			Assert.AreEqual(TimeSpan.FromSeconds(expectedAfterTalkTime), _workloadDayBase.TotalStatisticAverageAfterTaskTime);
			_workloadDayBase.SortedTaskPeriodList[0].StatisticTask.StatAnsweredTasks = 0;
			_workloadDayBase.SortedTaskPeriodList[1].StatisticTask.StatAnsweredTasks = 0;

			_workloadDayBase.RecalculateDailyAverageStatisticTimes();
			expectedAfterTalkTime = (60d + 120d) / 96d;
			Assert.AreEqual(TimeSpan.FromSeconds(expectedAfterTalkTime), _workloadDayBase.TotalStatisticAverageAfterTaskTime);

			_workloadDayBase.Close();
			_workloadDayBase.RecalculateDailyAverageStatisticTimes();
			Assert.AreEqual(TimeSpan.Zero, _workloadDayBase.TotalStatisticAverageAfterTaskTime);
		}

		[Test]
		public void VerifyChangingAverageTaskTimeDistributesToPeriods()
		{
			_workloadDayBase.Close();
			_workloadDayBase.MakeOpen24Hours();

			Task t1 = new Task(100, new TimeSpan(0, 0, 0, 120, 0), new TimeSpan((1)));
			_workloadDayBase.SortedTaskPeriodList[0].SetTasks(t1.Tasks);
			_workloadDayBase.SortedTaskPeriodList[0].AverageTaskTime = t1.AverageTaskTime;
			_workloadDayBase.SortedTaskPeriodList[0].AverageAfterTaskTime = t1.AverageAfterTaskTime;

			Task t2 = new Task(200, new TimeSpan(0, 0, 0, 240, 0), new TimeSpan((1)));
			_workloadDayBase.SortedTaskPeriodList[1].SetTasks(t2.Tasks);
			_workloadDayBase.SortedTaskPeriodList[1].AverageTaskTime = t2.AverageTaskTime;
			_workloadDayBase.SortedTaskPeriodList[1].AverageAfterTaskTime = t2.AverageAfterTaskTime;

			Assert.AreEqual(new TimeSpan(0, 0, 0, 200, 0), _workloadDayBase.AverageTaskTime);

			_workloadDayBase.AverageTaskTime = new TimeSpan(0, 0, 0, 400, 0);
			Assert.AreEqual(new TimeSpan(0, 0, 0, 240, 0), _workloadDayBase.TaskPeriodList[0].Task.AverageTaskTime);
		}

		[Test]
		public void VerifyAddingTaskPeriodAffectsAverageAfterTaskTime()
		{
			_workloadDayBase.Close();
			_workloadDayBase.MakeOpen24Hours();

			double tasks1 = 123;
			double tasks2 = 321;

			_workloadDayBase.SortedTaskPeriodList[0].SetTasks(tasks1);
			_workloadDayBase.SortedTaskPeriodList[0].AverageAfterTaskTime = new TimeSpan(0, 0, 0, 120, 500);

			_workloadDayBase.SortedTaskPeriodList[1].SetTasks(tasks2);
			_workloadDayBase.SortedTaskPeriodList[1].AverageAfterTaskTime = new TimeSpan(0, 0, 0, 240, 500);

			Assert.AreEqual(new TimeSpan(0, 0, 0, 120, 500), _workloadDayBase.SortedTaskPeriodList[0].AverageAfterTaskTime);
			long p1TotalTicks = (long)tasks1 * _workloadDayBase.SortedTaskPeriodList[0].AverageAfterTaskTime.Ticks;
			long p2TotalTicks = (long)tasks2 * _workloadDayBase.SortedTaskPeriodList[1].AverageAfterTaskTime.Ticks;
			Assert.AreEqual(new TimeSpan((long)((p1TotalTicks + p2TotalTicks) / _workloadDayBase.TotalTasks)), _workloadDayBase.AverageAfterTaskTime);
		}

		[Test]
		public void VerifyChangingAverageAfterTaskTimeDistributesToPeriods()
		{
			_workloadDayBase.Close();
			_workloadDayBase.MakeOpen24Hours();

			double tasks1 = 100;
			double tasks2 = 200;

			_workloadDayBase.SortedTaskPeriodList[0].SetTasks(tasks1);
			_workloadDayBase.SortedTaskPeriodList[0].AverageAfterTaskTime = TimeSpan.FromSeconds(120);

			_workloadDayBase.SortedTaskPeriodList[1].SetTasks(tasks2);
			_workloadDayBase.SortedTaskPeriodList[1].AverageAfterTaskTime = TimeSpan.FromSeconds(240);

			Assert.AreEqual(TimeSpan.FromSeconds(200), _workloadDayBase.AverageAfterTaskTime);

			_workloadDayBase.AverageAfterTaskTime = TimeSpan.FromSeconds(400);
			Assert.AreEqual(TimeSpan.FromSeconds(240), _workloadDayBase.SortedTaskPeriodList[0].AverageAfterTaskTime);
		}

		[Test]
		public void VerifyRemovingTaskPeriodAffectsAverageHandlingTimeAndAfterTaskTime()
		{
			_workloadDayBase.Close();
			_workloadDayBase.MakeOpen24Hours();

			Task t1 = new Task(123, new TimeSpan(0, 0, 0, 120, 500), new TimeSpan(0, 0, 0, 120, 500));
			_workloadDayBase.SortedTaskPeriodList[0].SetTasks(t1.Tasks);
			_workloadDayBase.SortedTaskPeriodList[0].AverageTaskTime = t1.AverageTaskTime;
			_workloadDayBase.SortedTaskPeriodList[0].AverageAfterTaskTime = t1.AverageAfterTaskTime;

			Assert.AreEqual(_workloadDayBase.SortedTaskPeriodList[0].AverageTaskTime, _workloadDayBase.AverageTaskTime);
			Assert.AreEqual(_workloadDayBase.SortedTaskPeriodList[0].AverageAfterTaskTime, _workloadDayBase.AverageAfterTaskTime);

			Task t2 = new Task(321, new TimeSpan(0, 0, 0, 240, 500), new TimeSpan(0, 0, 0, 240, 500));

			_workloadDayBase.SortedTaskPeriodList[1].SetTasks(t2.Tasks);
			_workloadDayBase.SortedTaskPeriodList[1].AverageTaskTime = t2.AverageTaskTime;
			_workloadDayBase.SortedTaskPeriodList[1].AverageAfterTaskTime = t2.AverageAfterTaskTime;

			Assert.AreEqual(new TimeSpan((long)(((t1.Tasks * t1.AverageTaskTime.Ticks) + (t2.Tasks * t2.AverageTaskTime.Ticks)) / _workloadDayBase.TotalTasks)), _workloadDayBase.AverageTaskTime);
			Assert.AreEqual(new TimeSpan((long)(((t1.Tasks * t1.AverageAfterTaskTime.Ticks) + (t2.Tasks * t2.AverageAfterTaskTime.Ticks)) / _workloadDayBase.TotalTasks)), _workloadDayBase.AverageAfterTaskTime);

			_workloadDayBase.SortedTaskPeriodList[1].SetTasks(0);
			_workloadDayBase.SortedTaskPeriodList[1].AverageTaskTime = new TimeSpan(1);
			_workloadDayBase.SortedTaskPeriodList[1].AverageAfterTaskTime = new TimeSpan(1);

			Assert.AreEqual(t1.AverageTaskTime, _workloadDayBase.AverageTaskTime);
			Assert.AreEqual(t1.AverageAfterTaskTime, _workloadDayBase.AverageAfterTaskTime);
		}

		[Test]
		public void VerifyNullAsWorkloadGivesException()
		{
			_workloadDayBase = new TestWorkloadDayBase();
			Assert.Throws<ArgumentNullException>(() => _workloadDayBase.Create(_workloadDayBase.CurrentDate, null, _openHours));
		}

		[Test]
		public void VerifyNullAsScenarioGivesException()
		{
			_workloadDayBase = new TestWorkloadDayBase();
			Assert.Throws<ArgumentNullException>(() => _workloadDayBase.Create(_workloadDayBase.CurrentDate, null, _openHours));
		}

		[Test]
		public void VerifyCannotSetAverageTaskTimeIfClosed()
		{
			_workloadDayBase.Close();
			Assert.Throws<InvalidOperationException>(() => _workloadDayBase.AverageTaskTime = TimeSpan.FromSeconds(5));
		}

		[Test]
		public void VerifyCannotSetAverageAfterTaskTimeIfClosed()
		{
			_workloadDayBase.Close();
			Assert.Throws<InvalidOperationException>(() => _workloadDayBase.AverageAfterTaskTime = TimeSpan.FromSeconds(5));
		}

		[Test]
		public void VerifyLockAndRelease()
		{
			_workloadDayBase.Lock();
			_workloadDayBase.Release();
		}

		[Test]
		public void VerifySetDirty()
		{
			_workloadDayBase.SetDirty();
		}

		[Test]
		public void VerifyChangeValuesWithParentInLockedMode()
		{
			MockRepository mocks = new MockRepository();
			ITaskOwner taskOwner = mocks.StrictMock<ITaskOwner>();

			using (mocks.Ordered())
			{
				taskOwner.Lock();
				LastCall.Repeat.Once();

				taskOwner.SetDirty();
				LastCall.Repeat.Once();

				taskOwner.Release();
				LastCall.Repeat.Once();
			}

			mocks.ReplayAll();

			_workloadDayBase.AddParent(taskOwner);
			_workloadDayBase.Lock();
			_workloadDayBase.Tasks = 900;
			_workloadDayBase.AverageTaskTime = TimeSpan.FromSeconds(5);
			_workloadDayBase.Release();

			mocks.VerifyAll();
		}

		[Test]
		public void VerifyChangeValuesWithParentInLockedModeFromTaskPeriod()
		{
			MockRepository mocks = new MockRepository();
			ITaskOwner taskOwner = mocks.StrictMock<ITaskOwner>();

			using (mocks.Ordered())
			{
				taskOwner.Lock();
				LastCall.Repeat.Once();

				taskOwner.SetDirty();
				LastCall.Repeat.Once();

				taskOwner.Release();
				LastCall.Repeat.Once();
			}

			mocks.ReplayAll();

			_workloadDayBase.AddParent(taskOwner);
			_workloadDayBase.Lock();
			_workloadDayBase.TaskPeriodList[0].Tasks = 900;
			_workloadDayBase.TaskPeriodList[0].AverageTaskTime = TimeSpan.FromSeconds(5);
			_workloadDayBase.Release();

			mocks.VerifyAll();
		}

		[Test]
		public void VerifyCurrentDate()
		{
			//WorkloadDayBase is not connected to a date, thereby this property will return MinValue
			Assert.AreEqual(_workloadDayBase.CurrentDate, SkillDayTemplate.BaseDate);
		}

		[Test]
		public void CanSplitTaskPeriodsIntoDefaultResolution()
		{
			_workload.Skill.DefaultResolution = 15;
			IList<TimePeriod> openHours = new List<TimePeriod>();
			TimePeriod timePeriod = new TimePeriod("8-17");
			openHours.Add(timePeriod);

			TestWorkloadDayBase workloadDayBase = new TestWorkloadDayBase();
			workloadDayBase.Create(SkillDayTemplate.BaseDate, _workload, openHours);

			Assert.AreEqual(36, workloadDayBase.TaskPeriodList.Count);

			//If day is closed.
			workloadDayBase.ChangeOpenHours(new List<TimePeriod>());

			Assert.AreEqual(0, workloadDayBase.TaskPeriodList.Count);

			_workload.Skill.DefaultResolution = 17;
			workloadDayBase.Create(workloadDayBase.CurrentDate, _workload, openHours);

			Assert.AreEqual(31, workloadDayBase.TaskPeriodList.Count);
		}

		[Test]
		public void CanAddNewOpenHourListAndDefaultTaskListIsCreated()
		{
			_workload.Skill.DefaultResolution = 15;
			IList<TimePeriod> openHours = new List<TimePeriod>();
			TimePeriod timePeriod = new TimePeriod("8-17");
			openHours.Add(timePeriod);

			//Day is closed
			_workloadDayBase.ChangeOpenHours(new List<TimePeriod>());
			_workloadDayBase.ChangeOpenHours(openHours);

			Assert.AreEqual(36, _workloadDayBase.TaskPeriodList.Count);
		}

		[Test]
		public void CanChangeOpenHours()
		{
			_workload.Skill.DefaultResolution = 15;
			_workloadDayBase.Close();
			IList<TimePeriod> openHours = new List<TimePeriod>();
			TimePeriod timePeriod = new TimePeriod("8-17");
			openHours.Add(timePeriod);

			_workloadDayBase.ChangeOpenHours(openHours);

			Assert.AreEqual(36, _workloadDayBase.TaskPeriodList.Count);

			IList<TimePeriod> openHours2 = new List<TimePeriod>();
			TimePeriod timePeriod2 = new TimePeriod("7-17");
			openHours2.Add(timePeriod2);
			_workloadDayBase.ChangeOpenHours(openHours2);

			Assert.AreEqual(40, _workloadDayBase.TaskPeriodList.Count);

			IList<TimePeriod> openHours3 = new List<TimePeriod>();
			TimePeriod timePeriod3 = new TimePeriod("7-18");
			openHours3.Add(timePeriod3);

			_workloadDayBase.ChangeOpenHours(openHours3);
			Assert.AreEqual(44, _workloadDayBase.TaskPeriodList.Count);

			IList<TimePeriod> openHours4 = new List<TimePeriod>();
			TimePeriod timePeriod4 = new TimePeriod("8-18");
			openHours4.Add(timePeriod4);

			_workloadDayBase.ChangeOpenHours(openHours4);

			Assert.AreEqual(40, _workloadDayBase.TaskPeriodList.Count);

			IList<TimePeriod> openHours5 = new List<TimePeriod>();
			TimePeriod timePeriod5 = new TimePeriod("9-19");
			openHours5.Add(timePeriod5);

			_workloadDayBase.ChangeOpenHours(openHours5);

			Assert.AreEqual(40, _workloadDayBase.TaskPeriodList.Count);

			IList<TimePeriod> openHours6 = new List<TimePeriod>();
			TimePeriod timePeriod6 = new TimePeriod("9-18");
			openHours6.Add(timePeriod6);

			_workloadDayBase.ChangeOpenHours(openHours6);

			Assert.AreEqual(36, _workloadDayBase.TaskPeriodList.Count);

			IList<TimePeriod> openHours7 = new List<TimePeriod>();
			TimePeriod timePeriod7 = new TimePeriod("10-17");
			openHours7.Add(timePeriod7);

			_workloadDayBase.ChangeOpenHours(openHours7);

			Assert.AreEqual(28, _workloadDayBase.TaskPeriodList.Count);

			IList<TimePeriod> openHours8 = new List<TimePeriod>();
			TimePeriod timePeriod8 = new TimePeriod("9-18");
			openHours8.Add(timePeriod8);

			_workloadDayBase.ChangeOpenHours(openHours8);

			Assert.AreEqual(36, _workloadDayBase.TaskPeriodList.Count);
		}

		[Test]
		public void CanClearTaskList()
		{
			Assert.AreEqual(68, _workloadDayBase.TaskPeriodList.Count);
			((TestWorkloadDayBase)_workloadDayBase).TestClear();
			Assert.AreEqual(0, _workloadDayBase.TaskPeriodList.Count);
		}

		[Test]
		public void VerifySplitTemplateTaskPeriod()
		{
			MockRepository mocks = new MockRepository();
			IQueueStatisticsProvider provider = mocks.StrictMock<IQueueStatisticsProvider>();

			using (mocks.Record())
			{
				Expect.Call(provider.GetStatisticsForPeriod(_workloadDayBase.TaskPeriodList[0].Period))
					.Return(new StatisticTask
					{
						Interval = _workloadDayBase.TaskPeriodList[0].Period.StartDateTime,
						StatCalculatedTasks = 200
					})
					.Repeat.AtLeastOnce();
				Expect.Call(provider.GetStatisticsForPeriod(new DateTimePeriod())).IgnoreArguments()
					.Return(new StatisticTask())
					.Repeat.AtLeastOnce();
			}

			using (mocks.Playback())
			{
				_workloadDayBase.SetQueueStatistics(provider);

				Assert.AreEqual(200, _workloadDayBase.TaskPeriodList[0].StatisticTask.StatCalculatedTasks);
				Assert.AreEqual(68, _workloadDayBase.TaskPeriodList.Count);

				_workloadDayBase.MergeTemplateTaskPeriods(new List<ITemplateTaskPeriod>(_workloadDayBase.TaskPeriodList));

				Assert.AreEqual(1, _workloadDayBase.TaskPeriodList.Count);

				_workloadDayBase.TaskPeriodList[0].Tasks = 104d;
				_workloadDayBase.TaskPeriodList[0].AverageTaskTime = TimeSpan.FromSeconds(10);
				_workloadDayBase.TaskPeriodList[0].AverageAfterTaskTime = TimeSpan.FromSeconds(20);
				_workloadDayBase.TaskPeriodList[0].CampaignTasks = new Percent(0.1d);
				_workloadDayBase.TaskPeriodList[0].CampaignTaskTime = new Percent(0.2d);
				_workloadDayBase.TaskPeriodList[0].CampaignAfterTaskTime = new Percent(0.3d);

				_workloadDayBase.SplitTemplateTaskPeriods(
					new List<ITemplateTaskPeriod>
					{
						_workloadDayBase.TaskPeriodList[0]
					});

				Assert.AreEqual(68, _workloadDayBase.TaskPeriodList.Count);
				Assert.AreEqual(1.529d, _workloadDayBase.TaskPeriodList[1].Tasks, 0.001);
				Assert.AreEqual(TimeSpan.FromSeconds(10), _workloadDayBase.TaskPeriodList[1].AverageTaskTime);
				Assert.AreEqual(TimeSpan.FromSeconds(20), _workloadDayBase.TaskPeriodList[1].AverageAfterTaskTime);
				Assert.AreEqual(0.1d, _workloadDayBase.TaskPeriodList[1].CampaignTasks.Value);
				Assert.AreEqual(0.2d, _workloadDayBase.TaskPeriodList[1].CampaignTaskTime.Value);
				Assert.AreEqual(0.3d, _workloadDayBase.TaskPeriodList[1].CampaignAfterTaskTime.Value);
				Assert.AreEqual(1.529d, _workloadDayBase.TaskPeriodList[52].Tasks, 0.001);
				Assert.AreEqual(TimeSpan.FromSeconds(10), _workloadDayBase.TaskPeriodList[52].AverageTaskTime);
				Assert.AreEqual(TimeSpan.FromSeconds(20), _workloadDayBase.TaskPeriodList[52].AverageAfterTaskTime);
				Assert.AreEqual(0.1d, _workloadDayBase.TaskPeriodList[52].CampaignTasks.Value);
				Assert.AreEqual(0.2d, _workloadDayBase.TaskPeriodList[52].CampaignTaskTime.Value);
				Assert.AreEqual(0.3d, _workloadDayBase.TaskPeriodList[52].CampaignAfterTaskTime.Value);
				Assert.AreEqual(200, _workloadDayBase.TaskPeriodList[0].StatisticTask.StatCalculatedTasks);
			}
		}

		[Test]
		public void VerifyMergeTemplateTaskPeriod()
		{
			Assert.AreEqual(68, _workloadDayBase.TaskPeriodList.Count);
			_workloadDayBase.MergeTemplateTaskPeriods(new List<ITemplateTaskPeriod>(_workloadDayBase.TaskPeriodList));
			Assert.AreEqual(1, _workloadDayBase.TaskPeriodList.Count);
		}

		[Test]
		public void ShouldNotMergeTemplateTaskPeriodFromOtherParent()
		{
			_workload.TemplateWeekCollection[0].MakeOpen24Hours();
			Assert.AreNotEqual(0, _workload.TemplateWeekCollection[0].TaskPeriodList.Count);
			Assert.Throws<ArgumentException>(() => _workloadDayBase.MergeTemplateTaskPeriods(new List<ITemplateTaskPeriod>(_workload.TemplateWeekCollection[0].TaskPeriodList)));
		}

		[Test]
		public void ShouldNotSplitTemplateTaskPeriodFromOtherParent()
		{
			_workload.TemplateWeekCollection[0].MakeOpen24Hours();
			Assert.AreNotEqual(0, _workload.TemplateWeekCollection[0].TaskPeriodList.Count);
			Assert.Throws<ArgumentException>(() => _workloadDayBase.SplitTemplateTaskPeriods(new List<ITemplateTaskPeriod>(_workload.TemplateWeekCollection[0].TaskPeriodList)));
		}

		[Test]
		public void VerifyMergeTemplateTaskPeriodOnDstSwitch()
		{
			_skill.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			_skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			_openHours = new List<TimePeriod> { new TimePeriod(1, 0, 18, 0) };
			_workloadDayBase.Create(new DateOnly(2009, 3, 29), _workload, _openHours);
			_workloadDayBase.Tasks = 999;
			var startDateTime = _workloadDayBase.SortedTaskPeriodList[0].Period.StartDateTime;
			var endDateTime = _workloadDayBase.SortedTaskPeriodList.Last().Period.EndDateTime;
			Assert.AreEqual(64, _workloadDayBase.TaskPeriodList.Count);
			_workloadDayBase.MergeTemplateTaskPeriods(new List<ITemplateTaskPeriod>(_workloadDayBase.TaskPeriodList));
			Assert.AreEqual(1, _workloadDayBase.TaskPeriodList.Count);
			Assert.AreEqual(startDateTime, _workloadDayBase.SortedTaskPeriodList[0].Period.StartDateTime);
			Assert.AreEqual(endDateTime, _workloadDayBase.SortedTaskPeriodList.Last().Period.EndDateTime);
			_workloadDayBase.SplitTemplateTaskPeriods(new List<ITemplateTaskPeriod>(_workloadDayBase.TaskPeriodList));
			Assert.AreEqual(64, _workloadDayBase.TaskPeriodList.Count);
		}

		[Test]
		public void VerifyMergeTemplateTaskPeriodWhenGoingFromDst()
		{
			_skill.MidnightBreakOffset = TimeSpan.FromHours(1);
			_skill.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			_openHours = new List<TimePeriod> { new TimePeriod(1, 0, 18, 0) };
			_workloadDayBase.Create(new DateOnly(2009, 10, 24), _workload, _openHours);
			_workloadDayBase.Tasks = 999;
			var startDateTime = _workloadDayBase.SortedTaskPeriodList[0].Period.StartDateTime;
			var endDateTime = _workloadDayBase.SortedTaskPeriodList.Last().Period.EndDateTime;
			Assert.AreEqual(68, _workloadDayBase.TaskPeriodList.Count);
			_workloadDayBase.MergeTemplateTaskPeriods(new List<ITemplateTaskPeriod>(_workloadDayBase.TaskPeriodList));
			Assert.AreEqual(1, _workloadDayBase.TaskPeriodList.Count);
			Assert.AreEqual(startDateTime, _workloadDayBase.SortedTaskPeriodList[0].Period.StartDateTime);
			Assert.AreEqual(endDateTime, _workloadDayBase.SortedTaskPeriodList.Last().Period.EndDateTime);
			_workloadDayBase.SplitTemplateTaskPeriods(new List<ITemplateTaskPeriod>(_workloadDayBase.TaskPeriodList));
			Assert.AreEqual(68, _workloadDayBase.TaskPeriodList.Count);
		}

		[Test]
		public void VerifyMergeTemplateTaskPeriodWithNoPeriods()
		{
			Assert.AreEqual(68, _workloadDayBase.TaskPeriodList.Count);
			_workloadDayBase.MergeTemplateTaskPeriods(new List<ITemplateTaskPeriod>());
			Assert.AreEqual(68, _workloadDayBase.TaskPeriodList.Count);
		}

		[Test]
		public void VerifyMergeTemplateTaskPeriodWithCampaign()
		{
			Assert.AreEqual(68, _workloadDayBase.TaskPeriodList.Count);
			_workloadDayBase.Lock();
			for (int i = 0; i < _workloadDayBase.TaskPeriodList.Count; i++)
			{
				_workloadDayBase.TaskPeriodList[i].Tasks = 100;
				_workloadDayBase.TaskPeriodList[i].AverageTaskTime = TimeSpan.FromSeconds(100);
				_workloadDayBase.TaskPeriodList[i].AverageAfterTaskTime = TimeSpan.FromSeconds(10);
				if (i < 34)
				{
					_workloadDayBase.TaskPeriodList[i].CampaignTasks = new Percent(0.1d);
					_workloadDayBase.TaskPeriodList[i].CampaignTaskTime = new Percent(0.2d);
					_workloadDayBase.TaskPeriodList[i].CampaignAfterTaskTime = new Percent(0.4d);
				}
			}
			_workloadDayBase.Release();
			_workloadDayBase.MergeTemplateTaskPeriods(new List<ITemplateTaskPeriod>(_workloadDayBase.TaskPeriodList));
			Assert.AreEqual(1, _workloadDayBase.TaskPeriodList.Count);
			Assert.AreEqual(0.05d,_workloadDayBase.TaskPeriodList[0].CampaignTasks.Value, 0.001d);
			Assert.AreEqual(0.1d, _workloadDayBase.TaskPeriodList[0].CampaignTaskTime.Value, 0.001d);
			Assert.AreEqual(0.2d, _workloadDayBase.TaskPeriodList[0].CampaignAfterTaskTime.Value, 0.001d);
		}

		[Test]
		public void VerifyTaskPeriodListPeriodWorks()
		{
			_workload.Skill.DefaultResolution = 15;
			IList<TimePeriod> openHours = new List<TimePeriod>();
			TimePeriod timePeriod = new TimePeriod("8-17");
			openHours.Add(timePeriod);

			//Day is closed
			_workloadDayBase.ChangeOpenHours(new List<TimePeriod>());

			_workloadDayBase.ChangeOpenHours(openHours);

			Assert.AreEqual(36, _workloadDayBase.TaskPeriodList.Count);
			DateTime start = _workloadDayBase.SortedTaskPeriodList[0].Period.StartDateTime;
			DateTime end = _workloadDayBase.SortedTaskPeriodList[_workloadDayBase.TaskPeriodList.Count - 1].Period.EndDateTime;
			Assert.AreEqual(start, _workloadDayBase.TaskPeriodListPeriod.StartDateTime);
			Assert.AreEqual(end, _workloadDayBase.TaskPeriodListPeriod.EndDateTime);
		}

		[Test]
		public void VerifyRecalculateDailyTaskStatisticsWorks()
		{
			Assert.AreEqual(0d, _workloadDayBase.TotalStatisticCalculatedTasks);
			Assert.AreEqual(0d, _workloadDayBase.TotalStatisticAbandonedTasks);
			Assert.AreEqual(0d, _workloadDayBase.TotalStatisticAnsweredTasks);
			_workloadDayBase.TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 100d;
			_workloadDayBase.TaskPeriodList[0].StatisticTask.StatAbandonedTasks = 50d;
			_workloadDayBase.TaskPeriodList[0].StatisticTask.StatAnsweredTasks = 150d;
			_workloadDayBase.RecalculateDailyStatisticTasks();
			Assert.AreEqual(100d, _workloadDayBase.TotalStatisticCalculatedTasks);
			Assert.AreEqual(50d, _workloadDayBase.TotalStatisticAbandonedTasks);
			Assert.AreEqual(150d, _workloadDayBase.TotalStatisticAnsweredTasks);
		}

		[Test]
		public void VerifyRecalculateDailyAverageStatisticTimesWorks()
		{
			Assert.AreEqual(TimeSpan.Zero, _workloadDayBase.TotalStatisticAverageTaskTime);
			Assert.AreEqual(TimeSpan.Zero, _workloadDayBase.TotalStatisticAverageAfterTaskTime);
			_workloadDayBase.TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 120 * 68; //68 periods
			_workloadDayBase.TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 60 * 68; //68 periods
			_workloadDayBase.RecalculateDailyAverageStatisticTimes();
			Assert.AreEqual(TimeSpan.FromSeconds(120), _workloadDayBase.TotalStatisticAverageTaskTime);
			Assert.AreEqual(TimeSpan.FromSeconds(60), _workloadDayBase.TotalStatisticAverageAfterTaskTime);
		}

		[Test]
		public void VerifyRecalculateDailyAverageStatisticTimesWorksWithTasks()
		{
			Assert.AreEqual(TimeSpan.Zero, _workloadDayBase.TotalStatisticAverageTaskTime);
			Assert.AreEqual(TimeSpan.Zero, _workloadDayBase.TotalStatisticAverageAfterTaskTime);
			_workloadDayBase.TaskPeriodList[0].StatisticTask.StatAnsweredTasks = 10d;
			_workloadDayBase.RecalculateDailyStatisticTasks();
			_workloadDayBase.TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 120;
			_workloadDayBase.TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 60;
			_workloadDayBase.RecalculateDailyAverageStatisticTimes();
			Assert.AreEqual(TimeSpan.FromSeconds(120), _workloadDayBase.TotalStatisticAverageTaskTime);
			Assert.AreEqual(TimeSpan.FromSeconds(60), _workloadDayBase.TotalStatisticAverageAfterTaskTime);
		}

		[Test]
		public void VerifySetCampaignTasksWhenClosedGivesException()
		{
			_workloadDayBase.Close();
			Assert.Throws<InvalidOperationException>(() => _workloadDayBase.CampaignTasks = new Percent());
		}

		[Test]
		public void VerifySetCampaignTaskTimeWhenClosedGivesException()
		{
			_workloadDayBase.Close();
			Assert.Throws<InvalidOperationException>(() => _workloadDayBase.CampaignTaskTime = new Percent());
		}

		[Test]
		public void VerifySetCampaignAfterTaskTimeWhenClosedGivesException()
		{
			_workloadDayBase.Close();
			Assert.Throws<InvalidOperationException>(() => _workloadDayBase.CampaignAfterTaskTime = new Percent());
		}

		[Test]
		public void VerifySetTasksWhenClosedGivesException()
		{
			_workloadDayBase.Close();
			Assert.Throws<InvalidOperationException>(() => _workloadDayBase.Tasks = 1d);
		}

		[Test]
		public void VerifySetEmailsWhenClosed()
		{
			_skill.SkillType.ForecastSource = ForecastSource.Email;
			_workloadDayBase.Close();
			_workloadDayBase.CampaignTaskTime = new Percent(0.2);
			_workloadDayBase.CampaignAfterTaskTime = new Percent(0.2);
			_workloadDayBase.Tasks = 1d;
			Assert.AreEqual(new Percent(0.2), _workloadDayBase.CampaignAfterTaskTime);
			Assert.AreEqual(new Percent(0.2), _workloadDayBase.CampaignTaskTime);
			Assert.AreEqual(1d, Math.Round(_workloadDayBase.Tasks, 2));
		}

		[Test]
		public void VerifyOnCampaignTasksChangedIsTriggeredWithLockAndParent()
		{
			MockRepository mocks = new MockRepository();
			ITaskOwner taskOwnerParent = mocks.StrictMock<ITaskOwner>();

			taskOwnerParent.Lock();
			LastCall.Repeat.Once();

			taskOwnerParent.SetDirty();
			LastCall.Repeat.Once();

			taskOwnerParent.Release();
			LastCall.Repeat.Once();

			mocks.ReplayAll();

			_workloadDayBase.AddParent(taskOwnerParent);
			_workloadDayBase.Lock();
			_workloadDayBase.CampaignTasks = new Percent(0.3d);
			_workloadDayBase.Release();

			mocks.VerifyAll();
		}

		[Test]
		public void VerifyOnCampaignAverageTimesChangedIsTriggeredWithLockAndParent()
		{
			MockRepository mocks = new MockRepository();
			ITaskOwner taskOwnerParent = mocks.StrictMock<ITaskOwner>();

			taskOwnerParent.Lock();
			LastCall.Repeat.Once();

			taskOwnerParent.SetDirty();
			LastCall.Repeat.Once();

			taskOwnerParent.Release();
			LastCall.Repeat.Once();

			mocks.ReplayAll();

			_workloadDayBase.AddParent(taskOwnerParent);
			_workloadDayBase.Lock();
			_workloadDayBase.CampaignTaskTime = new Percent(0.3d);
			_workloadDayBase.Release();

			mocks.VerifyAll();
		}

		[Test]
		public void VerifyRecalculateCampaignWhenClosed()
		{
			_workloadDayBase.Close();
			_workloadDayBase.RecalculateDailyCampaignTasks();
			Assert.AreEqual(new Percent(), _workloadDayBase.CampaignTasks);
			_workloadDayBase.RecalculateDailyAverageCampaignTimes();
			Assert.AreEqual(new Percent(), _workloadDayBase.CampaignTaskTime);
			Assert.AreEqual(new Percent(), _workloadDayBase.CampaignAfterTaskTime);
		}

		[Test]
		public void ShouldRecalculateCampaignTasksCorrect()
		{
			_workloadDayBase.CampaignTasks = new Percent(0.5d);
			_workloadDayBase.RecalculateDailyCampaignTasks();
			
			Assert.AreEqual(new Percent(0.5d).Value, Math.Round(_workloadDayBase.CampaignTasks.Value, 2));
		}

		[Test]
		public void ShouldRecalculateCampaignTaskTimesCorrect()
		{
			_workloadDayBase.AverageTaskTime = TimeSpan.FromSeconds(30);
			_workloadDayBase.AverageAfterTaskTime = TimeSpan.FromSeconds(60);
			_workloadDayBase.CampaignTaskTime = new Percent(0.5d);
			_workloadDayBase.CampaignAfterTaskTime = new Percent(0.75d);
			_workloadDayBase.RecalculateDailyAverageCampaignTimes();

			Assert.AreEqual(new Percent(0.5d).Value, Math.Round(_workloadDayBase.CampaignTaskTime.Value, 2));
			Assert.AreEqual(new Percent(0.75d).Value, Math.Round(_workloadDayBase.CampaignAfterTaskTime.Value, 2));
		}

		[Test]
		public void VerifyCampaignTimesCanBeSet()
		{
			_workloadDayBase.Lock();
			foreach (TemplateTaskPeriod taskPeriod in _workloadDayBase.TaskPeriodList)
			{
				taskPeriod.AverageTaskTime = TimeSpan.FromSeconds(40);
				taskPeriod.AverageAfterTaskTime = TimeSpan.FromSeconds(80);
			}
			_workloadDayBase.Release();

			Assert.AreEqual(TimeSpan.FromSeconds(40).TotalSeconds, Math.Round(_workloadDayBase.AverageTaskTime.TotalSeconds, 2));
			Assert.AreEqual(TimeSpan.FromSeconds(80).TotalSeconds, Math.Round(_workloadDayBase.AverageAfterTaskTime.TotalSeconds, 2));
			Assert.AreEqual(TimeSpan.FromSeconds(40).TotalSeconds, Math.Round(_workloadDayBase.TotalAverageTaskTime.TotalSeconds, 2));
			Assert.AreEqual(TimeSpan.FromSeconds(80).TotalSeconds, Math.Round(_workloadDayBase.TotalAverageAfterTaskTime.TotalSeconds, 2));
			_workloadDayBase.CampaignTaskTime = new Percent(0.25d);
			_workloadDayBase.CampaignAfterTaskTime = new Percent(0.5d);
			Assert.AreEqual(TimeSpan.FromSeconds(40).TotalSeconds, Math.Round(_workloadDayBase.AverageTaskTime.TotalSeconds, 2));
			Assert.AreEqual(TimeSpan.FromSeconds(80).TotalSeconds, Math.Round(_workloadDayBase.AverageAfterTaskTime.TotalSeconds, 2));
			Assert.AreEqual(TimeSpan.FromSeconds(50).TotalSeconds, Math.Round(_workloadDayBase.TotalAverageTaskTime.TotalSeconds, 2));
			Assert.AreEqual(TimeSpan.FromSeconds(120).TotalSeconds, Math.Round(_workloadDayBase.TotalAverageAfterTaskTime.TotalSeconds, 2));

			Assert.AreEqual(68, _workloadDayBase.TaskPeriodList.Count);
			Assert.AreEqual(TimeSpan.FromSeconds(40).TotalSeconds, Math.Round(_workloadDayBase.TaskPeriodList[0].AverageTaskTime.TotalSeconds, 2));
			Assert.AreEqual(TimeSpan.FromSeconds(80).TotalSeconds, Math.Round(_workloadDayBase.TaskPeriodList[0].AverageAfterTaskTime.TotalSeconds, 2));
			Assert.AreEqual(TimeSpan.FromSeconds(50).TotalSeconds, Math.Round(_workloadDayBase.TaskPeriodList[0].TotalAverageTaskTime.TotalSeconds, 2));
			Assert.AreEqual(TimeSpan.FromSeconds(120).TotalSeconds, Math.Round(_workloadDayBase.TaskPeriodList[0].TotalAverageAfterTaskTime.TotalSeconds, 2));
			Assert.AreEqual(TimeSpan.FromSeconds(40).TotalSeconds, Math.Round(_workloadDayBase.TaskPeriodList[67].AverageTaskTime.TotalSeconds, 2));
			Assert.AreEqual(TimeSpan.FromSeconds(80).TotalSeconds, Math.Round(_workloadDayBase.TaskPeriodList[67].AverageAfterTaskTime.TotalSeconds, 2));
			Assert.AreEqual(TimeSpan.FromSeconds(50).TotalSeconds, Math.Round(_workloadDayBase.TaskPeriodList[67].TotalAverageTaskTime.TotalSeconds, 2));
			Assert.AreEqual(TimeSpan.FromSeconds(120).TotalSeconds, Math.Round(_workloadDayBase.TaskPeriodList[67].TotalAverageAfterTaskTime.TotalSeconds, 2));
		}

		[Test]
		public void CanReset()
		{
			_workloadDayBase.CampaignAfterTaskTime = new Percent(2);
			_workloadDayBase.CampaignTaskTime = new Percent(2);
			_workloadDayBase.CampaignTasks = new Percent(2);
			_workloadDayBase.Tasks = 222;
			_workloadDayBase.AverageAfterTaskTime = new TimeSpan(222);
			_workloadDayBase.AverageTaskTime = new TimeSpan(222);

			Assert.AreNotEqual(0, _workloadDayBase.Tasks);
			Assert.AreNotEqual(new TimeSpan(0), _workloadDayBase.AverageAfterTaskTime);
			Assert.AreNotEqual(new TimeSpan(0), _workloadDayBase.AverageTaskTime);
			Assert.AreNotEqual(new Percent(0), _workloadDayBase.CampaignAfterTaskTime);
			Assert.AreNotEqual(new Percent(0), _workloadDayBase.CampaignTasks);
			Assert.AreNotEqual(new Percent(0), _workloadDayBase.CampaignTaskTime);

			_workloadDayBase.ResetTaskOwner();

			Assert.AreEqual(0, _workloadDayBase.Tasks);
			Assert.AreEqual(new TimeSpan(0), _workloadDayBase.AverageAfterTaskTime);
			Assert.AreEqual(new TimeSpan(0), _workloadDayBase.AverageTaskTime);
			Assert.AreEqual(new Percent(0), _workloadDayBase.CampaignAfterTaskTime);
			Assert.AreEqual(new Percent(0), _workloadDayBase.CampaignTasks);
			Assert.AreEqual(new Percent(0), _workloadDayBase.CampaignTaskTime);
		}

		//Exposure of a bug that removes the first period when lengthen the openours
		[Test]
		public void PeriodsAreSetCorrectly()
		{
			IList<TimePeriod> openHours = new List<TimePeriod>();
			openHours.Add(new TimePeriod("8:00-17:00"));

			WorkloadDayBase workloadDayBase = new TestWorkloadDayBase();
			_workload.Skill.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));
			DateOnly dateTime = new DateOnly(1800, 1, 1);

			workloadDayBase.Create(dateTime, _workload, openHours);

			int count = workloadDayBase.TaskPeriodList.Count;
			IList<TimePeriod> newOpenHours = new List<TimePeriod>();
			newOpenHours.Add(new TimePeriod("8:00-15:00"));

			workloadDayBase.ChangeOpenHours(newOpenHours);

			Assert.AreEqual(count - 8, workloadDayBase.TaskPeriodList.Count);
			Assert.AreEqual(new DateTime(2002, 12, 12, 8, 0, 0).TimeOfDay, workloadDayBase.SortedTaskPeriodList[0].Period.StartDateTime.TimeOfDay);
			Assert.AreEqual(new DateTime(2002, 12, 12, 15, 0, 0).TimeOfDay, workloadDayBase.SortedTaskPeriodList[27].Period.EndDateTime.TimeOfDay);
		}

		/// <summary>
		/// Determines whether this instance [can add periods to workload with skill with another midnight break].
		/// SPI: 8006
		/// Renders: A task period must be contained by open hours.
		/// </summary>
		/// <remarks>
		/// Created by: henryg
		/// Created date: 2009-10-04
		/// </remarks>
		[Test]
		public void CanAddPeriodsToWorkloadWithSkillWithAnotherMidnightBreak()
		{
			ISkill skill = SkillFactory.CreateSkill("testSkill", SkillTypeFactory.CreateSkillType(), 15, (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")), new TimeSpan(2, 0, 0));

			IWorkload workload = new Workload(skill);

			_openHours.Clear();
			WorkloadDayBase workloadDayBase = new TestWorkloadDayBase();

			var baseDateTime = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, skill.TimeZone);

			_openHours.Add(new TimePeriod(new TimeSpan(8, 0, 0), new TimeSpan(1, 2, 0, 0)));
			workloadDayBase.Create(SkillDayTemplate.BaseDate, workload, _openHours);

			workloadDayBase.Tasks = 999;

			Assert.IsTrue(skill.MidnightBreakOffset.Equals(new TimeSpan(2, 0, 0)));
			workloadDayBase.MakeOpen24Hours();
			Assert.AreEqual(baseDateTime.AddHours(2), workloadDayBase.SortedTaskPeriodList.First().Period.StartDateTime);
			Assert.AreEqual(baseDateTime.AddHours(25).AddMinutes(45), workloadDayBase.SortedTaskPeriodList.Last().Period.StartDateTime);
		}

		[Test]
		public void CanAddPeriodsToWorkloadWithEmailSkillWithAnotherMidnightBreak()
		{
			ISkill skill = SkillFactory.CreateSkill("testSkill", SkillTypeFactory.CreateSkillTypeEmail(), 60, (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")), new TimeSpan(2, 0, 0));

			IWorkload workload = new Workload(skill);

			_openHours.Clear();
			WorkloadDayBase workloadDayBase = new TestWorkloadDayBase();

			DateTime baseDateTime = TimeZoneInfo.ConvertTimeToUtc(SkillDayTemplate.BaseDate.Date, skill.TimeZone);

			_openHours.Add(new TimePeriod(new TimeSpan(8, 0, 0), new TimeSpan(1, 2, 0, 0)));
			workloadDayBase.Create(SkillDayTemplate.BaseDate, workload, _openHours);

			workloadDayBase.Tasks = 999;

			Assert.IsTrue(skill.MidnightBreakOffset.Equals(new TimeSpan(2, 0, 0)));
			Assert.AreEqual(baseDateTime.AddHours(2), workloadDayBase.SortedTaskPeriodList.First().Period.StartDateTime);
			Assert.AreEqual(baseDateTime.AddHours(25), workloadDayBase.SortedTaskPeriodList.Last().Period.StartDateTime);
		}

		[Test]
		public void VerifyCloneTaskPeriodList()
		{
			IList<TimePeriod> openHours = new List<TimePeriod>();
			openHours.Add(new TimePeriod("8:00-17:00"));

			//create a workloadday with a datetime of 10/24/2008
			var date = new DateOnly(2008, 10, 25);
			string templateName = "SUNDAY";
			IWorkloadDayTemplate workloadDayTemplate = new WorkloadDayTemplate();
			if (openHours.Count == 0)
			{
				openHours.Add(new TimePeriod(new TimeSpan(12, 0, 0), new TimeSpan(1, 2, 0, 0)));
				openHours.Add(new TimePeriod(new TimeSpan(6, 0, 0), new TimeSpan(9, 0, 0)));
			}
			workloadDayTemplate.Create(templateName, DateTime.SpecifyKind(date.Date, DateTimeKind.Utc), _workload, openHours);
			workloadDayTemplate.SetId(Guid.NewGuid());
			if (!_workload.TemplateWeekCollection.Values.Contains(workloadDayTemplate)) _workload.AddTemplate(workloadDayTemplate);

			WorkloadDay workloadDay = new WorkloadDay();
			workloadDay.CreateFromTemplate(date, _workload, workloadDayTemplate);
			WorkloadDayBase workloadDayBaseNew = new TestWorkloadDayBase();
			DateOnly baseDate = new DateOnly(1800, 1, 1);
			workloadDayBaseNew.Create(baseDate, _workload, openHours);

			workloadDayBaseNew.CloneTaskPeriodListFrom(workloadDay);
			int count = workloadDay.TaskPeriodList.Count;
			Assert.AreEqual(count, workloadDayBaseNew.TaskPeriodList.Count);
			Assert.AreEqual(workloadDayBaseNew.TaskPeriodListPeriod.StartDateTime.Date, baseDate.Date);
			Assert.AreEqual(workloadDayBaseNew.TaskPeriodListPeriod.EndDateTime.Date, baseDate.Date);
			Assert.AreNotEqual(workloadDayBaseNew.TaskPeriodListPeriod.StartDateTime.Date, workloadDay.CurrentDate.Date);
			Assert.AreNotEqual(workloadDayBaseNew.TaskPeriodListPeriod.EndDateTime.Date, workloadDay.CurrentDate.Date);
		}
		[Test]
		public void VerifyCloneTaskPeriodListWithNullWorkloadDay()
		{
			IList<TimePeriod> openHours = new List<TimePeriod>();
			openHours.Add(new TimePeriod("8:00-17:00"));
			WorkloadDayBase workloadDayBaseNew = new TestWorkloadDayBase();
			DateOnly dateTime = new DateOnly(2010, 1, 1);
			workloadDayBaseNew.Create(dateTime, _workload, openHours);
			workloadDayBaseNew.CloneTaskPeriodListFrom(null);
			DateTime baseDate = new DateTime(1800, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			Assert.AreNotEqual(workloadDayBaseNew.TaskPeriodListPeriod.StartDateTime.Date, baseDate);
		}
		[Test]
		public void TasksCannotBeLessThanZero()
		{
			_workloadDayBase.Tasks = 7000;
			Assert.AreEqual(7000, Math.Round(_workloadDayBase.Tasks, 4));
			_workloadDayBase.Tasks = -1000;
			Assert.AreEqual(0, _workloadDayBase.Tasks);
		}

		[Test]
		public void VerifyAverageTaskTimeAlwaysReflectsTaskPeriodListAverage()
		{
			_workloadDayBase = new TestWorkloadDayBase();
			_workloadDayBase.Create(SkillDayTemplate.BaseDate, _workload, new List<TimePeriod> { new TimePeriod(2, 0, 26, 0) });
			_workloadDayBase.MergeTemplateTaskPeriods(_workloadDayBase.TaskPeriodList);
			_workloadDayBase.Tasks = 0d;
			_workloadDayBase.Tasks = 1.5d;
			_workloadDayBase.Tasks = 0d;
			_workloadDayBase.AverageTaskTime = TimeSpan.FromSeconds(1);
		}

		//jan 1 2011 the timezone was changed in russia, if the openhours was 00 - 00 (+1) and calling ChangeOpenHours
		//on 2010-12-31 it creates an period that is from 00 - 01 (+1) and it raises an error
		[Test]
		public void Bug26815()
		{
			IList<TimePeriod> openHours = new List<TimePeriod>();

			_skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
			_skill.MidnightBreakOffset = TimeSpan.FromHours(0);
			_workloadDayBase.Create(new DateOnly(2010, 12, 31), _workload, openHours);
			_workload.Skill.DefaultResolution = 15;
			_workloadDayBase.Close();
			var timePeriod = new TimePeriod(TimeSpan.FromHours(0), TimeSpan.FromHours(24));
			openHours.Add(timePeriod);

			_workloadDayBase.ChangeOpenHours(openHours);

			Assert.AreEqual(96, _workloadDayBase.TaskPeriodList.Count);

		}

		[Test]
		public void ShouldCalculateAverageTimesWhenMissingAnsweredCalls()
		{
			_workloadDayBase.Close();
			IList<TimePeriod> timePeriods = new TimePeriod[] { new TimePeriod(8, 0, 8, 30) };
			_workloadDayBase.ChangeOpenHours(timePeriods);

			_workloadDayBase.SortedTaskPeriodList[0].StatisticTask.StatAnsweredTasks = 0;
			_workloadDayBase.SortedTaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 60;
			_workloadDayBase.SortedTaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 30;
			_workloadDayBase.SortedTaskPeriodList[1].StatisticTask.StatAnsweredTasks = 1;
			_workloadDayBase.SortedTaskPeriodList[1].StatisticTask.StatAverageTaskTimeSeconds = 120;
			_workloadDayBase.SortedTaskPeriodList[1].StatisticTask.StatAverageAfterTaskTimeSeconds = 60;
			_workloadDayBase.RecalculateDailyAverageStatisticTimes();

			Assert.AreEqual(TimeSpan.FromSeconds(180), _workloadDayBase.TotalStatisticAverageTaskTime);
			Assert.AreEqual(TimeSpan.FromSeconds(90), _workloadDayBase.TotalStatisticAverageAfterTaskTime);
		}
	}

	public class TestWorkloadDayBase : WorkloadDayBase
	{
		public override ITemplateReference TemplateReference
		{
			get { return new TemplateReference(Guid.Empty, 0, string.Empty, null); }
			protected set { }
		}

		public void TestClear()
		{
			ClearTaskList();
		}

		public override void ClearTemplateName()
		{
		}
	}
}
