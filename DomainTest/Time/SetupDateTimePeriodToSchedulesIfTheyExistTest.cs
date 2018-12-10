using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Time
{
	[TestWithStaticDependenciesDONOTUSE]
	[TestFixture]
    public  class  SetupDateTimePeriodToSchedulesIfTheyExistTest
    {

        private SetupDateTimePeriodToSchedulesIfTheyExist _target;
        private MockRepository _mocker;

        [SetUp]
        public void Setup()
        {
            _mocker = new MockRepository();   
        }

        [Test]
        public void VerifyUsesCallbackIfListIsEmpty()
        {
            DateTimePeriod period = new DateTimePeriod(2001,1,1,2001,1,3);
            ISetupDateTimePeriod fallBack=_mocker.StrictMock<ISetupDateTimePeriod>();
            _target = new SetupDateTimePeriodToSchedulesIfTheyExist(new List<IScheduleDay>(),fallBack );
            using(_mocker.Record())
            {
                Expect.Call(fallBack.Period).Return(period);
            }
            using (_mocker.Playback())
            {
                Assert.AreEqual(period, _target.Period);
            }
        }

        [Test]
        public void VerifyUsesCallbackIfListIsNull()
        {
            DateTimePeriod period = new DateTimePeriod(2001, 1, 1, 2001, 1, 3);
            ISetupDateTimePeriod fallBack = _mocker.StrictMock<ISetupDateTimePeriod>();
            _target = new SetupDateTimePeriodToSchedulesIfTheyExist(null, fallBack);
            using (_mocker.Record())
            {
                Expect.Call(fallBack.Period).Return(period);
            }
            using (_mocker.Playback())
            {
                Assert.AreEqual(period, _target.Period);
            }
        }

        [Test]
        public void VerifyCalculatesFromSchedulesIfTheyExistAndHasMainShift()
        {
            IList<IScheduleDay> scheduleDays = new List<IScheduleDay>{new SchedulePartFactoryForDomain().CreatePartWithMainShift()};
            SetupDateTimePeriodToSelectedSchedules comparer = new SetupDateTimePeriodToSelectedSchedules(scheduleDays);

            ISetupDateTimePeriod fallBack = _mocker.StrictMock<ISetupDateTimePeriod>();
            _target = new SetupDateTimePeriodToSchedulesIfTheyExist(scheduleDays, fallBack);
          
            Assert.AreEqual(comparer.Period, _target.Period);

        }

        [Test]
        public void VerifyUsesFallbackIfOneScheduleAndNoMainShift()
        {
            IList<IScheduleDay> scheduleDays = new List<IScheduleDay> { new SchedulePartFactoryForDomain().CreatePartWithoutMainShift() };
            var period = new DateTimePeriod(2001, 1, 1, 2001, 1, 3);
            var fallBack = _mocker.StrictMock<ISetupDateTimePeriod>();

           
            _target = new SetupDateTimePeriodToSchedulesIfTheyExist(scheduleDays, fallBack);
            using(_mocker.Record())
            {
                Expect.Call(fallBack.Period).Return(period);
            }
            using(_mocker.Playback())
            {
                Assert.AreEqual(period, _target.Period);
            }

        }

        [Test]
        public void VerifyCalculatesFromSchedulesIfMoreThanOneSchedule()
        {
            IList<IScheduleDay> scheduleDays = new List<IScheduleDay>() { new SchedulePartFactoryForDomain().CreatePartWithoutMainShift(), new SchedulePartFactoryForDomain().CreatePartWithoutMainShift() };
            SetupDateTimePeriodToSelectedSchedules comparer = new SetupDateTimePeriodToSelectedSchedules(scheduleDays);

            _target = new SetupDateTimePeriodToSchedulesIfTheyExist(scheduleDays, _mocker.StrictMock<ISetupDateTimePeriod>());

            Assert.AreEqual(comparer.Period, _target.Period);

        }
    }
}