using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;

namespace Teleopti.Ccc.AgentPortalCodeTest.AgentPreference
{
    [TestFixture]
    public class EditStudentAvailabilityModelTest
    {
        private EditStudentAvailabilityModel _target;
        private MockRepository _mocks;
        private IPermissionService _permissionService;
        private IApplicationFunctionHelper _applicationFunctionHelper;
        StudentAvailabilityRestriction _studentRestriction;
        private IList<StudentAvailabilityRestriction> _studentRestrictions;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _mocks.StrictMock<ISessionData>();
            _permissionService = _mocks.StrictMock<IPermissionService>();
            _applicationFunctionHelper = _mocks.StrictMock<IApplicationFunctionHelper>();
            _target = new EditStudentAvailabilityModel(_permissionService, _applicationFunctionHelper);

            _studentRestriction = new StudentAvailabilityRestriction();
            ITimeLimitationValidator validator = new TimeOfDayValidator(false);
            _studentRestriction.StartTimeLimitation = new TimeLimitation(validator);
            _studentRestriction.EndTimeLimitation = new TimeLimitation(validator);
            _studentRestriction.StartTimeLimitation.MinTime = new TimeSpan(7, 0, 0);
            _studentRestriction.EndTimeLimitation.MaxTime = new TimeSpan(15, 0, 0);
            _studentRestrictions = new List<StudentAvailabilityRestriction> { _studentRestriction };
        }

        [Test]
        public void ShouldSetFieldValues()
        {
            var secondStudentRestriction = new StudentAvailabilityRestriction();
            ITimeLimitationValidator validator = new TimeOfDayValidator(false);
            secondStudentRestriction.StartTimeLimitation = new TimeLimitation(validator);
            secondStudentRestriction.EndTimeLimitation = new TimeLimitation(validator);
            if (_studentRestriction.StartTimeLimitation.MinTime != null)
                secondStudentRestriction.StartTimeLimitation.MinTime =
                    _studentRestriction.StartTimeLimitation.MinTime.Value.Add(new TimeSpan(2, 0, 0));
            if (_studentRestriction.EndTimeLimitation.MaxTime != null)
                secondStudentRestriction.EndTimeLimitation.MaxTime =
                    _studentRestriction.EndTimeLimitation.MaxTime.Value.Add(new TimeSpan(4, 0, 0));
            _studentRestrictions.Add(secondStudentRestriction);

            _target.SetStudentAvailabilityRestrictions(_studentRestrictions);

            Assert.That(_target.StartTimeLimitation, Is.EqualTo(_studentRestrictions[0].StartTimeLimitation.MinTime));
            Assert.That(_target.EndTimeLimitation, Is.EqualTo(_studentRestrictions[0].EndTimeLimitation.MaxTime));
            Assert.That(_target.SecondStartTimeLimitation, Is.EqualTo(_studentRestrictions[1].StartTimeLimitation.MinTime));
            Assert.That(_target.SecondEndTimeLimitation, Is.EqualTo(_studentRestrictions[1].EndTimeLimitation.MaxTime));
        }

        [Test]
        public void ShouldInitializeWithStudentAvailabilityValuesWhenEndTimeIsBeforeStartTime()
        {
            _studentRestriction.StartTimeLimitation.MinTime = new TimeSpan(7, 0, 0);
            var endTime = new TimeSpan(16, 0, 0);
            _studentRestriction.EndTimeLimitation.MaxTime = endTime.Add(new TimeSpan(12, 0, 0));
            _studentRestrictions = new List<StudentAvailabilityRestriction> { _studentRestriction };
            var secondStudentRestriction = new StudentAvailabilityRestriction();
            ITimeLimitationValidator validator = new TimeOfDayValidator(false);
            secondStudentRestriction.StartTimeLimitation = new TimeLimitation(validator);
            secondStudentRestriction.EndTimeLimitation = new TimeLimitation(validator);
            secondStudentRestriction.StartTimeLimitation.MinTime =
                _studentRestriction.StartTimeLimitation.MinTime.Value.Add(new TimeSpan(2, 0, 0));
            secondStudentRestriction.EndTimeLimitation.MaxTime =
                _studentRestriction.EndTimeLimitation.MaxTime.Value.Add(new TimeSpan(10, 0, 0));
            _studentRestrictions.Add(secondStudentRestriction);

            _target.SetStudentAvailabilityRestrictions(_studentRestrictions);

            Assert.That(_target.StartTimeLimitation, Is.EqualTo(_studentRestrictions[0].StartTimeLimitation.MinTime));
            if (_target.EndTimeLimitation != null && _studentRestrictions[0].EndTimeLimitation.MaxTime != null)
                Assert.That(_target.EndTimeLimitation.Value.Hours, Is.EqualTo(_studentRestrictions[0].EndTimeLimitation.MaxTime.Value.Hours));
            Assert.That(_target.SecondStartTimeLimitation, Is.EqualTo(_studentRestrictions[1].StartTimeLimitation.MinTime));
            if (_target.SecondEndTimeLimitation != null && _studentRestrictions[1].EndTimeLimitation.MaxTime != null)
                Assert.That(_target.SecondEndTimeLimitation.Value.Hours, Is.EqualTo(_studentRestrictions[1].EndTimeLimitation.MaxTime.Value.Hours));
        }

        [Test]
        public void ShouldSetValuesToStudentAvailabilityRestrictions()
        {
            var secondStudentRestriction = new StudentAvailabilityRestriction();
            ITimeLimitationValidator validator = new TimeOfDayValidator(false);
            secondStudentRestriction.StartTimeLimitation = new TimeLimitation(validator);
            secondStudentRestriction.EndTimeLimitation = new TimeLimitation(validator);
            if (_studentRestriction.StartTimeLimitation.MinTime != null)
                secondStudentRestriction.StartTimeLimitation.MinTime =
                    _studentRestriction.StartTimeLimitation.MinTime.Value.Add(new TimeSpan(2, 0, 0));
            if (_studentRestriction.EndTimeLimitation.MaxTime != null)
                secondStudentRestriction.EndTimeLimitation.MaxTime =
                    _studentRestriction.EndTimeLimitation.MaxTime.Value.Add(new TimeSpan(4, 0, 0));
            _studentRestrictions.Add(secondStudentRestriction);

            _target.SetStudentAvailabilityRestrictions(_studentRestrictions);

            _target.StartTimeLimitation = TimeSpan.FromHours(7);
            _target.EndTimeLimitation = TimeSpan.FromHours(15);
            _target.EndTimeLimitationNextDay = false;
            _target.SecondStartTimeLimitation = TimeSpan.FromHours(9);
            _target.SecondEndTimeLimitation = TimeSpan.FromHours(19);
            _target.SecondEndTimeLimitationNextDay = false;

            _target.SetValuesToStudentAvailabilityRestrictions();

            Assert.That(_studentRestrictions[0].StartTimeLimitation.MinTime, Is.EqualTo(_target.StartTimeLimitation));
            Assert.That(_studentRestrictions[0].EndTimeLimitation.MaxTime, Is.EqualTo(_target.EndTimeLimitation));
            Assert.That(_studentRestrictions[1].StartTimeLimitation.MinTime, Is.EqualTo(_target.SecondStartTimeLimitation));
            Assert.That(_studentRestrictions[1].EndTimeLimitation.MaxTime, Is.EqualTo(_target.SecondEndTimeLimitation));
        }
    }
}