using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class SignificantPartServiceTest
    {
        private TestSignificantPartSource _source;
        private ISignificantPartService _service;

        [SetUp]
        public void Setup()
        {
            _source = new TestSignificantPartSource();
        }
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyPartIsNotNull()
        {
            _source = null;
            _service = SignificantPartService.CreateService(_source);
            Assert.IsNotNull(_service,"we will not touch this, but fxcop will");
        }

        [Test]
        public void VerifyHasDayOffAndFullAbsence()
        {
            _source.hasDayOff = true;
            _source.hasFullAbsence = true;
            Assert.AreEqual(SchedulePartView.DayOff, SignificantPartService.CreateService(_source).SignificantPart());
        }

        [Test]
        public void VerifyHasFullAbsence()
        {
            _source.hasDayOff = false;
            _source.hasFullAbsence = true;
            Assert.AreEqual(SchedulePartView.FullDayAbsence, SignificantPartService.CreateService(_source).SignificantPart());
        }

        [Test]
        public void VerifyMainShift()
        {
            _source.hasAssignment = true;
            _source.hasMainShift = true;
            Assert.AreEqual(SchedulePartView.MainShift, SignificantPartService.CreateService(_source).SignificantPart());
        }

        [Test]
        public void VerifyDayOff()
        {
            _source.hasFullAbsence = false;
            _source.hasAssignment = true;
            _source.hasMainShift = false;
            _source.hasDayOff = true;
            Assert.AreEqual(SchedulePartView.DayOff, SignificantPartService.CreateService(_source).SignificantPart());

            _source.SetAll(false);
            _source.hasDayOff = true;
            Assert.AreEqual(SchedulePartView.DayOff, SignificantPartService.CreateService(_source).SignificantPart());
        }

        [Test]
        public void VerifyPersonalShift()
        {
            _source.hasAssignment = true;
            _source.hasPersonalShift = true;
            Assert.AreEqual(SchedulePartView.PersonalShift, SignificantPartService.CreateService(_source).SignificantPart());
        }

        [Test]
        public void VerifyAbsence()
        {
            _source.hasFullAbsence = false;
            _source.hasAssignment = false;
            _source.hasDayOff = false;
            _source.hasPersonalShift = true;
            Assert.AreEqual(SchedulePartView.Absence, SignificantPartService.CreateService(_source).SignificantPart());
        }

        [Test]
        public void VerifyOvertime()
        {
            _source.hasFullAbsence = false;
            _source.hasAssignment = false;
            _source.hasDayOff = false;
            _source.hasPersonalShift = false;
            _source.hasOvertime = true;
            Assert.AreEqual(SchedulePartView.Overtime, SignificantPartService.CreateService(_source).SignificantPart());
           
        }

        [Test]
        public void VerifyPreferenceRestriction()
        {
            _source.hasFullAbsence = false;
            _source.hasAssignment = false;
            _source.hasDayOff = false;
            _source.hasPersonalShift = false;
            _source.hasPreferenceRestriction = true;
            Assert.AreEqual(SchedulePartView.PreferenceRestriction, SignificantPartService.CreateService(_source).SignificantPart());
        }

        [Test]
        public void VerifyStudentAvailabilityRestriction()
        {
            _source.hasFullAbsence = false;
            _source.hasAssignment = false;
            _source.hasDayOff = false;
            _source.hasPersonalShift = false;
            _source.hasPreferenceRestriction = false;
            _source.hasStudentAvailabilityRestriction = true;
            Assert.AreEqual(SchedulePartView.StudentAvailabilityRestriction, SignificantPartService.CreateService(_source).SignificantPart());
        }

        [Test]
        public void VerifyNone()
        {
            _source.hasDayOff = false;
            _source.hasFullAbsence = false;
            _source.hasAssignment = false;
            _source.hasDayOff = false;
            _source.hasPersonalShift = false;
            _source.hasAbsence = false;
            Assert.AreEqual(SchedulePartView.None, SignificantPartService.CreateService(_source).SignificantPart());
        }

        [Test]
        public void VerifyContractDayOff()
        {
            _source.hasContractDayOff = true;
            Assert.That(SignificantPartService.CreateService(_source).SignificantPart(), Is.EqualTo(SchedulePartView.ContractDayOff));
        }

        [Test]
        public void ShouldBeAbleToCreateServiceForDisplay()
        {
            Assert.That(SignificantPartService.CreateServiceForDisplay(_source),Is.Not.Null);
        }

        #region helpers
        private class TestSignificantPartSource : ISignificantPartProvider
        {

            internal bool hasDayOff { private get; set; }
            internal bool hasFullAbsence { private get; set; }
            internal bool hasAbsence { private get; set; }
            internal bool hasMainShift { private get; set; }
            internal bool hasAssignment { private get; set; }
            internal bool hasPersonalShift { private get; set; }
            internal bool hasPreferenceRestriction { private get; set; }
            internal bool hasStudentAvailabilityRestriction { private get; set; }
            internal bool hasOvertime { private get; set; }
            internal bool hasContractDayOff { private get; set; }

            public bool HasDayOff()
            {
                return hasDayOff;
            }

            public bool HasFullAbsence()
            {
                return hasFullAbsence;
            }

            public bool HasMainShift()
            {
                return hasMainShift;
            }

            public bool HasAssignment()
            {
                return hasAssignment;
            }

            public bool HasPersonalShift()
            {
                return hasPersonalShift;
            }

            public bool HasAbsence()
            {
                return hasAbsence;
            }

            public bool HasPreferenceRestriction()
            {
                return hasPreferenceRestriction;
            }

            public bool HasStudentAvailabilityRestriction()
            {
                return hasStudentAvailabilityRestriction;
            }

            public bool HasContractDayOff()
            {
                return hasContractDayOff;
            }

            public IDisposable BeginRead()
            {
                return new DummyDisposable();
            }

            private class DummyDisposable : IDisposable
            {
                public void Dispose()
                {
                }
            }

            public bool HasOvertimeShift()
            {
                return hasOvertime;
            }

            public void SetAll(bool property)
            {
                hasDayOff = property;
                hasFullAbsence = property;
                hasMainShift = property;
                hasAssignment = property;
                hasPersonalShift = property;
                hasPreferenceRestriction = property;
                hasStudentAvailabilityRestriction = property;
                hasOvertime = property;
                hasContractDayOff = property;
            }   
        }
        #endregion //helpers
    }

}
