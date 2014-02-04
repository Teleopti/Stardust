using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Rhino.Mocks;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
    public class ScheduleDayAvailabilityRestrictionExtractorTest
    {
        private ScheduleDayAvailabilityRestrictionExtractor _target;
        private MockRepository _mock;
        private IRestrictionExtractor _restrictionExtractor;
        private IList<IScheduleDay> _scheduleDays;
        private IScheduleDay _scheduleDay;
        private IAvailabilityRestriction _availabilityRestriction;
        private IList<IAvailabilityRestriction> _availabilityRestrictions;
        private ICheckerRestriction _restrictionChecker;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _restrictionExtractor = _mock.StrictMock<IRestrictionExtractor>();
            _target = new ScheduleDayAvailabilityRestrictionExtractor(_restrictionExtractor);
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _scheduleDays = new List<IScheduleDay>{_scheduleDay};
            _availabilityRestriction = _mock.StrictMock<IAvailabilityRestriction>();
            _availabilityRestrictions = new List<IAvailabilityRestriction>{_availabilityRestriction};
            _restrictionChecker = _mock.StrictMock<ICheckerRestriction>();
        }

        [Test]
        public void ShouldGetAllUnavailableRestrictions()
        {
            using(_mock.Record())
            {
                Expect.Call(() => _restrictionExtractor.Extract(_scheduleDay));
                Expect.Call(_restrictionExtractor.AvailabilityList).Return(_availabilityRestrictions);
                Expect.Call(_availabilityRestriction.NotAvailable).Return(true);    
            }

            using(_mock.Playback())
            {
                var scheduleDays = _target.AllUnavailable(_scheduleDays);
                Assert.AreEqual(1, scheduleDays.Count);
            }
        }

        [Test]
        public void ShouldGetAllAvailableRestrictions()
        {
            using(_mock.Record())
            {
                Expect.Call(() => _restrictionExtractor.Extract(_scheduleDay));
                Expect.Call(_restrictionExtractor.AvailabilityList).Return(_availabilityRestrictions);
                Expect.Call(_availabilityRestriction.NotAvailable).Return(false);
            }

            using(_mock.Playback())
            {
                var scheduleDays = _target.AllAvailable(_scheduleDays);
                Assert.AreEqual(1, scheduleDays.Count);
            }
        }

        [Test]
        public void ShouldGetAllFulfilledRestrictions()
        {
            using(_mock.Record())
            {
                Expect.Call(_restrictionChecker.CheckAvailability(_scheduleDay)).Return(PermissionState.Satisfied);
            }

            using(_mock.Playback())
            {
                var scheduleDay = _target.RestrictionFulfilled(_restrictionChecker, _scheduleDay);
                Assert.AreEqual(_scheduleDay, scheduleDay);
            }
        }
    }
}
