using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class IsSignificantPartFullDayAbsenceOrDayOffSpecificationTest
    {
        private IsSignificantPartFullDayAbsenceOrDayOffSpecification _target;

        [SetUp]
        public void Setup()
        {
            _target = new IsSignificantPartFullDayAbsenceOrDayOffSpecification();
        }

        [Test]
        public void ShouldSatisfiedTheseSignificantParts()
        {
            Assert.That(_target.IsSatisfiedBy(SchedulePartView.FullDayAbsence), Is.True);
            Assert.That(_target.IsSatisfiedBy(SchedulePartView.ContractDayOff), Is.True);
            Assert.That(_target.IsSatisfiedBy(SchedulePartView.DayOff), Is.True);
        }

        [Test]
        public void ShouldNotSatisfiedTheseSignificantParts()
        {
            Assert.That(_target.IsSatisfiedBy(SchedulePartView.Absence), Is.False);
            Assert.That(_target.IsSatisfiedBy(SchedulePartView.MainShift), Is.False);
            Assert.That(_target.IsSatisfiedBy(SchedulePartView.None), Is.False);
            Assert.That(_target.IsSatisfiedBy(SchedulePartView.Overtime), Is.False);
            Assert.That(_target.IsSatisfiedBy(SchedulePartView.PersonalShift), Is.False);
            Assert.That(_target.IsSatisfiedBy(SchedulePartView.PreferenceRestriction), Is.False);
            Assert.That(_target.IsSatisfiedBy(SchedulePartView.StudentAvailabilityRestriction), Is.False);
        }
    }
}
