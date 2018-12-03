using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class TotalVolumeTest
    {
        private TotalVolume target;
        private readonly IList<ITaskOwner> _taskOwnerCollection = new List<ITaskOwner>();
        private double _averageTasks;
        private const double _dayTrendFactor = 1.002;
        private const bool _useTrend = true;
        private IWorkload workload;
        private TaskOwnerPeriod historicalDepth;
        private readonly TimeZoneInfo _timeZone = (TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"));
        private MockRepository mocks;
        private IList<IVolumeYear> volumes;

        [SetUp]
        public void Setup()
        {
            ISkill skill = SkillFactory.CreateSkill("testSkill");
            skill.TimeZone = _timeZone;
            workload = WorkloadFactory.CreateWorkload(skill);
            mocks = new MockRepository();

            var date = new DateOnly(2008, 3, 31);
            var dateTime = new DateOnly(2006, 1, 1);

            TaskOwnerHelper periodForHelper = SkillDayFactory.GenerateMockedStatistics(dateTime, workload);

            _taskOwnerCollection.Clear();

            var openHourPeriods = new List<TimePeriod> {new TimePeriod(8, 0, 8, 15)};

            for (int i = 0; i <=6; i++)
            {
                var anotherDayTemplate = (IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, (DayOfWeek) i);
                anotherDayTemplate.ChangeOpenHours(openHourPeriods);
                workload.SetTemplateAt(i, anotherDayTemplate);
            }
            

            for (int i = 0; i < 15; i++)
            {
                IWorkloadDay workloadDay = new WorkloadDay();

                workloadDay.Create(date, workload, openHourPeriods);

                workloadDay.Tasks = 200;
                workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(20);
                workloadDay.AverageTaskTime = TimeSpan.FromSeconds(120);
                workloadDay.Initialize();

                _taskOwnerCollection.Add(workloadDay);
                date = workloadDay.CurrentDate.AddDays(1);
                
            }



            IVolumeYear monthOfYear = mocks.StrictMock<IVolumeYear>();
            IVolumeYear weekOfMonth = mocks.StrictMock<IVolumeYear>();
            IVolumeYear dayOfWeek = mocks.StrictMock<IVolumeYear>();

            Expect.Call(monthOfYear.TaskIndex(date)).IgnoreArguments().Repeat.AtLeastOnce().Return(1d);
            Expect.Call(monthOfYear.AfterTaskTimeIndex(date)).IgnoreArguments().Repeat.AtLeastOnce().Return(1d);
            Expect.Call(monthOfYear.TaskTimeIndex(date)).IgnoreArguments().Repeat.AtLeastOnce().Return(1d);
            Expect.Call(weekOfMonth.TaskIndex(date)).IgnoreArguments().Repeat.AtLeastOnce().Return(1.1d);
            Expect.Call(weekOfMonth.AfterTaskTimeIndex(date)).IgnoreArguments().Repeat.AtLeastOnce().Return(1.1d);
            Expect.Call(weekOfMonth.TaskTimeIndex(date)).IgnoreArguments().Repeat.AtLeastOnce().Return(1.1d);
            Expect.Call(dayOfWeek.TaskIndex(date)).IgnoreArguments().Repeat.AtLeastOnce().Return(1.2d);
            Expect.Call(dayOfWeek.AfterTaskTimeIndex(date)).IgnoreArguments().Repeat.AtLeastOnce().Return(1.2d);
            Expect.Call(dayOfWeek.TaskTimeIndex(date)).IgnoreArguments().Repeat.AtLeastOnce().Return(1.2d);

            volumes = new List<IVolumeYear> {monthOfYear, weekOfMonth, dayOfWeek};

            mocks.ReplayAll();

            historicalDepth = new TaskOwnerPeriod(dateTime, periodForHelper.TaskOwnerDays, TaskOwnerPeriodType.Other);
            _averageTasks = historicalDepth.TotalStatisticCalculatedTasks / historicalDepth.TaskOwnerDayCollection.Count;

            target = new TotalVolume();
            target.Create(historicalDepth, _taskOwnerCollection, volumes, new List<IOutlier>(), 1, _dayTrendFactor, _useTrend, workload);
        }

        [Test]
        public void VerifyOutliersWithoutStatisticsWorks()
        {
            Outlier outlier1 = new Outlier(new Description("Easter day"));
            outlier1.AddDate(new DateOnly(2008, 4, 29));

            historicalDepth =
                new TaskOwnerPeriod(historicalDepth.CurrentDate,
                                    new List<ITaskOwner> { historicalDepth.TaskOwnerDayCollection[0] }, TaskOwnerPeriodType.Other);

            target.Create(historicalDepth, _taskOwnerCollection, volumes, new List<IOutlier> { outlier1 }, 1, _dayTrendFactor, false, workload);

            Assert.IsNotNull(target);
        }

        [Test]
        public void VerifyRemoveNonExistingOutlierWorks()
        {
            Outlier outlier = new Outlier(workload, new Description("temp"));
            target.RemoveOutlier(outlier);

            Assert.IsNotNull(target);
        }

        [Test]
        public void VerifyCannotRecalculateOutlierWithoutStatistics()
        {
            target.Create(new TaskOwnerPeriod(historicalDepth.StartDate, new List<ITaskOwner>(), TaskOwnerPeriodType.Other),
                _taskOwnerCollection, volumes, new List<IOutlier>(), 1, 1, false, workload);

            Assert.IsNotNull(target);
        }

        [Test]
        public void VerifyCreateWithOutliersWorks()
        {
            var dateHistory = new DateOnly(2006, 1, 1);
            var dateCurrent = new DateOnly(2008, 4, 13);

            IList<IOutlier> outliers = new List<IOutlier>();
            IOutlier outlier1 = new Outlier(new Description("Easter day"));
            outlier1.AddDate(dateHistory);
            outlier1.AddDate(dateCurrent);
            outliers.Add(outlier1);

            target.Create(historicalDepth, _taskOwnerCollection, volumes, outliers, 1, _dayTrendFactor, false, workload);
            Assert.AreNotEqual(_taskOwnerCollection.First(t => t.CurrentDate == new DateOnly(2008, 4, 12)).Tasks,
                               _taskOwnerCollection.First(t => t.CurrentDate == dateCurrent).Tasks);
            Assert.AreEqual(historicalDepth.TaskOwnerDayCollection.First(t => t.CurrentDate == dateHistory).TotalStatisticCalculatedTasks,
                            Math.Round(_taskOwnerCollection.First(t => t.CurrentDate == dateCurrent).Tasks, 3));
        }

        [Test]
        public void VerifyRemoveOutlierWorks()
        {
            var dateHistory = new DateOnly(2006, 1, 1);
            var dateCurrent = new DateOnly(2008, 4, 13);

            IList<IOutlier> outliers = new List<IOutlier>();
            IOutlier outlier1 = new Outlier(new Description("Easter day"));
            outlier1.AddDate(dateHistory);
            outlier1.AddDate(dateCurrent);
            outliers.Add(outlier1);

            Guid value = Guid.NewGuid();
            outlier1.SetId(value);

            target.Create(historicalDepth, _taskOwnerCollection, volumes, outliers, 1, 1, false, workload);
            target.RemoveOutlier(outlier1);

            Assert.AreEqual(1d, Math.Round(target.TotalDayItemCollection.First(d => dateCurrent == d.CurrentDate).TaskIndex, 4));
        }

        [Test]
        public void VerifyAddDateToOutlier()
        {
            DateTime dateHistory = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2006, 1, 1), _timeZone);
            var dateCurrent1 = new DateOnly(2008, 4, 10);
            var dateCurrent2 = new DateOnly(2008, 4, 13);

            IList<IOutlier> outliers = new List<IOutlier>();
            IOutlier outlier1 = new Outlier(new Description("Easter day"));
            outlier1.AddDate(new DateOnly(TimeZoneInfo.ConvertTimeFromUtc(dateHistory, _timeZone)));
            outlier1.AddDate(dateCurrent1);
            outliers.Add(outlier1);

            target.Create(historicalDepth, _taskOwnerCollection, volumes, outliers, 1, 1, false, workload);

            Assert.AreNotEqual(
                Math.Round(target.TotalDayItemCollection.First(d => dateCurrent1 == d.CurrentDate).TaskIndex, 4),
                Math.Round(target.TotalDayItemCollection.First(d => dateCurrent2 == d.CurrentDate).TaskIndex, 4));

            outlier1.AddDate(new DateOnly(TimeZoneInfo.ConvertTimeFromUtc(dateCurrent2.Date, _timeZone)));
            target.RecalculateOutlier(outlier1);

            Assert.AreEqual(
                Math.Round(target.TotalDayItemCollection.First(d => dateCurrent1 == d.CurrentDate).TaskIndex, 4),
                Math.Round(target.TotalDayItemCollection.First(d => dateCurrent2 == d.CurrentDate).TaskIndex, 4));
        }

		[Test]
		public void ShouldNotCrashWhenAddingOutlierForClosedDay()
		{
            DateTime dateHistory = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2006, 1, 1), _timeZone);
            DateTime dateCurrent1 = TimeZoneInfo.ConvertTimeToUtc(new DateTime(2008, 4, 10), _timeZone);
			DateOnly dateCurrent2 = new DateOnly(2008, 4, 13);

			IList<IOutlier> outliers = new List<IOutlier>();
			Outlier outlier1 = new Outlier(new Description("Easter day"));
            outlier1.AddDate(new DateOnly(TimeZoneInfo.ConvertTimeFromUtc(dateHistory, _timeZone)));
            outlier1.AddDate(new DateOnly(TimeZoneInfo.ConvertTimeFromUtc(dateCurrent1, _timeZone)));
			outliers.Add(outlier1);

            target.Create(historicalDepth, _taskOwnerCollection, volumes, outliers, 1, 1, false, workload);

            outlier1.AddDate(new DateOnly(TimeZoneInfo.ConvertTimeFromUtc(dateCurrent2.Date, _timeZone)));
			((IWorkloadDayBase)_taskOwnerCollection.First(d => dateCurrent2 == d.CurrentDate)).Close();

			target.RecalculateOutlier(outlier1);
		}


        [Test]
        public void VerifyDeleteDateFromOutlier()
        {
            var dateHistory = new DateOnly(2006, 1, 1);
            var dateCurrent1 = new DateOnly(2008, 4, 10);
            var dateCurrent2 = new DateOnly(2008, 4, 13);

            IList<IOutlier> outliers = new List<IOutlier>();
            IOutlier outlier1 = new Outlier(new Description("Easter day"));
            outlier1.AddDate(dateHistory);
            outlier1.AddDate(dateCurrent1);
            outlier1.AddDate(dateCurrent2);
            outliers.Add(outlier1);

            target.Create(historicalDepth, _taskOwnerCollection, volumes, outliers, 1, 1, false, workload);

            Assert.AreEqual(
                Math.Round(target.TotalDayItemCollection.First(d => dateCurrent1 == d.CurrentDate).TaskIndex, 4),
                Math.Round(target.TotalDayItemCollection.First(d => dateCurrent2 == d.CurrentDate).TaskIndex, 4));

            outlier1.RemoveDate(outlier1.Dates[2]);
            target.RecalculateOutlier(outlier1);

            Assert.AreEqual(1d, Math.Round(target.TotalDayItemCollection.First(d => dateCurrent2 == d.CurrentDate).TaskIndex, 4));
        }

        [Test]
        public void VerifyCreateWithOutliersAndTrendWorks()
        {
            var dateHistory = new DateOnly(2006, 1, 1);
            var dateCurrent = new DateOnly(2008, 4, 13);

            IList<IOutlier> outliers = new List<IOutlier>();
            IOutlier outlier1 = new Outlier(new Description("Easter day"));
            outlier1.AddDate(dateHistory);
            outlier1.AddDate(dateCurrent);
            outliers.Add(outlier1);

            target.Create(historicalDepth, _taskOwnerCollection, volumes, outliers, 1, _dayTrendFactor, true, workload);
            Assert.AreEqual(Math.Round(historicalDepth.TaskOwnerDayCollection.First(t => t.CurrentDate == dateHistory).TotalStatisticCalculatedTasks * Math.Pow(_dayTrendFactor, 13), 3),
                            Math.Round(_taskOwnerCollection.First(t => t.CurrentDate == dateCurrent).Tasks, 3));
        }

        [Test]
        public void VerifyCreateWithoutHistoricDepth()
        {
            historicalDepth = new TaskOwnerPeriod(historicalDepth.CurrentDate, new List<ITaskOwner>(), historicalDepth.TypeOfTaskOwnerPeriod);
            target.Create(historicalDepth, _taskOwnerCollection, volumes, new List<IOutlier>(), 1, 1, false, workload);

            Assert.AreEqual(0, target.WorkloadDayCollection[2].Tasks);
            Assert.AreEqual(target.WorkloadDayCollection.Count, target.TotalDayItemCollection.Count);
        }

        /// <summary>
        /// Verifies the create with only ouliers.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-20
        /// </remarks>
        [Test]
        public void VerifyCreateWithOnlyOutliers()
        {
            var dateHistory = new DateOnly(2006, 1, 1);
            var dateCurrent = new DateOnly(2008, 4, 13);

            IList<IOutlier> outliers = new List<IOutlier>();
            IOutlier outlier1 = new Outlier(new Description("Easter day"));
            outlier1.AddDate(dateHistory);
            outlier1.AddDate(dateCurrent);
            outliers.Add(outlier1);

            ITaskOwner historicDayOne = mocks.StrictMock<ITaskOwner>();
            historicDayOne.AddParent(historicalDepth);
            LastCall.IgnoreArguments().Repeat.Twice();
            Expect.Call(historicDayOne.IsLocked).Return(false).Repeat.AtLeastOnce();
            Expect.Call(historicDayOne.OpenForWork).Return(new OpenForWork(true, true)).Repeat.AtLeastOnce();
            Expect.Call(historicDayOne.CurrentDate).Return(outlier1.Dates[0]).Repeat.AtLeastOnce();
            Expect.Call(historicDayOne.TotalTasks).Return(0d).Repeat.AtLeastOnce();
            Expect.Call(historicDayOne.Tasks).Return(0d).Repeat.AtLeastOnce();
            Expect.Call(historicDayOne.AverageTaskTime).Return(TimeSpan.Zero).Repeat.AtLeastOnce();
            Expect.Call(historicDayOne.AverageAfterTaskTime).Return(TimeSpan.Zero).Repeat.AtLeastOnce();
            Expect.Call(historicDayOne.TotalAverageTaskTime).Return(TimeSpan.Zero).Repeat.AtLeastOnce();
            Expect.Call(historicDayOne.TotalAverageAfterTaskTime).Return(TimeSpan.Zero).Repeat.AtLeastOnce();
            Expect.Call(historicDayOne.TotalStatisticAnsweredTasks).Return(0d).Repeat.AtLeastOnce();
            Expect.Call(historicDayOne.TotalStatisticAbandonedTasks).Return(0d).Repeat.AtLeastOnce();
            Expect.Call(historicDayOne.TotalStatisticCalculatedTasks).Return(50d).Repeat.AtLeastOnce();
            Expect.Call(historicDayOne.TotalStatisticAverageTaskTime).Return(TimeSpan.Zero).Repeat.AtLeastOnce();
            Expect.Call(historicDayOne.TotalStatisticAverageAfterTaskTime).Return(TimeSpan.Zero).Repeat.AtLeastOnce();
         
            var openHourPeriods = new List<TimePeriod> {new TimePeriod(8, 0, 8, 15)};
            var currentDayOne = new WorkloadDay();
            var date = outlier1.Dates[1];
            currentDayOne.Create(date, workload, openHourPeriods);
            currentDayOne.Lock();
            LastCall.Repeat.AtLeastOnce();

            mocks.ReplayAll();
            historicalDepth = new TaskOwnerPeriod(dateHistory, new List<ITaskOwner> { historicDayOne }, historicalDepth.TypeOfTaskOwnerPeriod);
            target.Create(historicalDepth, new List<ITaskOwner> { currentDayOne }, volumes, outliers, 1, 1, false, workload);
            mocks.VerifyAll();

            Assert.AreEqual(50, target.WorkloadDayCollection[0].Tasks);
            Assert.AreEqual(target.WorkloadDayCollection.Count, target.TotalDayItemCollection.Count);
        }

        [Test]
        public void VerifyPropertiesWorks()
        {
            double indexMonth = 1d;
            double indexWeek = 1.1d;
            double indexDay = 1.2d;

            double totalIndex = indexMonth * indexWeek * indexDay;
            double tasks = totalIndex * _averageTasks * Math.Pow(_dayTrendFactor, 2); //Two days!

            Assert.AreEqual(Math.Round(tasks, 4), Math.Round(target.WorkloadDayCollection[2].Tasks, 4));
            Assert.AreEqual(target.WorkloadDayCollection.Count, target.TotalDayItemCollection.Count);
        }

        [Test]
        public void VerifyWorksWithClosedDays()
        {
            double indexMonth = 1d;
            double indexWeek = 1.1d;
            double indexDay = 1.2d;

            double totalIndex = indexMonth * indexWeek * indexDay;
            double tasks = totalIndex * _averageTasks * Math.Pow(_dayTrendFactor, 2); //Two days!

            ((IWorkloadDayBase)target.WorkloadDayCollection[1]).Close();
            target.Create(historicalDepth, target.WorkloadDayCollection, volumes, new List<IOutlier>(), 1, _dayTrendFactor, true, workload);
            
            Assert.AreEqual(Math.Round(tasks, 4), Math.Round(target.WorkloadDayCollection[2].Tasks, 4));
            Assert.AreEqual(target.WorkloadDayCollection.Count, target.TotalDayItemCollection.Count);
        }

        [Test]
        public void ShouldGetOpenHoursFromTemplates()
        {
            IList<TimePeriod> openHourPeriods = new List<TimePeriod> {new TimePeriod(8, 0, 8, 15)};

            //1. create closed day
            IWorkloadDay aDay = new WorkloadDay();
            aDay.Create(new DateOnly(2008,12,27), workload, openHourPeriods);
            aDay.Close();

            IWorkloadDay anotherDay = new WorkloadDay();
            anotherDay.Create(new DateOnly(2008, 12, 29), workload, openHourPeriods);

            var taskOwners = new List<ITaskOwner>();
            taskOwners.Add(aDay);
            taskOwners.Add(anotherDay);

            var workloadDayTemplate = (IWorkloadDayTemplate) workload.GetTemplate(TemplateTarget.Workload, DayOfWeek.Saturday);
            workloadDayTemplate.ChangeOpenHours(openHourPeriods);
            workload.SetTemplateAt((int) DayOfWeek.Saturday, workloadDayTemplate);

            var anotherDayTemplate = (IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, DayOfWeek.Monday);
            anotherDayTemplate.ChangeOpenHours(openHourPeriods);
            workload.SetTemplateAt((int)DayOfWeek.Saturday, anotherDayTemplate);


            var t = new TotalVolume();
            t.Create(historicalDepth, taskOwners, volumes, new List<IOutlier>(), 0, 0, false, workload);

            Assert.IsTrue(t.WorkloadDayCollection[0].OpenForWork.IsOpen);
            Assert.AreEqual(openHourPeriods, ((WorkloadDay)t.WorkloadDayCollection[1]).OpenHourList);
        }


        [Test]
        public void VerifyEasternTimeZoneWithDayOfWeeks()
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var dateLocal = new DateOnly(2010, 5, 9);

            workload.Skill.TimeZone = timeZoneInfo;
            IWorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(dateLocal, workload, new List<TimePeriod> { new TimePeriod(8, 0, 8, 15) });
            workloadDay.MergeTemplateTaskPeriods(workloadDay.TaskPeriodList);
            workloadDay.TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 200;
            workloadDay.TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 20;
            workloadDay.TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 120;
            workloadDay.Initialize();

            _taskOwnerCollection.Clear();
            _taskOwnerCollection.Add(workloadDay);

            workloadDay = new WorkloadDay();
            workloadDay.Create(dateLocal.AddDays(1), workload, new List<TimePeriod> { new TimePeriod(8, 0, 8, 15) });
            workloadDay.MergeTemplateTaskPeriods(workloadDay.TaskPeriodList);
            workloadDay.TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 400;
            workloadDay.TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 40;
            workloadDay.TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 240;
            workloadDay.Initialize();

            _taskOwnerCollection.Add(workloadDay);

            DayOfWeeks dayOfWeeks = new DayOfWeeks(new TaskOwnerPeriod(dateLocal, _taskOwnerCollection, TaskOwnerPeriodType.Other), new DaysOfWeekCreator());
            Assert.Less(dayOfWeeks.TaskIndex(dateLocal), 1);
            Assert.Greater(dayOfWeeks.TaskIndex(dateLocal.AddDays(1)), 1);
            Assert.Less(dayOfWeeks.TaskTimeIndex(dateLocal), 1);
            Assert.Greater(dayOfWeeks.TaskTimeIndex(dateLocal.AddDays(1)), 1);
            Assert.Less(dayOfWeeks.AfterTaskTimeIndex(dateLocal), 1);
            Assert.Greater(dayOfWeeks.AfterTaskTimeIndex(dateLocal.AddDays(1)), 1);
        }

        [Test]
        public void VerifyEasternTimeZoneWithWeekOfMonth()
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var dateLocal = new DateOnly(2010, 5, 9);

            workload.Skill.TimeZone = timeZoneInfo;
            IWorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(dateLocal, workload, new List<TimePeriod> { new TimePeriod(8, 0, 8, 15) });
            workloadDay.MergeTemplateTaskPeriods(workloadDay.TaskPeriodList);
            workloadDay.TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 200;
            workloadDay.TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 20;
            workloadDay.TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 120;
            workloadDay.Initialize();

            _taskOwnerCollection.Clear();
            _taskOwnerCollection.Add(workloadDay);

            workloadDay = new WorkloadDay();
            workloadDay.Create(dateLocal.AddDays(8), workload, new List<TimePeriod> { new TimePeriod(8, 0, 8, 15) });
            workloadDay.MergeTemplateTaskPeriods(workloadDay.TaskPeriodList);
            workloadDay.TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 400;
            workloadDay.TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 40;
            workloadDay.TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 240;
            workloadDay.Initialize();

            _taskOwnerCollection.Add(workloadDay);

            WeekOfMonth weekOfMonth = new WeekOfMonth(new TaskOwnerPeriod(dateLocal,  _taskOwnerCollection, TaskOwnerPeriodType.Other), new WeekOfMonthCreator());
            Assert.Less(weekOfMonth.TaskIndex(dateLocal), 1);
            Assert.Greater(weekOfMonth.TaskIndex(dateLocal.AddDays(8)), 1);
            Assert.Less(weekOfMonth.TaskTimeIndex(dateLocal), 1);
            Assert.Greater(weekOfMonth.TaskTimeIndex(dateLocal.AddDays(8)), 1);
            Assert.Less(weekOfMonth.AfterTaskTimeIndex(dateLocal), 1);
            Assert.Greater(weekOfMonth.AfterTaskTimeIndex(dateLocal.AddDays(8)), 1);
        }

        [Test]
        public void VerifyEasternTimeZoneWithMonthOfYear()
        {
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var dateLocal = new DateOnly(2010, 5, 9);

            workload.Skill.TimeZone = timeZoneInfo;
            IWorkloadDay workloadDay = new WorkloadDay();
            workloadDay.Create(dateLocal, workload, new List<TimePeriod> { new TimePeriod(8, 0, 8, 15) });
            workloadDay.MergeTemplateTaskPeriods(workloadDay.TaskPeriodList);
            workloadDay.TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 200;
            workloadDay.TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 20;
            workloadDay.TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 120;
            workloadDay.Initialize();

            _taskOwnerCollection.Clear();
            _taskOwnerCollection.Add(workloadDay);

            workloadDay = new WorkloadDay();
            workloadDay.Create(dateLocal.AddDays(32), workload, new List<TimePeriod> { new TimePeriod(8, 0, 8, 15) });
            workloadDay.MergeTemplateTaskPeriods(workloadDay.TaskPeriodList);
            workloadDay.TaskPeriodList[0].StatisticTask.StatCalculatedTasks = 400;
            workloadDay.TaskPeriodList[0].StatisticTask.StatAverageTaskTimeSeconds = 40;
            workloadDay.TaskPeriodList[0].StatisticTask.StatAverageAfterTaskTimeSeconds = 240;
            workloadDay.Initialize();

            _taskOwnerCollection.Add(workloadDay);

            MonthOfYear monthOfYear =
                new MonthOfYear(new TaskOwnerPeriod(dateLocal, _taskOwnerCollection, TaskOwnerPeriodType.Other),
                                new MonthOfYearCreator());
            Assert.Less(monthOfYear.TaskIndex(dateLocal), 1);
            Assert.Greater(monthOfYear.TaskIndex(dateLocal.AddDays(32)), 1);
            Assert.Less(monthOfYear.TaskTimeIndex(dateLocal), 1);
            Assert.Greater(monthOfYear.TaskTimeIndex(dateLocal.AddDays(32)), 1);
            Assert.Less(monthOfYear.AfterTaskTimeIndex(dateLocal), 1);
            Assert.Greater(monthOfYear.AfterTaskTimeIndex(dateLocal.AddDays(32)), 1);
        }
    }
}
