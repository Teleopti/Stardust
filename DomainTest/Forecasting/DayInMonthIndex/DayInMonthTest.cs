using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.DayInMonthIndex;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting.DayInMonthIndex
{
    [TestFixture]
    public class DayInMonthTest
    {
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
        }

        [Test]
        public void ShouldCallCreatorInConstructor()
        {
            var creator = _mocks.DynamicMock<IDayInMonthCreator>();
            var taskOwnerPeriod = _mocks.DynamicMock<ITaskOwnerPeriod>();
            Expect.Call(() => creator.Create(null)).IgnoreArguments();
            _mocks.ReplayAll();
            new DayInMonth(taskOwnerPeriod, creator);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldGetIndexFromList()
        {
            var creator = _mocks.DynamicMock<IDayInMonthCreator>();
            var taskOwnerPeriod = _mocks.DynamicMock<ITaskOwnerPeriod>();
            var dayInMonth =new DayInMonth(taskOwnerPeriod, creator);
            dayInMonth.PeriodTypeCollection.Add(1, new DayInMonthItem { TaskIndex = 0.01, AfterTalkTimeIndex = 1, TalkTimeIndex = 1 });
            dayInMonth.PeriodTypeCollection.Add(15, new DayInMonthItem { TaskIndex = 0.15, AfterTalkTimeIndex = 1, TalkTimeIndex = 1 });
            dayInMonth.PeriodTypeCollection.Add(16, new DayInMonthItem { TaskIndex = 0.16, AfterTalkTimeIndex = 1, TalkTimeIndex = 1 });
            dayInMonth.PeriodTypeCollection.Add(30, new DayInMonthItem { TaskIndex = 0.31, AfterTalkTimeIndex = 1, TalkTimeIndex = 1 });

            Assert.That(dayInMonth.TaskIndex(new DateOnly(2011,2,28)),Is.EqualTo(0.31));
            Assert.That(dayInMonth.TaskIndex(new DateOnly(2011,1,31)),Is.EqualTo(0.31));
            Assert.That(dayInMonth.TaskIndex(new DateOnly(2011,4,30)),Is.EqualTo(0.31));
            Assert.That(dayInMonth.TaskIndex(new DateOnly(2012,2,29)),Is.EqualTo(0.31));
            Assert.That(dayInMonth.TaskIndex(new DateOnly(2012,2,1)),Is.EqualTo(0.01));
            Assert.That(dayInMonth.TaskIndex(new DateOnly(2012,2,15)),Is.EqualTo(0.16));
            Assert.That(dayInMonth.AfterTaskTimeIndex(new DateOnly(2012, 2, 15)), Is.EqualTo(1));
            Assert.That(dayInMonth.TaskTimeIndex(new DateOnly(2012, 2, 15)), Is.EqualTo(1));
        }

        [Test]
        public void ShouldDoNothingOnReload()
        {
            var creator = _mocks.DynamicMock<IDayInMonthCreator>();
            var taskOwnerPeriod = _mocks.DynamicMock<ITaskOwnerPeriod>();
            Expect.Call(() => creator.Create(null)).IgnoreArguments().Repeat.Times(1);
            _mocks.ReplayAll();
            var dayInMonth = new DayInMonth(taskOwnerPeriod, creator);
            //no call to creator here
            dayInMonth.ReloadHistoricalDataDepth(taskOwnerPeriod);
            _mocks.VerifyAll();

        }
    }
}