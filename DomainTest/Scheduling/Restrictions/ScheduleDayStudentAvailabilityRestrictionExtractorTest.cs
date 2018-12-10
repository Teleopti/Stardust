using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
    [TestFixture]
    public class ScheduleDayStudentAvailabilityRestrictionExtractorTest
    {
        private ScheduleDayStudentAvailabilityRestrictionExtractor _target;
        private MockRepository _mock;
        private IRestrictionExtractor _restrictionExtractor;
        private IScheduleDay _scheduleDay;
        private IList<IScheduleDay> _scheduleDays;
        private IList<IStudentAvailabilityDay> _studentAvailabilityDays;
        private IStudentAvailabilityDay _studentAvailabilityDay;
        private ICheckerRestriction _restrictionChecker;
        private IPerson _person;
        private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;
        private IPersonPeriod _personPeriod;
        private IPersonContract _personContract;
        private IContract _contract;
	    private IExtractedRestrictionResult _extractedRestrictionResult;

	    [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _restrictionExtractor = _mock.StrictMock<IRestrictionExtractor>();
	        _extractedRestrictionResult = _mock.StrictMock<IExtractedRestrictionResult>();
            _target = new ScheduleDayStudentAvailabilityRestrictionExtractor(_restrictionExtractor);
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _scheduleDays = new List<IScheduleDay>{_scheduleDay};
            _studentAvailabilityDay = _mock.StrictMock<IStudentAvailabilityDay>();
            _studentAvailabilityDays = new List<IStudentAvailabilityDay>{_studentAvailabilityDay};
            _restrictionChecker = _mock.StrictMock<ICheckerRestriction>();
            _person = _mock.StrictMock<IPerson>();
            _dateOnlyAsDateTimePeriod = _mock.StrictMock<IDateOnlyAsDateTimePeriod>();
            _personPeriod = _mock.StrictMock<IPersonPeriod>();
            _personContract = _mock.StrictMock<IPersonContract>();
            _contract = _mock.StrictMock<IContract>();
        }

        [Test]
        public void ShouldGetAllUnavailable()
        {
            using(_mock.Record())
            {
                Expect.Call(_restrictionExtractor.Extract(_scheduleDay)).Return(_extractedRestrictionResult);
                Expect.Call(_extractedRestrictionResult.StudentAvailabilityList).Return(new List<IStudentAvailabilityDay>());
                Expect.Call(_scheduleDay.Person).Return(_person);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly());
                Expect.Call(_person.Period(new DateOnly())).Return(_personPeriod);
                Expect.Call(_personPeriod.PersonContract).Return(_personContract);
                Expect.Call(_personContract.Contract).Return(_contract);
                Expect.Call(_contract.EmploymentType).Return(EmploymentType.HourlyStaff);
            }

            using(_mock.Playback())
            {
                var scheduleDays = _target.AllUnavailable(_scheduleDays);
                Assert.AreEqual(1, scheduleDays.Count);
            }
        }

        [Test]
        public void ShouldNotConsiderAgentsWithoutPersonPeriodOnAllUnavailable()
        {
            using (_mock.Record())
            {
                Expect.Call(_restrictionExtractor.Extract(_scheduleDay)).Return(_extractedRestrictionResult);
                Expect.Call(_extractedRestrictionResult.StudentAvailabilityList).Return(new List<IStudentAvailabilityDay>());
                Expect.Call(_scheduleDay.Person).Return(_person);
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
                Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly());
                Expect.Call(_person.Period(new DateOnly())).Return(null);
            }

            using (_mock.Playback())
            {
                var scheduleDays = _target.AllUnavailable(_scheduleDays);
                Assert.AreEqual(0, scheduleDays.Count);
            }    
        }

        [Test]
        public void ShouldGetAllAvailable()
        {
            using(_mock.Record())
            {
                Expect.Call(_restrictionExtractor.Extract(_scheduleDay)).Return(_extractedRestrictionResult);
                Expect.Call(_extractedRestrictionResult.StudentAvailabilityList).Return(_studentAvailabilityDays);
            }

            using(_mock.Playback())
            {
                var scheduleDays = _target.AllAvailable(_scheduleDays);
                Assert.AreEqual(1, scheduleDays.Count);
            }
        }

        [Test]
        public void ShouldGetAllFulfilled()
        {
            using(_mock.Record())
            {
                Expect.Call(_restrictionChecker.CheckStudentAvailability(_scheduleDay)).Return(PermissionState.Satisfied);
            }

            using(_mock.Playback())
            {
                var scheduleDay = _target.RestrictionFulfilled(_restrictionChecker, _scheduleDay);
                Assert.AreEqual(_scheduleDay, scheduleDay);
            }
        }
    }
}
