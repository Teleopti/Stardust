using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class WeekOfMonthCreatorTest
    {
        private IWeekOfMonthCreator target;
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            target = new WeekOfMonthCreator();
        }

        [Test]
        public void ShouldCreateMonthOfYear()
        {
            IVolumeYear volumeYear = mocks.StrictMock<IVolumeYear>();
            ITaskOwner taskOwner = mocks.StrictMock<ITaskOwner>();
            IDictionary<int,IPeriodType> periodTypes = new Dictionary<int, IPeriodType>();
            using (mocks.Record())
            {
                Expect.Call(volumeYear.PeriodTypeCollection).Return(periodTypes).Repeat.AtLeastOnce();
                Expect.Call(volumeYear.TaskOwnerDays).Return(new List<ITaskOwner> {taskOwner});

                //This could be avoided if we mock the task owner helper. Look at that if there's time.
                Expect.Call(taskOwner.CurrentDate).Return(new DateOnly(2010, 9, 21)).Repeat.AtLeastOnce();
                Expect.Call(()=>taskOwner.AddParent(null)).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(taskOwner.IsLocked).Return(false).Repeat.AtLeastOnce();
                Expect.Call(taskOwner.OpenForWork).Return(new OpenForWork()).Repeat.AtLeastOnce(); 
                Expect.Call(taskOwner.TotalStatisticCalculatedTasks).Return(0).Repeat.AtLeastOnce();
                Expect.Call(taskOwner.TotalStatisticAnsweredTasks).Return(0).Repeat.AtLeastOnce();
                Expect.Call(taskOwner.TotalStatisticAbandonedTasks).Return(0).Repeat.AtLeastOnce();
                Expect.Call(taskOwner.TotalStatisticAverageTaskTime).Return(TimeSpan.Zero).Repeat.AtLeastOnce();
                Expect.Call(taskOwner.TotalStatisticAverageAfterTaskTime).Return(TimeSpan.Zero).Repeat.AtLeastOnce();

                Expect.Call(volumeYear.AverageTasksPerDay).Return(10).Repeat.AtLeastOnce();
                Expect.Call(volumeYear.TalkTime).Return(TimeSpan.FromSeconds(10)).Repeat.AtLeastOnce();
                Expect.Call(volumeYear.AfterTalkTime).Return(TimeSpan.FromSeconds(20)).Repeat.AtLeastOnce();
            }
            using (mocks.Playback())
            {
                target.Create(volumeYear);
                Assert.AreEqual(5,periodTypes.Count);
            }
        }
    }
}
