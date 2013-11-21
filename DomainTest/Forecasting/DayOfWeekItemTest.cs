using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class DayOfWeekItemTest
    {
        private MockRepository mocks;
        private ITaskOwnerPeriod taskOwnerPeriod;
        private IVolumeYear volumeYear;
        private DayOfWeekItem target;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            taskOwnerPeriod = mocks.StrictMock<ITaskOwnerPeriod>();
            volumeYear = mocks.StrictMock<IVolumeYear>();
        }

        [Test]
        public void VerifyProperties()
        {
            using(mocks.Record())
            {
                Expect.Call(taskOwnerPeriod.TotalStatisticCalculatedTasks).Return(14d);
                Expect.Call(taskOwnerPeriod.TotalStatisticAverageTaskTime).Return(TimeSpan.FromSeconds(14));
                Expect.Call(taskOwnerPeriod.TotalStatisticAverageAfterTaskTime).Return(TimeSpan.FromSeconds(28));
                Expect.Call(taskOwnerPeriod.TaskOwnerDayCollection).Return(
                    new ReadOnlyCollection<ITaskOwner>(new List<ITaskOwner> {null})).Repeat.AtLeastOnce();
                Expect.Call(volumeYear.AverageTasksPerDay).Return(2);
                Expect.Call(volumeYear.TalkTime).Return(TimeSpan.FromSeconds(28));
                Expect.Call(volumeYear.AfterTalkTime).Return(TimeSpan.FromSeconds(56));
            }
            using (mocks.Playback())
            {
                target = new DayOfWeekItem(taskOwnerPeriod, volumeYear);
                Assert.AreEqual(14,target.DailyAverageTasks);
                Assert.AreEqual(7,target.TaskIndex);
                target.TaskIndex = 14;
                Assert.AreEqual(14,target.TaskIndex);
                Assert.AreEqual(28,target.AverageTasks);

                target.AverageTasks = 14;
                Assert.AreEqual(7, target.TaskIndex);

                Assert.AreEqual(0.5, target.TalkTimeIndex);
                target.TalkTimeIndex = 1;
                Assert.AreEqual(1, target.TalkTimeIndex);
                Assert.AreEqual(TimeSpan.FromSeconds(28), target.AverageTalkTime);

                target.AverageTalkTime = TimeSpan.FromSeconds(14);
                Assert.AreEqual(0.5, target.TalkTimeIndex);

                Assert.AreEqual(0.5, target.AfterTalkTimeIndex);
                target.AfterTalkTimeIndex = 1;
                Assert.AreEqual(1, target.AfterTalkTimeIndex);
                Assert.AreEqual(TimeSpan.FromSeconds(56), target.AverageAfterWorkTime);

                target.AverageAfterWorkTime = TimeSpan.FromSeconds(28);
                Assert.AreEqual(0.5, target.TalkTimeIndex);
            }
        }

        [Test]
        public void ShouldAbleToChangeAverageTasksAfterItWasZero()
        {
            using(mocks.Record())
            {
                Expect.Call(taskOwnerPeriod.TotalStatisticCalculatedTasks).Return(14d);
                Expect.Call(taskOwnerPeriod.TotalStatisticAverageTaskTime).Return(TimeSpan.FromSeconds(14));
                Expect.Call(taskOwnerPeriod.TotalStatisticAverageAfterTaskTime).Return(TimeSpan.FromSeconds(28));
                Expect.Call(taskOwnerPeriod.TaskOwnerDayCollection).Return(
                    new ReadOnlyCollection<ITaskOwner>(new List<ITaskOwner> {null})).Repeat.AtLeastOnce();
                Expect.Call(volumeYear.AverageTasksPerDay).Return(2);
                Expect.Call(volumeYear.TalkTime).Return(TimeSpan.FromSeconds(28));
                Expect.Call(volumeYear.AfterTalkTime).Return(TimeSpan.FromSeconds(56));
            }
            using (mocks.Playback())
            {
                target = new DayOfWeekItem(taskOwnerPeriod, volumeYear);

                target.AverageTasks = 0;
                Assert.AreEqual(0, target.AverageTasks);
                Assert.AreEqual(0, target.TaskIndex);

                target.AverageTasks = 100;
                Assert.AreEqual(100, target.AverageTasks);
                Assert.AreEqual(50d, Math.Round(target.TaskIndex, 2));
            }
        }

        [Test]
        public void ShouldAbleToChangeAverageTalkTimeAfterItWasZero()
        {
            using (mocks.Record())
            {
                Expect.Call(taskOwnerPeriod.TotalStatisticCalculatedTasks).Return(14d);
                Expect.Call(taskOwnerPeriod.TotalStatisticAverageTaskTime).Return(TimeSpan.FromSeconds(14));
                Expect.Call(taskOwnerPeriod.TotalStatisticAverageAfterTaskTime).Return(TimeSpan.FromSeconds(28));
                Expect.Call(taskOwnerPeriod.TaskOwnerDayCollection).Return(
                    new ReadOnlyCollection<ITaskOwner>(new List<ITaskOwner> { null })).Repeat.AtLeastOnce();
                Expect.Call(volumeYear.AverageTasksPerDay).Return(2);
                Expect.Call(volumeYear.TalkTime).Return(TimeSpan.FromSeconds(28));
                Expect.Call(volumeYear.AfterTalkTime).Return(TimeSpan.FromSeconds(56));
            }
            using (mocks.Playback())
            {
                target = new DayOfWeekItem(taskOwnerPeriod, volumeYear);

                target.AverageTalkTime = TimeSpan.FromSeconds(0);
                Assert.AreEqual(TimeSpan.FromSeconds(0), target.AverageTalkTime);
                Assert.AreEqual(0, target.TalkTimeIndex);

                target.AverageTalkTime = TimeSpan.FromSeconds(100);
                Assert.AreEqual(TimeSpan.FromSeconds(100), target.AverageTalkTime);
                Assert.AreEqual(3.57d, Math.Round(target.TalkTimeIndex, 2));
            }
        }

        [Test]
        public void ShouldAbleToChangeAverageAfterWorkTimeAfterItWasZero()
        {
            using(mocks.Record())
            {
                Expect.Call(taskOwnerPeriod.TotalStatisticCalculatedTasks).Return(14d);
                Expect.Call(taskOwnerPeriod.TotalStatisticAverageTaskTime).Return(TimeSpan.FromSeconds(14));
                Expect.Call(taskOwnerPeriod.TotalStatisticAverageAfterTaskTime).Return(TimeSpan.FromSeconds(28));
                Expect.Call(taskOwnerPeriod.TaskOwnerDayCollection).Return(
                    new ReadOnlyCollection<ITaskOwner>(new List<ITaskOwner> {null})).Repeat.AtLeastOnce();
                Expect.Call(volumeYear.AverageTasksPerDay).Return(2);
                Expect.Call(volumeYear.TalkTime).Return(TimeSpan.FromSeconds(28));
                Expect.Call(volumeYear.AfterTalkTime).Return(TimeSpan.FromSeconds(56));
            }
            using (mocks.Playback())
            {
                target = new DayOfWeekItem(taskOwnerPeriod, volumeYear);

                target.AverageAfterWorkTime = TimeSpan.FromSeconds(0);
                Assert.AreEqual(TimeSpan.FromSeconds(0), target.AverageAfterWorkTime);
                Assert.AreEqual(0, target.AfterTalkTimeIndex);

                target.AverageAfterWorkTime = TimeSpan.FromSeconds(100);
                Assert.AreEqual(TimeSpan.FromSeconds(100), target.AverageAfterWorkTime);
                Assert.AreEqual(1.79d, Math.Round(target.AfterTalkTimeIndex, 2));
            }
        }

		[Test]
		public void ShouldNotAllowNegativeValuesInAverageTasks()
		{
			using (mocks.Record())
			{
				Expect.Call(taskOwnerPeriod.TotalStatisticCalculatedTasks).Return(14d);
				Expect.Call(taskOwnerPeriod.TotalStatisticAverageTaskTime).Return(TimeSpan.FromSeconds(14));
				Expect.Call(taskOwnerPeriod.TotalStatisticAverageAfterTaskTime).Return(TimeSpan.FromSeconds(28));
				Expect.Call(taskOwnerPeriod.TaskOwnerDayCollection).Return(new ReadOnlyCollection<ITaskOwner>(new List<ITaskOwner> { null })).Repeat.AtLeastOnce();
				Expect.Call(volumeYear.AverageTasksPerDay).Return(2);
				Expect.Call(volumeYear.TalkTime).Return(TimeSpan.FromSeconds(28));
				Expect.Call(volumeYear.AfterTalkTime).Return(TimeSpan.FromSeconds(56));
			}

			using (mocks.Playback())
			{
				target = new DayOfWeekItem(taskOwnerPeriod, volumeYear);

				target.AverageTasks = 1;
				Assert.AreEqual(1, target.AverageTasks);

				target.AverageTasks = -2;
				Assert.AreEqual(1, target.AverageTasks);
			}
		}
    }
}
