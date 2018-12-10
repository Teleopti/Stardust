using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
	[TestFixture, Category("BucketB")]
	public class VolumeYearTest
	{
		private ISkill _skill;
		private IWorkload _workload;
		private DateOnly _dateInMonth;

		[SetUp]
		public void Setup()
		{
			_skill = SkillFactory.CreateSkill("newSkill");
			_workload = WorkloadFactory.CreateWorkload(_skill);

			_dateInMonth = new DateOnly(2008, 3, 8);
		}

		#region Common

		/// <summary>
		/// Verifies the clear period type collection.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-04-22
		/// </remarks>
		[Test]
		public void VerifyClearPeriodTypeCollection()
		{
			WeekOfMonth weekOfMonth =
				new WeekOfMonth(
					new TaskOwnerPeriod(_dateInMonth,
										WorkloadDayFactory.GetWorkloadDaysForTest(_dateInMonth.Date, _dateInMonth.Date, _workload)
											.OfType<ITaskOwner>(), TaskOwnerPeriodType.Other), new WeekOfMonthCreator());

			Assert.AreEqual(5, weekOfMonth.PeriodTypeCollection.Count);
			weekOfMonth.ReloadHistoricalDataDepth(new TaskOwnerPeriod(_dateInMonth, new List<ITaskOwner>(),
																	  TaskOwnerPeriodType.Other));
			Assert.AreEqual(0, weekOfMonth.PeriodTypeCollection.Count);
		}

		private void areEqualWithDelta(double expected, double actual)
		{
			Assert.AreEqual(expected, actual, 0.0000001);
		}
		#endregion

		#region Month
		[Test]
		public void CanCalculateAverageCallsForPacificTimeZone()
		{
			_skill.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
			TaskOwnerHelper periodForMonth =
				SkillDayFactory.GenerateStatisticsForDayTests(
					TimeZoneInfo.ConvertTimeToUtc(new DateTime(2001, 09, 18), _skill.TimeZone), _workload);
			var monthPeriod = new TaskOwnerPeriod(_dateInMonth, periodForMonth.TaskOwnerDays, TaskOwnerPeriodType.Other);
			MonthOfYear monthOfYear = new MonthOfYear(monthPeriod, new MonthOfYearCreator());

			Assert.Greater(Math.Round(monthOfYear.AverageTasksPerDay), 0);
		}

		[Test]
		public void CanCalculateAverageCalls()
		{
			TaskOwnerHelper periodForMonth = SkillDayFactory.GenerateStatistics(_dateInMonth, _workload);
			var monthPeriod = new TaskOwnerPeriod(_dateInMonth, periodForMonth.TaskOwnerDays, TaskOwnerPeriodType.Other);
			MonthOfYear monthOfYear = new MonthOfYear(monthPeriod, new MonthOfYearCreator());

			Assert.AreEqual(138.3333, Math.Round(monthOfYear.AverageTasksPerDay, 4));
		}

		[Test]
		public void CanReloadHistoricalDataDepthMonth()
		{
			TaskOwnerHelper periodForMonth = SkillDayFactory.GenerateStatistics(_dateInMonth, _workload);
			var monthPeriod = new TaskOwnerPeriod(_dateInMonth, periodForMonth.TaskOwnerDays, TaskOwnerPeriodType.Other);
			MonthOfYear monthOfYear = new MonthOfYear(monthPeriod, new MonthOfYearCreator());

			monthOfYear.ReloadHistoricalDataDepth(new TaskOwnerPeriod(_dateInMonth, new List<ITaskOwner>(), TaskOwnerPeriodType.Other));
			Assert.AreEqual(0, monthOfYear.PeriodTypeCollection.Count);
			monthOfYear.ReloadHistoricalDataDepth(monthPeriod);
			Assert.IsTrue(monthOfYear.PeriodTypeCollection.Count > 0);
		}

		[Test]
		public void VerifyCanGetTaskIndexByDateForMonth()
		{
			TaskOwnerHelper periodForMonth = SkillDayFactory.GenerateStatistics(_dateInMonth, _workload);
			var monthPeriod = new TaskOwnerPeriod(_dateInMonth, periodForMonth.TaskOwnerDays, TaskOwnerPeriodType.Other);
			MonthOfYear monthOfYear = new MonthOfYear(monthPeriod, new MonthOfYearCreator());

			double taskIndexJanuari = monthOfYear.PeriodTypeCollection[1].TaskIndex;
			double taskIndexJuly = monthOfYear.PeriodTypeCollection[7].TaskIndex;
			double taskIndexOctober = monthOfYear.PeriodTypeCollection[10].TaskIndex;

			var dateTime = new DateOnly(2006, 1, 1); //expect UTC 
			Assert.AreEqual(Math.Round(taskIndexJanuari, 4), Math.Round(monthOfYear.TaskIndex(dateTime), 4));
			Assert.AreEqual(1.0843373493975903d, monthOfYear.TaskIndex(dateTime));

			var dateTimeJuly = new DateOnly(2006, 6, 1); //expect UTC 
			Assert.AreEqual(Math.Round(taskIndexJuly, 4), Math.Round(monthOfYear.TaskIndex(dateTimeJuly), 4));
			Assert.AreEqual(0.93975903614457834d, monthOfYear.TaskIndex(dateTimeJuly));

			var dateTimeOctober = new DateOnly(2006, 10, 1); //expect UTC 
			Assert.AreEqual(Math.Round(taskIndexOctober, 4), Math.Round(monthOfYear.TaskIndex(dateTimeOctober), 4));
			Assert.AreEqual(1.0843373493975903d, monthOfYear.TaskIndex(dateTimeOctober));
		}

		[Test]
		public void VerifyCanGetTalkTimeIndexByDateForMonth()
		{
			TaskOwnerHelper periodForMonth = SkillDayFactory.GenerateStatistics(_dateInMonth, _workload);
			var monthPeriod = new TaskOwnerPeriod(_dateInMonth, periodForMonth.TaskOwnerDays, TaskOwnerPeriodType.Other);
			MonthOfYear monthOfYear = new MonthOfYear(monthPeriod, new MonthOfYearCreator());

			double talkTimeIndexJanuari = monthOfYear.PeriodTypeCollection[1].TalkTimeIndex;
			double talkTimeIndexJuly = monthOfYear.PeriodTypeCollection[7].TalkTimeIndex;
			double talkTimeIndexOctober = monthOfYear.PeriodTypeCollection[10].TalkTimeIndex;

			var dateTime = new DateOnly(2006, 1, 1); //expect UTC 
			Assert.AreEqual(Math.Round(talkTimeIndexJanuari, 4), Math.Round(monthOfYear.TaskTimeIndex(dateTime), 4));
			Assert.AreEqual(0.96554809813242659d, monthOfYear.TaskTimeIndex(dateTime));

			var dateTimeJuly = new DateOnly(2006, 6, 1); //expect UTC 
			Assert.AreEqual(Math.Round(talkTimeIndexJuly, 4), Math.Round(monthOfYear.TaskTimeIndex(dateTimeJuly), 4));
			Assert.AreEqual(1.0283944225960715d, monthOfYear.TaskTimeIndex(dateTimeJuly));

			var dateTimeOctober = new DateOnly(2006, 10, 1); //expect UTC 
			Assert.AreEqual(Math.Round(talkTimeIndexOctober, 4), Math.Round(monthOfYear.TaskTimeIndex(dateTimeOctober), 4));
			Assert.AreEqual(0.96554809813242659d, monthOfYear.TaskTimeIndex(dateTimeOctober));
		}

		[Test]
		public void VerifyCanGetAfterTalkTimeIndexByDateForMonth()
		{
			TaskOwnerHelper periodForMonth = SkillDayFactory.GenerateStatistics(_dateInMonth, _workload);
			var monthPeriod = new TaskOwnerPeriod(_dateInMonth, periodForMonth.TaskOwnerDays, TaskOwnerPeriodType.Other);
			MonthOfYear monthOfYear = new MonthOfYear(monthPeriod, new MonthOfYearCreator());

			double afterTalkTimeIndexJanuari = monthOfYear.PeriodTypeCollection[1].AfterTalkTimeIndex;
			double afterTalkTimeIndexJuly = monthOfYear.PeriodTypeCollection[7].AfterTalkTimeIndex;
			double afterTalkTimeIndexOctober = monthOfYear.PeriodTypeCollection[10].AfterTalkTimeIndex;

			var dateTime = new DateOnly(2006, 1, 1); //expect UTC 
			Assert.AreEqual(Math.Round(afterTalkTimeIndexJanuari, 4), Math.Round(monthOfYear.AfterTaskTimeIndex(dateTime), 4));
			Assert.AreEqual(1.0513333337472268d, monthOfYear.AfterTaskTimeIndex(dateTime));

			var dateTimeJuly = new DateOnly(2006, 6, 1); //expect UTC 
			Assert.AreEqual(Math.Round(afterTalkTimeIndexJuly, 4), Math.Round(monthOfYear.AfterTaskTimeIndex(dateTimeJuly), 4));
			Assert.AreEqual(0.957692307041077d, monthOfYear.AfterTaskTimeIndex(dateTimeJuly));

			var dateTimeOctober = new DateOnly(2006, 10, 1); //expect UTC 
			Assert.AreEqual(Math.Round(afterTalkTimeIndexOctober, 4), Math.Round(monthOfYear.AfterTaskTimeIndex(dateTimeOctober), 4));
			Assert.AreEqual(1.0513333337472268d, monthOfYear.AfterTaskTimeIndex(dateTimeOctober));
		}

		[Test]
		public void VerifyAverageCallsForMonth()
		{
			TaskOwnerHelper periodForMonth = SkillDayFactory.GenerateStatistics(_dateInMonth, _workload);
			var monthPeriod = new TaskOwnerPeriod(_dateInMonth, periodForMonth.TaskOwnerDays, TaskOwnerPeriodType.Other);
			MonthOfYear monthOfYear = new MonthOfYear(monthPeriod, new MonthOfYearCreator());

			IDictionary<int, double> resultsAverageTasks = new Dictionary<int, double>();

			resultsAverageTasks.Add(1, 4650);
			resultsAverageTasks.Add(2, 4200);
			resultsAverageTasks.Add(3, 4030);
			resultsAverageTasks.Add(4, 3900);
			resultsAverageTasks.Add(5, 4030);
			resultsAverageTasks.Add(6, 3900);
			resultsAverageTasks.Add(7, 4030);
			resultsAverageTasks.Add(8, 4030);
			resultsAverageTasks.Add(9, 3900);
			resultsAverageTasks.Add(10, 4650);
			resultsAverageTasks.Add(11, 4500);
			resultsAverageTasks.Add(12, 4650);

			foreach (KeyValuePair<int, double> result in resultsAverageTasks)
			{
				Assert.AreEqual(Math.Round(result.Value, 4), Math.Round(monthOfYear.PeriodTypeCollection[result.Key].AverageTasks, 4));
			}
		}

		[Test]
		public void VerifyDailyAverageTasksForMonth()
		{
			TaskOwnerHelper periodForMonth = SkillDayFactory.GenerateStatistics(_dateInMonth, _workload);
			var monthPeriod = new TaskOwnerPeriod(_dateInMonth, periodForMonth.TaskOwnerDays, TaskOwnerPeriodType.Other);
			MonthOfYear monthOfYear = new MonthOfYear(monthPeriod, new MonthOfYearCreator());

			IDictionary<int, double> resultsDailyAverageTasks = new Dictionary<int, double>();

			resultsDailyAverageTasks.Add(1, 150);
			resultsDailyAverageTasks.Add(2, 150);
			resultsDailyAverageTasks.Add(3, 130);
			resultsDailyAverageTasks.Add(4, 130);
			resultsDailyAverageTasks.Add(5, 130);
			resultsDailyAverageTasks.Add(6, 130);
			resultsDailyAverageTasks.Add(7, 130);
			resultsDailyAverageTasks.Add(8, 130);
			resultsDailyAverageTasks.Add(9, 130);
			resultsDailyAverageTasks.Add(10, 150);
			resultsDailyAverageTasks.Add(11, 150);
			resultsDailyAverageTasks.Add(12, 150);

			foreach (KeyValuePair<int, double> result in resultsDailyAverageTasks)
			{
				Assert.AreEqual(Math.Round(result.Value, 4), Math.Round(monthOfYear.PeriodTypeCollection[result.Key].DailyAverageTasks, 4));
			}

		}
		[Test]
		public void VerifyAverageTalkTimeForMonth()
		{
			TaskOwnerHelper periodForMonth = SkillDayFactory.GenerateStatistics(_dateInMonth, _workload);
			MonthOfYear monthOfYear = new MonthOfYear(new TaskOwnerPeriod(_dateInMonth, periodForMonth.TaskOwnerDays, TaskOwnerPeriodType.Other), new MonthOfYearCreator());

			IDictionary<int, double> resultsAverageTalkTime = new Dictionary<int, double>();

			resultsAverageTalkTime.Add(1, 17.3333333);
			resultsAverageTalkTime.Add(2, 17.3333333);
			resultsAverageTalkTime.Add(3, 18.4615384);
			resultsAverageTalkTime.Add(4, 18.4615384);
			resultsAverageTalkTime.Add(5, 18.4615384);
			resultsAverageTalkTime.Add(6, 18.4615384);
			resultsAverageTalkTime.Add(7, 18.4615384);
			resultsAverageTalkTime.Add(8, 18.4615384);
			resultsAverageTalkTime.Add(9, 18.4615384);
			resultsAverageTalkTime.Add(10, 17.3333333);
			resultsAverageTalkTime.Add(11, 17.3333333);
			resultsAverageTalkTime.Add(12, 17.3333333);

			foreach (KeyValuePair<int, double> result in resultsAverageTalkTime)
			{
				Assert.AreEqual(Math.Round(result.Value, 4), Math.Round(monthOfYear.PeriodTypeCollection[result.Key].AverageTalkTime.TotalSeconds, 4));
			}
		}
		[Test]
		public void VerifyAverageAfterWorkTimeForMonth()
		{
			TaskOwnerHelper periodForMonth = SkillDayFactory.GenerateStatistics(_dateInMonth, _workload);
			var monthPeriod = new TaskOwnerPeriod(_dateInMonth, periodForMonth.TaskOwnerDays, TaskOwnerPeriodType.Other);
			MonthOfYear monthOfYear = new MonthOfYear(monthPeriod, new MonthOfYearCreator());

			IDictionary<int, double> resultsAverageAfterWorkTime = new Dictionary<int, double>();

			resultsAverageAfterWorkTime.Add(1, 63.3333333);
			resultsAverageAfterWorkTime.Add(2, 63.3333333);
			resultsAverageAfterWorkTime.Add(3, 57.6923076);
			resultsAverageAfterWorkTime.Add(4, 57.6923076);
			resultsAverageAfterWorkTime.Add(5, 57.6923076);
			resultsAverageAfterWorkTime.Add(6, 57.6923076);
			resultsAverageAfterWorkTime.Add(7, 57.6923076);
			resultsAverageAfterWorkTime.Add(8, 57.6923076);
			resultsAverageAfterWorkTime.Add(9, 57.6923076);
			resultsAverageAfterWorkTime.Add(10, 63.3333333);
			resultsAverageAfterWorkTime.Add(11, 63.3333333);
			resultsAverageAfterWorkTime.Add(12, 63.3333333);

			foreach (KeyValuePair<int, double> result in resultsAverageAfterWorkTime)
			{
				Assert.AreEqual(Math.Round(result.Value, 4), Math.Round(monthOfYear.PeriodTypeCollection[result.Key].AverageAfterWorkTime.TotalSeconds, 4));
			}
		}

		[Test]
		public void VerifyIndexOnMonths()
		{
			TaskOwnerHelper periodForMonth = SkillDayFactory.GenerateStatistics(_dateInMonth, _workload);
			var monthPeriod = new TaskOwnerPeriod(_dateInMonth, periodForMonth.TaskOwnerDays, TaskOwnerPeriodType.Other);
			MonthOfYear monthOfYear = new MonthOfYear(monthPeriod, new MonthOfYearCreator());

			IDictionary<int, double> resultsCalls = new Dictionary<int, double>();
			IDictionary<int, double> resultsTalkTime = new Dictionary<int, double>();
			IDictionary<int, double> resultsAfterTalkTime = new Dictionary<int, double>();

			resultsCalls.Add(1, 1.084337349);
			resultsCalls.Add(2, 1.084337349);
			resultsCalls.Add(3, 0.9397590361);
			resultsCalls.Add(4, 0.9397590361);
			resultsCalls.Add(5, 0.9397590361);
			resultsCalls.Add(6, 0.9397590361);
			resultsCalls.Add(7, 0.9397590361);
			resultsCalls.Add(8, 0.9397590361);
			resultsCalls.Add(9, 0.9397590361);
			resultsCalls.Add(10, 1.084337349);
			resultsCalls.Add(11, 1.084337349);
			resultsCalls.Add(12, 1.084337349);

			resultsTalkTime.Add(1, 0.96554809813242659);
			resultsTalkTime.Add(2, 0.96554809813242659);
			resultsTalkTime.Add(3, 1.0283944225960715);
			resultsTalkTime.Add(4, 1.0283944225960715);
			resultsTalkTime.Add(5, 1.0283944225960715);
			resultsTalkTime.Add(6, 1.0283944225960715);
			resultsTalkTime.Add(7, 1.0283944225960715);
			resultsTalkTime.Add(8, 1.0283944225960715);
			resultsTalkTime.Add(9, 1.0283944225960715);
			resultsTalkTime.Add(10, 0.96554809813242659);
			resultsTalkTime.Add(11, 0.96554809813242659);
			resultsTalkTime.Add(12, 0.96554809813242659);

			resultsAfterTalkTime.Add(1, 1.0513333337472268);
			resultsAfterTalkTime.Add(2, 1.0513333337472268);
			resultsAfterTalkTime.Add(3, 0.957692307041077);
			resultsAfterTalkTime.Add(4, 0.957692307041077);
			resultsAfterTalkTime.Add(5, 0.957692307041077);
			resultsAfterTalkTime.Add(6, 0.957692307041077);
			resultsAfterTalkTime.Add(7, 0.957692307041077);
			resultsAfterTalkTime.Add(8, 0.957692307041077);
			resultsAfterTalkTime.Add(9, 0.957692307041077);
			resultsAfterTalkTime.Add(10, 1.0513333337472268);
			resultsAfterTalkTime.Add(11, 1.0513333337472268);
			resultsAfterTalkTime.Add(12, 1.0513333337472268);

			foreach (KeyValuePair<int, double> result in resultsCalls)
			{
				Assert.AreEqual(Math.Round(result.Value, 4), Math.Round(monthOfYear.PeriodTypeCollection[result.Key].TaskIndex, 4));
			}
			foreach (KeyValuePair<int, double> result in resultsTalkTime)
			{
				Assert.AreEqual(Math.Round(result.Value, 4), Math.Round(monthOfYear.PeriodTypeCollection[result.Key].TalkTimeIndex, 4));
			}
			foreach (KeyValuePair<int, double> result in resultsAfterTalkTime)
			{
				Assert.AreEqual(Math.Round(result.Value, 4), Math.Round(monthOfYear.PeriodTypeCollection[result.Key].AfterTalkTimeIndex, 4));
			}
		}

		[Test]
		public void VerifyAverageTaskAndTaskIndexProperties()
		{
			TaskOwnerHelper periodForMonth = SkillDayFactory.GenerateStatistics(_dateInMonth, _workload);
			var monthPeriod = new TaskOwnerPeriod(_dateInMonth, periodForMonth.TaskOwnerDays, TaskOwnerPeriodType.Other);
			MonthOfYear monthOfYear = new MonthOfYear(monthPeriod, new MonthOfYearCreator());
			//Ok this start index starts from 1.08, I mean not 1
			double indexBefore = monthOfYear.PeriodTypeCollection[1].TaskIndex;
			double averageBefore = monthOfYear.PeriodTypeCollection[1].AverageTasks;

			//Should be 1.5 larger than the "real index 1 value" after talktimeindex is set
			monthOfYear.PeriodTypeCollection[1].TaskIndex = 1.5;
			//Need to calculate from "index 1" base
			Assert.AreEqual((averageBefore * 1.5) / indexBefore, monthOfYear.PeriodTypeCollection[1].AverageTasks);

			//Should now go back to original
			monthOfYear.PeriodTypeCollection[1].TaskIndex = indexBefore;
			Assert.AreEqual(averageBefore, monthOfYear.PeriodTypeCollection[1].AverageTasks);

			indexBefore = monthOfYear.PeriodTypeCollection[1].TaskIndex;
			double averageBefore2 = monthOfYear.PeriodTypeCollection[1].AverageTasks;
			monthOfYear.PeriodTypeCollection[1].AverageTasks = 2325;

			Assert.AreEqual(Math.Round((2325 / averageBefore2) * indexBefore, 2), Math.Round(monthOfYear.PeriodTypeCollection[1].TaskIndex, 2));

			//Set it back the index should also set back
			monthOfYear.PeriodTypeCollection[1].AverageTasks = averageBefore2;
			Assert.AreEqual(indexBefore, monthOfYear.PeriodTypeCollection[1].TaskIndex);
		}

		[Test]
		public void VerifyAverageTalkTimeAndTalkTimeIndexProperties()
		{
			TaskOwnerHelper periodForMonth = SkillDayFactory.GenerateStatistics(_dateInMonth, _workload);
			var monthPeriod = new TaskOwnerPeriod(_dateInMonth, periodForMonth.TaskOwnerDays, TaskOwnerPeriodType.Other);
			MonthOfYear monthOfYear = new MonthOfYear(monthPeriod, new MonthOfYearCreator());

			double indexBefore = monthOfYear.PeriodTypeCollection[1].TalkTimeIndex;
			double averageBefore = monthOfYear.PeriodTypeCollection[1].AverageTalkTime.TotalSeconds;

			//Should be 1.5 larger than the "real original value" after talktimeindex is set
			monthOfYear.PeriodTypeCollection[1].TalkTimeIndex = 1.5;
			Assert.AreEqual(Math.Round((averageBefore * 1.5) / indexBefore, 3), Math.Round(monthOfYear.PeriodTypeCollection[1].AverageTalkTime.TotalSeconds, 3));


			monthOfYear.PeriodTypeCollection[1].TalkTimeIndex = indexBefore; //Should now go back to original
			Assert.AreEqual(Math.Round(averageBefore, 3), Math.Round(monthOfYear.PeriodTypeCollection[1].AverageTalkTime.TotalSeconds, 3));

			indexBefore = monthOfYear.PeriodTypeCollection[1].TalkTimeIndex;
			double secondsAverageBefore2 = monthOfYear.PeriodTypeCollection[1].AverageTalkTime.TotalSeconds;
			long ticksAverageBefore2 = monthOfYear.PeriodTypeCollection[1].AverageTalkTime.Ticks;

			monthOfYear.PeriodTypeCollection[1].AverageTalkTime = new TimeSpan(0, 0, 8);

			Assert.AreEqual(Math.Round((8 / secondsAverageBefore2) * indexBefore, 3), Math.Round(monthOfYear.PeriodTypeCollection[1].TalkTimeIndex, 3));

			////Set it back and the index should also set back
			monthOfYear.PeriodTypeCollection[1].AverageTalkTime = new TimeSpan(ticksAverageBefore2);
			Assert.AreEqual(Math.Round(indexBefore, 3), Math.Round(monthOfYear.PeriodTypeCollection[1].TalkTimeIndex, 3));
		}

		[Test]
		public void VerifyAverageAfterWorkTimeAndAfterWorkTimeIndexProperties()
		{
			TaskOwnerHelper periodForMonth = SkillDayFactory.GenerateStatistics(_dateInMonth, _workload);
			var monthPeriod = new TaskOwnerPeriod(_dateInMonth, periodForMonth.TaskOwnerDays, TaskOwnerPeriodType.Other);
			MonthOfYear monthOfYear = new MonthOfYear(monthPeriod, new MonthOfYearCreator());

			double averageBefore = monthOfYear.PeriodTypeCollection[1].AverageAfterWorkTime.TotalSeconds;
			double indexBefore = monthOfYear.PeriodTypeCollection[1].AfterTalkTimeIndex;

			//Should be 1.5 larger after aftertalktimeindex is set
			monthOfYear.PeriodTypeCollection[1].AfterTalkTimeIndex = 1.5;
			areEqualWithDelta(Math.Round((averageBefore * 1.5) / indexBefore, 3), Math.Round(monthOfYear.PeriodTypeCollection[1].AverageAfterWorkTime.TotalSeconds, 3));

			monthOfYear.PeriodTypeCollection[1].AfterTalkTimeIndex = indexBefore; //Should now go back to original
			areEqualWithDelta(Math.Round(averageBefore, 3), Math.Round(monthOfYear.PeriodTypeCollection[1].AverageAfterWorkTime.TotalSeconds, 3));

			indexBefore = monthOfYear.PeriodTypeCollection[1].AfterTalkTimeIndex;
			double secondsAverageBefore2 = monthOfYear.PeriodTypeCollection[1].AverageAfterWorkTime.TotalSeconds;
			long ticksAverageBefore2 = monthOfYear.PeriodTypeCollection[1].AverageAfterWorkTime.Ticks;

			monthOfYear.PeriodTypeCollection[1].AverageAfterWorkTime = new TimeSpan(0, 0, 8);

			areEqualWithDelta(Math.Round((8 / secondsAverageBefore2) * indexBefore, 3), Math.Round(monthOfYear.PeriodTypeCollection[1].AfterTalkTimeIndex, 3));

			////Set it back and the index should also set back
			monthOfYear.PeriodTypeCollection[1].AverageAfterWorkTime = new TimeSpan(ticksAverageBefore2);
			areEqualWithDelta(indexBefore, monthOfYear.PeriodTypeCollection[1].AfterTalkTimeIndex);

			//Also give it a below zero value
			indexBefore = monthOfYear.PeriodTypeCollection[1].AfterTalkTimeIndex;
			averageBefore = monthOfYear.PeriodTypeCollection[1].AverageAfterWorkTime.TotalSeconds;
			monthOfYear.PeriodTypeCollection[1].AfterTalkTimeIndex = 0.5;
			areEqualWithDelta(Math.Round((averageBefore * 0.5) / indexBefore, 3), Math.Round(monthOfYear.PeriodTypeCollection[1].AverageAfterWorkTime.TotalSeconds, 3));
		}
		#endregion

		#region Week

		[Test]
		public void VerifyIndexOfWeeks()
		{
			TaskOwnerHelper periodForMonth = SkillDayFactory.GenerateStatistics(_dateInMonth, _workload);
			var monthPeriod = new TaskOwnerPeriod(_dateInMonth, periodForMonth.TaskOwnerDays, TaskOwnerPeriodType.Other);
			MonthOfYear monthOfYear = new MonthOfYear(monthPeriod, new MonthOfYearCreator());

			TaskOwnerHelper periodForWeek = SkillDayFactory.GenerateStatisticsForWeekTests(new DateTime(2008, 2, 29, 0, 0, 0, DateTimeKind.Utc), _workload);
			var weekPeriod = new TaskOwnerPeriod(_dateInMonth, periodForWeek.TaskOwnerDays, TaskOwnerPeriodType.Other);
			WeekOfMonth weekOfMonth = new WeekOfMonth(weekPeriod, new WeekOfMonthCreator());

			IDictionary<int, double> resultsCalls = new Dictionary<int, double>();
			IDictionary<int, double> resultsTalkTime = new Dictionary<int, double>();
			IDictionary<int, double> resultsAfterTalkTime = new Dictionary<int, double>();

			resultsCalls.Add(1, 0.901639344);
			resultsCalls.Add(2, 0.901639344);
			resultsCalls.Add(3, 1.06557377);
			resultsCalls.Add(4, 1.06557377);
			resultsCalls.Add(5, 1.06557377);

			resultsTalkTime.Add(1, 0.96554809813242659);
			resultsTalkTime.Add(2, 0.96554809813242659);
			resultsTalkTime.Add(3, 1.0283944225960715);
			resultsTalkTime.Add(4, 1.0283944225960715);
			resultsTalkTime.Add(5, 1.0283944225960715);

			resultsAfterTalkTime.Add(1, 1.0513333337472268);
			resultsAfterTalkTime.Add(2, 1.0513333337472268);
			resultsAfterTalkTime.Add(3, 0.957692307041077);
			resultsAfterTalkTime.Add(4, 0.957692307041077);
			resultsAfterTalkTime.Add(5, 0.957692307041077);

			foreach (KeyValuePair<int, double> result in resultsCalls)
			{
				Assert.AreEqual(Math.Round(result.Value, 4), Math.Round(weekOfMonth.PeriodTypeCollection[result.Key].TaskIndex, 4));
			}
			foreach (KeyValuePair<int, double> result in resultsTalkTime)
			{
				Assert.AreEqual(Math.Round(result.Value, 4), Math.Round(monthOfYear.PeriodTypeCollection[result.Key].TalkTimeIndex, 4));
			}
			foreach (KeyValuePair<int, double> result in resultsAfterTalkTime)
			{
				Assert.AreEqual(Math.Round(result.Value, 4), Math.Round(monthOfYear.PeriodTypeCollection[result.Key].AfterTalkTimeIndex, 4));
			}
		}

		[Test]
		public void VerifyCanGetTaskIndexByDateForWeek()
		{
			TaskOwnerHelper periodForWeek = SkillDayFactory.GenerateStatisticsForWeekTests(new DateTime(2008, 2, 29, 0, 0, 0, DateTimeKind.Utc), _workload);
			var weekPeriod = new TaskOwnerPeriod(_dateInMonth, periodForWeek.TaskOwnerDays, TaskOwnerPeriodType.Other);
			WeekOfMonth weekOfMonth = new WeekOfMonth(weekPeriod, new WeekOfMonthCreator());

			double taskIndexWeek1 = weekOfMonth.PeriodTypeCollection[1].TaskIndex;
			double taskIndexWeek2 = weekOfMonth.PeriodTypeCollection[2].TaskIndex;
			double taskIndexWeek5 = weekOfMonth.PeriodTypeCollection[5].TaskIndex;

			var dateTime = new DateOnly(2006, 1, 1); //expect UTC 
			Assert.AreEqual(Math.Round(taskIndexWeek1, 4), Math.Round(weekOfMonth.TaskIndex(dateTime), 4));
			Assert.AreEqual(0.90163934426229508d, weekOfMonth.TaskIndex(dateTime));

			var dateTimeWeek2 = new DateOnly(2006, 6, 9); //expect UTC 
			Assert.AreEqual(Math.Round(taskIndexWeek2, 4), Math.Round(weekOfMonth.TaskIndex(dateTimeWeek2), 4));
			Assert.AreEqual(0.90163934426229508d, weekOfMonth.TaskIndex(dateTimeWeek2));

			var dateTimeWeek5 = new DateOnly(2006, 10, 30); //expect UTC 
			Assert.AreEqual(Math.Round(taskIndexWeek5, 4), Math.Round(weekOfMonth.TaskIndex(dateTimeWeek5), 4));
			Assert.AreEqual(1.0655737704918034d, weekOfMonth.TaskIndex(dateTimeWeek5));
		}

		[Test]
		public void VerifyCanGetTalkTimeIndexByDateForWeek()
		{
			TaskOwnerHelper periodForWeek = SkillDayFactory.GenerateStatisticsForWeekTests(new DateTime(2008, 2, 29, 0, 0, 0, DateTimeKind.Utc), _workload);
			var weekPeriod = new TaskOwnerPeriod(_dateInMonth, periodForWeek.TaskOwnerDays, TaskOwnerPeriodType.Other);
			WeekOfMonth weekOfMonth = new WeekOfMonth(weekPeriod, new WeekOfMonthCreator());

			double talkTimeIndexWeek1 = weekOfMonth.PeriodTypeCollection[1].TalkTimeIndex;
			double talkTimeIndexWeek2 = weekOfMonth.PeriodTypeCollection[2].TalkTimeIndex;
			double talkTimeIndexWeek5 = weekOfMonth.PeriodTypeCollection[5].TalkTimeIndex;

			var dateTimeWeek1 = new DateOnly(2006, 1, 1); //expect UTC 
			Assert.AreEqual(Math.Round(talkTimeIndexWeek1, 4), Math.Round(weekOfMonth.TaskTimeIndex(dateTimeWeek1), 4));
			Assert.AreEqual(1.0517241402883473d, weekOfMonth.TaskTimeIndex(dateTimeWeek1));

			var dateTimeWeek2 = new DateOnly(2006, 6, 9); //expect UTC 
			Assert.AreEqual(Math.Round(talkTimeIndexWeek2, 4), Math.Round(weekOfMonth.TaskTimeIndex(dateTimeWeek2), 4));
			Assert.AreEqual(1.0517241402883473d, weekOfMonth.TaskTimeIndex(dateTimeWeek2));

			var dateTimeWeek5 = new DateOnly(2006, 10, 30); //expect UTC 
			Assert.AreEqual(Math.Round(talkTimeIndexWeek5, 4), Math.Round(weekOfMonth.TaskTimeIndex(dateTimeWeek5), 4));
			Assert.AreEqual(0.97082228010701543d, weekOfMonth.TaskTimeIndex(dateTimeWeek5));
		}

		[Test]
		public void VerifyCanGetAfterTalkTimeIndexByDateForWeek()
		{
			TaskOwnerHelper periodForWeek = SkillDayFactory.GenerateStatisticsForWeekTests(new DateTime(2008, 2, 29, 0, 0, 0, DateTimeKind.Utc), _workload);
			var weekPeriod = new TaskOwnerPeriod(_dateInMonth, periodForWeek.TaskOwnerDays, TaskOwnerPeriodType.Other);
			WeekOfMonth weekOfMonth = new WeekOfMonth(weekPeriod, new WeekOfMonthCreator());

			double afterTalkTimeIndexWeek1 = weekOfMonth.PeriodTypeCollection[1].AfterTalkTimeIndex;
			double afterTalkTimeIndexWeek2 = weekOfMonth.PeriodTypeCollection[2].AfterTalkTimeIndex;
			double afterTalkTimeIndexWeek5 = weekOfMonth.PeriodTypeCollection[5].AfterTalkTimeIndex;

			var dateTimeWeek1 = new DateOnly(2006, 1, 1); //expect UTC 
			Assert.AreEqual(Math.Round(afterTalkTimeIndexWeek1, 4), Math.Round(weekOfMonth.AfterTaskTimeIndex(dateTimeWeek1), 4));
			Assert.AreEqual(0.91044776263443983d, weekOfMonth.AfterTaskTimeIndex(dateTimeWeek1));

			var dateTimeWeek2 = new DateOnly(2006, 6, 9); //expect UTC 
			Assert.AreEqual(Math.Round(afterTalkTimeIndexWeek2, 4), Math.Round(weekOfMonth.AfterTaskTimeIndex(dateTimeWeek2), 4));
			Assert.AreEqual(0.91044776263443983d, weekOfMonth.AfterTaskTimeIndex(dateTimeWeek2));

			var dateTimeWeek5 = new DateOnly(2006, 10, 30); //expect UTC 
			Assert.AreEqual(Math.Round(afterTalkTimeIndexWeek5, 4), Math.Round(weekOfMonth.AfterTaskTimeIndex(dateTimeWeek5), 4));
			Assert.AreEqual(1.0505166475127576d, weekOfMonth.AfterTaskTimeIndex(dateTimeWeek5));
		}

		[Test]
		public void VerifyIndexOfWeeksWhenIncompleteSelection()
		{
			TaskOwnerHelper periodForIncompleteWOM = SkillDayFactory.GenerateStatisticsForDayTests(new DateTime(2008, 2, 29, 0, 0, 0, DateTimeKind.Utc), _workload);
			var weekPeriod = new TaskOwnerPeriod(_dateInMonth, periodForIncompleteWOM.TaskOwnerDays, TaskOwnerPeriodType.Other);
			WeekOfMonth weekOfMonthIncomplete = new WeekOfMonth(weekPeriod, new WeekOfMonthCreator());

			IDictionary<int, double> resultsCalls = new Dictionary<int, double>();
			IDictionary<int, double> resultsTalkTime = new Dictionary<int, double>();
			IDictionary<int, double> resultsAfterTalkTime = new Dictionary<int, double>();

			resultsCalls.Add(1, 0.901639344);//Not relevant
			resultsCalls.Add(2, 0.901639344);//Not relevant
			resultsCalls.Add(3, 1);
			resultsCalls.Add(4, 1);
			resultsCalls.Add(5, 1);

			resultsTalkTime.Add(1, 0.964890281728894);//Not relevant
			resultsTalkTime.Add(2, 0.964890281728894);//Not relevant
			resultsTalkTime.Add(3, 1);
			resultsTalkTime.Add(4, 1);
			resultsTalkTime.Add(5, 1);

			resultsAfterTalkTime.Add(1, 1.05194805239889);//Not relevant
			resultsAfterTalkTime.Add(2, 1.05194805239889);//Not relevant
			resultsAfterTalkTime.Add(3, 1);
			resultsAfterTalkTime.Add(4, 1);
			resultsAfterTalkTime.Add(5, 1);

			foreach (KeyValuePair<int, double> result in resultsCalls)
			{
				if (result.Key > 2)
					Assert.AreEqual(Math.Round(result.Value, 4),
									Math.Round(weekOfMonthIncomplete.PeriodTypeCollection[result.Key].TaskIndex, 4));
			}
			foreach (KeyValuePair<int, double> result in resultsTalkTime)
			{
				if (result.Key > 2)
					Assert.AreEqual(Math.Round(result.Value, 4),
									Math.Round(weekOfMonthIncomplete.PeriodTypeCollection[result.Key].TalkTimeIndex, 4));
			}
			foreach (KeyValuePair<int, double> result in resultsAfterTalkTime)
			{
				if (result.Key > 2)
					Assert.AreEqual(Math.Round(result.Value, 4),
									Math.Round(weekOfMonthIncomplete.PeriodTypeCollection[result.Key].AfterTalkTimeIndex, 4));
			}
		}

		[Test]
		public void VerifyAverageCallsForWeek()
		{
			TaskOwnerHelper periodForWeek = SkillDayFactory.GenerateStatisticsForWeekTests(new DateTime(2008, 2, 29, 0, 0, 0, DateTimeKind.Utc), _workload);
			var weekPeriod = new TaskOwnerPeriod(_dateInMonth, periodForWeek.TaskOwnerDays, TaskOwnerPeriodType.Other);
			WeekOfMonth weekOfMonth = new WeekOfMonth(weekPeriod, new WeekOfMonthCreator());

			IDictionary<int, double> resultsAverageTasks = new Dictionary<int, double>();

			resultsAverageTasks.Add(1, 770);
			resultsAverageTasks.Add(2, 770);
			resultsAverageTasks.Add(3, 910);
			resultsAverageTasks.Add(4, 910);
			resultsAverageTasks.Add(5, 910);

			foreach (KeyValuePair<int, double> result in resultsAverageTasks)
			{
				Assert.AreEqual(Math.Round(result.Value, 4), Math.Round(weekOfMonth.PeriodTypeCollection[result.Key].AverageTasks, 4));
			}
		}

		[Test]
		public void VerifyAverageCallsForWeekPacific()
		{
			_skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
			TaskOwnerHelper periodForWeek =
				SkillDayFactory.GenerateStatisticsForDayTests(
					TimeZoneInfo.ConvertTimeToUtc(new DateTime(2001, 09, 01), _skill.TimeZone), _workload);
			var weekPeriod = new TaskOwnerPeriod(_dateInMonth, periodForWeek.TaskOwnerDays, TaskOwnerPeriodType.Other);
			WeekOfMonth weekOfMonth = new WeekOfMonth(weekPeriod, new WeekOfMonthCreator());

			IDictionary<int, double> resultsAverageTasks = new Dictionary<int, double>();
			foreach (KeyValuePair<int, double> result in resultsAverageTasks)
			{
				Assert.AreEqual(Math.Round(result.Value, 4), Math.Round(weekOfMonth.PeriodTypeCollection[result.Key].AverageTasks, 4));
			}
		}

		[Test]
		public void VerifyAverageTaskAndTaskIndexPropertiesForWeek()
		{
			TaskOwnerHelper periodForWeek = SkillDayFactory.GenerateStatisticsForWeekTests(new DateTime(2008, 2, 29, 0, 0, 0, DateTimeKind.Utc), _workload);
			var weekPeriod = new TaskOwnerPeriod(_dateInMonth, periodForWeek.TaskOwnerDays, TaskOwnerPeriodType.Other);
			WeekOfMonth weekOfMonth = new WeekOfMonth(weekPeriod, new WeekOfMonthCreator());

			//Ok this start index starts from 1.08, I mean not 1
			double indexBefore = weekOfMonth.PeriodTypeCollection[1].TaskIndex;
			double averageBefore = weekOfMonth.PeriodTypeCollection[1].AverageTasks;

			//Should be 1.5 larger than the "real index 1 value" after talktimeindex is set
			weekOfMonth.PeriodTypeCollection[1].TaskIndex = 1.5;
			//Need to calculate from "index 1" base
			Assert.AreEqual((averageBefore * 1.5) / indexBefore, weekOfMonth.PeriodTypeCollection[1].AverageTasks);

			//Should now go back to original
			weekOfMonth.PeriodTypeCollection[1].TaskIndex = indexBefore;
			Assert.AreEqual(averageBefore, weekOfMonth.PeriodTypeCollection[1].AverageTasks);

			indexBefore = weekOfMonth.PeriodTypeCollection[1].TaskIndex;
			double averageBefore2 = weekOfMonth.PeriodTypeCollection[1].AverageTasks;
			weekOfMonth.PeriodTypeCollection[1].AverageTasks = 2325;

			Assert.AreEqual(Math.Round((2325 / averageBefore2) * indexBefore, 2), Math.Round(weekOfMonth.PeriodTypeCollection[1].TaskIndex, 2));

			//Set it back the index should also set back
			weekOfMonth.PeriodTypeCollection[1].AverageTasks = averageBefore2;
			Assert.AreEqual(indexBefore, weekOfMonth.PeriodTypeCollection[1].TaskIndex);
		}

		[Test]
		public void VerifyAverageTalkTimeAndTalkTimeIndexPropertiesForWeek()
		{
			TaskOwnerHelper periodForWeek = SkillDayFactory.GenerateStatisticsForWeekTests(new DateTime(2008, 2, 29, 0, 0, 0, DateTimeKind.Utc), _workload);
			var weekPeriod = new TaskOwnerPeriod(_dateInMonth, periodForWeek.TaskOwnerDays, TaskOwnerPeriodType.Other);
			WeekOfMonth weekOfMonth = new WeekOfMonth(weekPeriod, new WeekOfMonthCreator());

			double indexBefore = weekOfMonth.PeriodTypeCollection[1].TalkTimeIndex;
			double averageBefore = weekOfMonth.PeriodTypeCollection[1].AverageTalkTime.TotalSeconds;

			//Should be 1.5 larger than the "real original value" after talktimeindex is set
			weekOfMonth.PeriodTypeCollection[1].TalkTimeIndex = 1.5;
			Assert.AreEqual(Math.Round((averageBefore * 1.5) / indexBefore, 3), Math.Round(weekOfMonth.PeriodTypeCollection[1].AverageTalkTime.TotalSeconds, 3));


			weekOfMonth.PeriodTypeCollection[1].TalkTimeIndex = indexBefore; //Should now go back to original
			Assert.AreEqual(Math.Round(averageBefore, 3), Math.Round(weekOfMonth.PeriodTypeCollection[1].AverageTalkTime.TotalSeconds, 3));

			indexBefore = weekOfMonth.PeriodTypeCollection[1].TalkTimeIndex;
			double secondsAverageBefore2 = weekOfMonth.PeriodTypeCollection[1].AverageTalkTime.TotalSeconds;
			long ticksAverageBefore2 = weekOfMonth.PeriodTypeCollection[1].AverageTalkTime.Ticks;

			weekOfMonth.PeriodTypeCollection[1].AverageTalkTime = new TimeSpan(0, 0, 8);

			Assert.AreEqual(Math.Round((8 / secondsAverageBefore2) * indexBefore, 3), Math.Round(weekOfMonth.PeriodTypeCollection[1].TalkTimeIndex, 3));

			////Set it back and the index should also set back
			weekOfMonth.PeriodTypeCollection[1].AverageTalkTime = new TimeSpan(ticksAverageBefore2);
			Assert.AreEqual(Math.Round(indexBefore, 3), Math.Round(weekOfMonth.PeriodTypeCollection[1].TalkTimeIndex, 3));
		}

		[Test]
		public void VerifyAverageAfterWorkTimeAndAfterWorkTimeIndexPropertiesForWeek()
		{
			TaskOwnerHelper periodForWeek = SkillDayFactory.GenerateStatisticsForWeekTests(new DateTime(2008, 2, 29, 0, 0, 0, DateTimeKind.Utc), _workload);
			var weekPeriod = new TaskOwnerPeriod(_dateInMonth, periodForWeek.TaskOwnerDays, TaskOwnerPeriodType.Other);
			WeekOfMonth weekOfMonth = new WeekOfMonth(weekPeriod, new WeekOfMonthCreator());

			double averageBefore = weekOfMonth.PeriodTypeCollection[1].AverageAfterWorkTime.TotalSeconds;
			double indexBefore = weekOfMonth.PeriodTypeCollection[1].AfterTalkTimeIndex;

			//Should be 1.5 larger after aftertalktimeindex is set
			weekOfMonth.PeriodTypeCollection[1].AfterTalkTimeIndex = 1.5;
			areEqualWithDelta(Math.Round((averageBefore * 1.5) / indexBefore, 3), Math.Round(weekOfMonth.PeriodTypeCollection[1].AverageAfterWorkTime.TotalSeconds, 3));

			weekOfMonth.PeriodTypeCollection[1].AfterTalkTimeIndex = indexBefore; //Should now go back to original
			areEqualWithDelta(Math.Round(averageBefore, 3), Math.Round(weekOfMonth.PeriodTypeCollection[1].AverageAfterWorkTime.TotalSeconds, 3));

			indexBefore = weekOfMonth.PeriodTypeCollection[1].AfterTalkTimeIndex;
			double secondsAverageBefore2 = weekOfMonth.PeriodTypeCollection[1].AverageAfterWorkTime.TotalSeconds;
			long ticksAverageBefore2 = weekOfMonth.PeriodTypeCollection[1].AverageAfterWorkTime.Ticks;

			weekOfMonth.PeriodTypeCollection[1].AverageAfterWorkTime = new TimeSpan(0, 0, 8);

			areEqualWithDelta(Math.Round((8 / secondsAverageBefore2) * indexBefore, 3), Math.Round(weekOfMonth.PeriodTypeCollection[1].AfterTalkTimeIndex, 3));

			////Set it back and the index should also set back
			weekOfMonth.PeriodTypeCollection[1].AverageAfterWorkTime = new TimeSpan(ticksAverageBefore2);
			areEqualWithDelta(indexBefore, weekOfMonth.PeriodTypeCollection[1].AfterTalkTimeIndex);

			//Also give it a below zero value
			indexBefore = weekOfMonth.PeriodTypeCollection[1].AfterTalkTimeIndex;
			averageBefore = weekOfMonth.PeriodTypeCollection[1].AverageAfterWorkTime.TotalSeconds;
			weekOfMonth.PeriodTypeCollection[1].AfterTalkTimeIndex = 0.5;
			areEqualWithDelta(Math.Round((averageBefore * 0.5) / indexBefore, 3), Math.Round(weekOfMonth.PeriodTypeCollection[1].AverageAfterWorkTime.TotalSeconds, 3));
		}

		#endregion

		#region Day
		[Test]
		public void VerifyIndexOfDays()
		{
			TaskOwnerHelper periodForDays = SkillDayFactory.GenerateStatisticsForDayTests(new DateTime(2008, 2, 29, 0, 0, 0, DateTimeKind.Utc), _workload);
			var dayPeriod = new TaskOwnerPeriod(_dateInMonth, periodForDays.TaskOwnerDays, TaskOwnerPeriodType.Other);
			DayOfWeeks dayOfWeek = new DayOfWeeks(dayPeriod, new DaysOfWeekCreator());

			IDictionary<int, double> resultsCalls = new Dictionary<int, double>();

			resultsCalls.Add(1, 0.974683544);
			resultsCalls.Add(2, 0.974683544);
			resultsCalls.Add(3, 0.974683544);
			resultsCalls.Add(4, 0.974683544);
			resultsCalls.Add(5, 0.974683544);
			resultsCalls.Add(6, 0.974683544);
			resultsCalls.Add(7, 1.151898734);

			foreach (KeyValuePair<int, double> result in resultsCalls)
			{
				Assert.AreEqual(Math.Round(result.Value, 4),
								Math.Round(dayOfWeek.PeriodTypeCollection[result.Key].TaskIndex, 4));
			}
		}

		[Test]
		public void VerifyCanCreateDaysOfWeek()
		{
			IList<ITaskOwner> workloadDays = new List<ITaskOwner>();
			IList<IStatisticTask> statisticTasks = new List<IStatisticTask>();
			DateTime date = new DateTime(2008, 12, 18, 0, 0, 0, 0, DateTimeKind.Utc);

			IWorkloadDay workloadDay = WorkloadDayFactory.GetWorkloadDaysForTest(date, date, _workload)[0];
			workloadDays.Add(workloadDay);
			TaskOwnerHelper period = new TaskOwnerHelper(workloadDays);
			period.BeginUpdate();
			new Statistic(_workload).Match(workloadDays.OfType<IWorkloadDayBase>(), statisticTasks);
			period.EndUpdate();

			var dayPeriod = new TaskOwnerPeriod(_dateInMonth, period.TaskOwnerDays, TaskOwnerPeriodType.Other);
			DayOfWeeks dayOfWeek = new DayOfWeeks(dayPeriod, new DaysOfWeekCreator());
			Assert.IsNotNull(dayOfWeek);
		}

		[Test]
		public void CanReloadHistoricalDataDepthDay()
		{
			TaskOwnerHelper periodForDays = SkillDayFactory.GenerateStatisticsForDayTests(new DateTime(2008, 2, 29, 0, 0, 0, DateTimeKind.Utc), _workload);
			var dayPeriod = new TaskOwnerPeriod(_dateInMonth, periodForDays.TaskOwnerDays, TaskOwnerPeriodType.Other);
			DayOfWeeks dayOfWeek = new DayOfWeeks(dayPeriod, new DaysOfWeekCreator());

			TaskOwnerHelper periodForMonth = SkillDayFactory.GenerateStatistics(_dateInMonth, _workload);

			dayOfWeek.ReloadHistoricalDataDepth(new TaskOwnerPeriod(_dateInMonth, new List<ITaskOwner>(), TaskOwnerPeriodType.Other));
			Assert.AreEqual(0, dayOfWeek.PeriodTypeCollection.Count);
			dayOfWeek.ReloadHistoricalDataDepth(new TaskOwnerPeriod(_dateInMonth, periodForMonth.TaskOwnerDays, TaskOwnerPeriodType.Other));
			Assert.IsTrue(dayOfWeek.PeriodTypeCollection.Count > 0);
		}

		[Test]
		public void VerifyAverageCallsForDay()
		{
			TaskOwnerHelper periodForDays = SkillDayFactory.GenerateStatisticsForDayTests(new DateTime(2008, 2, 29, 0, 0, 0, DateTimeKind.Utc), _workload);
			var dayPeriod = new TaskOwnerPeriod(_dateInMonth, periodForDays.TaskOwnerDays, TaskOwnerPeriodType.Other);
			DayOfWeeks dayOfWeek = new DayOfWeeks(dayPeriod, new DaysOfWeekCreator());

			IDictionary<int, double> resultsAverageTasks = new Dictionary<int, double>();

			resultsAverageTasks.Add(1, 110);
			resultsAverageTasks.Add(2, 110);
			resultsAverageTasks.Add(3, 110);
			resultsAverageTasks.Add(4, 110);
			resultsAverageTasks.Add(5, 110);
			resultsAverageTasks.Add(6, 110);
			resultsAverageTasks.Add(7, 130);//Monday

			foreach (KeyValuePair<int, double> result in resultsAverageTasks)
			{
				Assert.AreEqual(Math.Round(result.Value, 4), Math.Round(dayOfWeek.PeriodTypeCollection[result.Key].AverageTasks, 4));
			}
		}

		[Test]
		public void VerifyCanGetTaskIndexByDateForDay()
		{
			TaskOwnerHelper periodForDays = SkillDayFactory.GenerateStatisticsForDayTests(new DateTime(2008, 2, 29, 0, 0, 0, DateTimeKind.Utc), _workload);
			var dayPeriod = new TaskOwnerPeriod(_dateInMonth, periodForDays.TaskOwnerDays, TaskOwnerPeriodType.Other);
			DayOfWeeks dayOfWeek = new DayOfWeeks(dayPeriod, new DaysOfWeekCreator());

			double taskIndexMonday = dayOfWeek.PeriodTypeCollection[2].TaskIndex;
			double taskIndexTuesday = dayOfWeek.PeriodTypeCollection[3].TaskIndex;
			double taskIndexSunday = dayOfWeek.PeriodTypeCollection[1].TaskIndex;

			var dateTimeMonday = new DateOnly(2008, 3, 10); //expect UTC 
			Assert.AreEqual(Math.Round(taskIndexMonday, 4), Math.Round(dayOfWeek.TaskIndex(dateTimeMonday), 4));
			Assert.AreEqual(0.97468354430379744d, dayOfWeek.TaskIndex(dateTimeMonday));

			var dateTimeTuesday = new DateOnly(2008, 3, 11); //expect UTC 
			Assert.AreEqual(Math.Round(taskIndexTuesday, 4), Math.Round(dayOfWeek.TaskIndex(dateTimeTuesday), 4));
			Assert.AreEqual(0.97468354430379744d, dayOfWeek.TaskIndex(dateTimeTuesday));

			var dateTimeSunday = new DateOnly(2008, 1, 27); //expect UTC 
			Assert.AreEqual(Math.Round(taskIndexSunday, 4), Math.Round(dayOfWeek.TaskIndex(dateTimeSunday), 4));
			Assert.AreEqual(0.97468354430379744d, dayOfWeek.TaskIndex(dateTimeSunday));
		}

		[Test]
		public void VerifyCanGetTalkTimeIndexByDateForDay()
		{
			TaskOwnerHelper periodForDays = SkillDayFactory.GenerateStatisticsForDayTests(new DateTime(2008, 2, 29, 0, 0, 0, DateTimeKind.Utc), _workload);
			var dayPeriod = new TaskOwnerPeriod(_dateInMonth, periodForDays.TaskOwnerDays, TaskOwnerPeriodType.Other);
			DayOfWeeks dayOfWeek = new DayOfWeeks(dayPeriod, new DaysOfWeekCreator());

			double talkTimeIndexMonday = dayOfWeek.PeriodTypeCollection[2].TalkTimeIndex;
			double talkTimeIndexTuesday = dayOfWeek.PeriodTypeCollection[3].TalkTimeIndex;
			double talkTimeIndexSunday = dayOfWeek.PeriodTypeCollection[1].TalkTimeIndex;

			var dateTimeMonday = new DateOnly(2008, 3, 10); //expect UTC 
			Assert.AreEqual(Math.Round(talkTimeIndexMonday, 4), Math.Round(dayOfWeek.TaskTimeIndex(dateTimeMonday), 4));
			Assert.AreEqual(1.0128205150279421, dayOfWeek.TaskTimeIndex(dateTimeMonday));

			var dateTimeTuesday = new DateOnly(2008, 3, 11); //expect UTC 
			Assert.AreEqual(Math.Round(talkTimeIndexTuesday, 4), Math.Round(dayOfWeek.TaskTimeIndex(dateTimeTuesday), 4));
			Assert.AreEqual(1.0128205150279421, dayOfWeek.TaskTimeIndex(dateTimeTuesday));

			var dateTimeSunday = new DateOnly(2008, 1, 27); //expect UTC 
			Assert.AreEqual(Math.Round(talkTimeIndexSunday, 4), Math.Round(dayOfWeek.TaskTimeIndex(dateTimeSunday), 4));
			Assert.AreEqual(1.0128205150279421, dayOfWeek.TaskTimeIndex(dateTimeSunday));
		}

		[Test]
		public void VerifyCanGetAfterTalkTimeIndexByDateForDay()
		{
			TaskOwnerHelper periodForDays = SkillDayFactory.GenerateStatisticsForDayTests(new DateTime(2008, 2, 29, 0, 0, 0, DateTimeKind.Utc), _workload);
			var dayPeriod = new TaskOwnerPeriod(_dateInMonth, periodForDays.TaskOwnerDays, TaskOwnerPeriodType.Other);
			DayOfWeeks dayOfWeek = new DayOfWeeks(dayPeriod, new DaysOfWeekCreator());

			double afterTalkTimeIndexMonday = dayOfWeek.PeriodTypeCollection[2].AfterTalkTimeIndex;
			double afterTalkTimeIndexTuesday = dayOfWeek.PeriodTypeCollection[3].AfterTalkTimeIndex;
			double afterTalkTimeIndexSunday = dayOfWeek.PeriodTypeCollection[1].AfterTalkTimeIndex;

			var dateTimeMonday = new DateOnly(2008, 3, 10); //expect UTC 
			Assert.AreEqual(Math.Round(afterTalkTimeIndexMonday, 4), Math.Round(dayOfWeek.AfterTaskTimeIndex(dateTimeMonday), 4));
			Assert.AreEqual(0.97530864358878222d, dayOfWeek.AfterTaskTimeIndex(dateTimeMonday));

			var dateTimeTuesday = new DateOnly(2008, 3, 11); //expect UTC 
			Assert.AreEqual(Math.Round(afterTalkTimeIndexTuesday, 4), Math.Round(dayOfWeek.AfterTaskTimeIndex(dateTimeTuesday), 4));
			Assert.AreEqual(0.97530864358878222d, dayOfWeek.AfterTaskTimeIndex(dateTimeTuesday));

			var dateTimeSunday = new DateOnly(2008, 1, 27); //expect UTC 
			Assert.AreEqual(Math.Round(afterTalkTimeIndexSunday, 4), Math.Round(dayOfWeek.AfterTaskTimeIndex(dateTimeSunday), 4));
			Assert.AreEqual(0.97530864358878222d, dayOfWeek.AfterTaskTimeIndex(dateTimeSunday));
		}


		[Test]
		public void VerifyAverageTaskAndTaskIndexPropertiesForDay()
		{
			TaskOwnerHelper periodForDays = SkillDayFactory.GenerateStatisticsForDayTests(new DateTime(2008, 2, 29, 0, 0, 0, DateTimeKind.Utc), _workload);
			var dayPeriod = new TaskOwnerPeriod(_dateInMonth, periodForDays.TaskOwnerDays, TaskOwnerPeriodType.Other);
			DayOfWeeks dayOfWeek = new DayOfWeeks(dayPeriod, new DaysOfWeekCreator());

			//Ok this start index starts from 1.08, I mean not 1
			double indexBefore = dayOfWeek.PeriodTypeCollection[1].TaskIndex;
			double averageBefore = dayOfWeek.PeriodTypeCollection[1].AverageTasks;

			//Should be 1.5 larger than the "real index 1 value" after talktimeindex is set
			dayOfWeek.PeriodTypeCollection[1].TaskIndex = 1.5;
			//Need to calculate from "index 1" base
			Assert.AreEqual((averageBefore * 1.5) / indexBefore, dayOfWeek.PeriodTypeCollection[1].AverageTasks);

			//Should now go back to original
			dayOfWeek.PeriodTypeCollection[1].TaskIndex = indexBefore;
			Assert.AreEqual(averageBefore, dayOfWeek.PeriodTypeCollection[1].AverageTasks);

			indexBefore = dayOfWeek.PeriodTypeCollection[1].TaskIndex;
			double averageBefore2 = dayOfWeek.PeriodTypeCollection[1].AverageTasks;
			dayOfWeek.PeriodTypeCollection[1].AverageTasks = 2325;

			Assert.AreEqual(Math.Round((2325 / averageBefore2) * indexBefore, 2), Math.Round(dayOfWeek.PeriodTypeCollection[1].TaskIndex, 2));

			//Set it back the index should also set back
			dayOfWeek.PeriodTypeCollection[1].AverageTasks = averageBefore2;
			Assert.AreEqual(indexBefore, dayOfWeek.PeriodTypeCollection[1].TaskIndex);
		}

		[Test]
		public void VerifyAverageTalkTimeAndTalkTimeIndexPropertiesForDay()
		{
			TaskOwnerHelper periodForDays = SkillDayFactory.GenerateStatisticsForDayTests(new DateTime(2008, 2, 29, 0, 0, 0, DateTimeKind.Utc), _workload);
			var dayPeriod = new TaskOwnerPeriod(_dateInMonth, periodForDays.TaskOwnerDays, TaskOwnerPeriodType.Other);
			DayOfWeeks dayOfWeek = new DayOfWeeks(dayPeriod, new DaysOfWeekCreator());

			double indexBefore = dayOfWeek.PeriodTypeCollection[1].TalkTimeIndex;
			double averageBefore = dayOfWeek.PeriodTypeCollection[1].AverageTalkTime.TotalSeconds;

			//Should be 1.5 larger than the "real original value" after talktimeindex is set
			dayOfWeek.PeriodTypeCollection[1].TalkTimeIndex = 1.5;
			Assert.AreEqual(Math.Round((averageBefore * 1.5) / indexBefore, 3), Math.Round(dayOfWeek.PeriodTypeCollection[1].AverageTalkTime.TotalSeconds, 3));


			dayOfWeek.PeriodTypeCollection[1].TalkTimeIndex = indexBefore; //Should now go back to original
			Assert.AreEqual(Math.Round(averageBefore, 3), Math.Round(dayOfWeek.PeriodTypeCollection[1].AverageTalkTime.TotalSeconds, 3));

			indexBefore = dayOfWeek.PeriodTypeCollection[1].TalkTimeIndex;
			double secondsAverageBefore2 = dayOfWeek.PeriodTypeCollection[1].AverageTalkTime.TotalSeconds;
			long ticksAverageBefore2 = dayOfWeek.PeriodTypeCollection[1].AverageTalkTime.Ticks;

			dayOfWeek.PeriodTypeCollection[1].AverageTalkTime = new TimeSpan(0, 0, 8);

			Assert.AreEqual(Math.Round((8 / secondsAverageBefore2) * indexBefore, 3), Math.Round(dayOfWeek.PeriodTypeCollection[1].TalkTimeIndex, 3));

			////Set it back and the index should also set back
			dayOfWeek.PeriodTypeCollection[1].AverageTalkTime = new TimeSpan(ticksAverageBefore2);
			Assert.AreEqual(Math.Round(indexBefore, 3), Math.Round(dayOfWeek.PeriodTypeCollection[1].TalkTimeIndex, 3));
		}

		[Test]
		public void VerifyAverageAfterWorkTimeAndAfterWorkTimeIndexPropertiesForDay()
		{
			TaskOwnerHelper periodForDays = SkillDayFactory.GenerateStatisticsForDayTests(new DateTime(2008, 2, 29, 0, 0, 0, DateTimeKind.Utc), _workload);
			var dayPeriod = new TaskOwnerPeriod(_dateInMonth, periodForDays.TaskOwnerDays, TaskOwnerPeriodType.Other);
			DayOfWeeks dayOfWeek = new DayOfWeeks(dayPeriod, new DaysOfWeekCreator());

			double averageBefore = dayOfWeek.PeriodTypeCollection[1].AverageAfterWorkTime.TotalSeconds;
			double indexBefore = dayOfWeek.PeriodTypeCollection[1].AfterTalkTimeIndex;

			//Should be 1.5 larger after aftertalktimeindex is set
			dayOfWeek.PeriodTypeCollection[1].AfterTalkTimeIndex = 1.5;
			Assert.AreEqual(Math.Round((averageBefore * 1.5) / indexBefore, 3), Math.Round(dayOfWeek.PeriodTypeCollection[1].AverageAfterWorkTime.TotalSeconds, 3));

			dayOfWeek.PeriodTypeCollection[1].AfterTalkTimeIndex = indexBefore; //Should now go back to original
			Assert.AreEqual(Math.Round(averageBefore, 3), Math.Round(dayOfWeek.PeriodTypeCollection[1].AverageAfterWorkTime.TotalSeconds, 3));

			indexBefore = dayOfWeek.PeriodTypeCollection[1].AfterTalkTimeIndex;
			double secondsAverageBefore2 = dayOfWeek.PeriodTypeCollection[1].AverageAfterWorkTime.TotalSeconds;
			long ticksAverageBefore2 = dayOfWeek.PeriodTypeCollection[1].AverageAfterWorkTime.Ticks;

			dayOfWeek.PeriodTypeCollection[1].AverageAfterWorkTime = new TimeSpan(0, 0, 8);

			Assert.AreEqual(Math.Round((8 / secondsAverageBefore2) * indexBefore, 3), Math.Round(dayOfWeek.PeriodTypeCollection[1].AfterTalkTimeIndex, 3));

			////Set it back and the index should also set back
			dayOfWeek.PeriodTypeCollection[1].AverageAfterWorkTime = new TimeSpan(ticksAverageBefore2);
			Assert.AreEqual(Math.Round(indexBefore, 6), Math.Round(dayOfWeek.PeriodTypeCollection[1].AfterTalkTimeIndex, 6));

			//Also give it a below zero value
			indexBefore = dayOfWeek.PeriodTypeCollection[1].AfterTalkTimeIndex;
			averageBefore = dayOfWeek.PeriodTypeCollection[1].AverageAfterWorkTime.TotalSeconds;
			dayOfWeek.PeriodTypeCollection[1].AfterTalkTimeIndex = 0.5;
			Assert.AreEqual(Math.Round((averageBefore * 0.5) / indexBefore, 3), Math.Round(dayOfWeek.PeriodTypeCollection[1].AverageAfterWorkTime.TotalSeconds, 3));
		}

		#endregion
	}
}
