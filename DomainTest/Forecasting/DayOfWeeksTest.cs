using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class DayOfWeeksTest
    {
        private DayOfWeeks target;
        private MockRepository mocks;
        private ITaskOwnerPeriod taskOwnerPeriod;
        private IDaysOfWeekCreator daysOfWeekCreator;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            taskOwnerPeriod = mocks.StrictMock<ITaskOwnerPeriod>();
            daysOfWeekCreator = mocks.StrictMock<IDaysOfWeekCreator>();
        }

        [Test]
        public void ShouldCreateNewInstance()
        {
            using (mocks.Record())
            {
                ExpectTaskOwnerPeriodStuff();
                Expect.Call(() => daysOfWeekCreator.Create(null)).IgnoreArguments();
            }
            using (mocks.Playback())
            {
                target = new DayOfWeeks(taskOwnerPeriod, daysOfWeekCreator);
                Assert.IsNotNull(target);
            }
        }

        private void ExpectTaskOwnerPeriodStuff()
        {
            Expect.Call(taskOwnerPeriod.TotalStatisticCalculatedTasks).Return(10d);
            Expect.Call(taskOwnerPeriod.TotalStatisticAverageTaskTime).Return(TimeSpan.FromSeconds(10));
            Expect.Call(taskOwnerPeriod.TotalStatisticAverageAfterTaskTime).Return(TimeSpan.FromSeconds(20));
        }

        [Test]
        public void ShouldReloadHistoricalData()
        {
            using (mocks.Record())
            {
                ExpectTaskOwnerPeriodStuff();
                ExpectTaskOwnerPeriodStuff();
                Expect.Call(() => daysOfWeekCreator.Create(null)).IgnoreArguments().Repeat.Times(2);
            }
            using (mocks.Playback())
            {
                target = new DayOfWeeks(taskOwnerPeriod, daysOfWeekCreator);
                target.ReloadHistoricalDataDepth(taskOwnerPeriod);
                Assert.IsNotNull(target);
            }
        }

        [Test]
        public void ShouldGetIndexValues()
        {
            IPeriodType monthPeriod = mocks.StrictMock<IPeriodType>();
            using (mocks.Record())
            {
                ExpectTaskOwnerPeriodStuff();
                Expect.Call(monthPeriod.TaskIndex).Return(1.2);
                Expect.Call(monthPeriod.TalkTimeIndex).Return(1.3);
                Expect.Call(monthPeriod.AfterTalkTimeIndex).Return(1.4);
                Expect.Call(() => daysOfWeekCreator.Create(null)).IgnoreArguments();
            }
            using (mocks.Playback())
            {
                var theDate = new DateOnly(2010, 9, 21);
                int weekNumber = WeekOfMonthItem.WeekIndex(CultureInfo.CurrentCulture.Calendar.GetDayOfMonth(theDate.Date));
                target = new DayOfWeeks(taskOwnerPeriod, daysOfWeekCreator);
                target.PeriodTypeCollection.Add(weekNumber, monthPeriod);

                Assert.AreEqual(1.2, target.TaskIndex(theDate));
                Assert.AreEqual(1.3, target.TaskTimeIndex(theDate));
                Assert.AreEqual(1.4, target.AfterTaskTimeIndex(theDate));
            }
        }

        [Test]
        public void VerifyProperties()
        {
            using (mocks.Record())
            {
                ExpectTaskOwnerPeriodStuff();
                Expect.Call(taskOwnerPeriod.TaskOwnerDayCollection).Return(
                    new ReadOnlyCollection<ITaskOwner>(new List<ITaskOwner> {null})).Repeat.Times(3);
                Expect.Call(() => daysOfWeekCreator.Create(null)).IgnoreArguments();
            }
            using (mocks.Playback())
            {
                target = new DayOfWeeks(taskOwnerPeriod, daysOfWeekCreator);
                Assert.AreEqual(1,target.TaskOwnerDays.Count);
                Assert.AreEqual(1,target.NumberOfDays);
                Assert.AreEqual(10d,target.AverageTasksPerDay);
                Assert.AreEqual(10d,target.TotalTasks);
                Assert.AreEqual(TimeSpan.FromSeconds(10d),target.TalkTime);
                Assert.AreEqual(TimeSpan.FromSeconds(20d),target.AfterTalkTime);
            }
        }
    }
}