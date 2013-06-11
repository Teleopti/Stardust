using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
    public class PersonAccountBalanceValidatorTest
    {
        private IAbsenceRequestValidator _target;
        private MockRepository _mocks;
        private IPersonAccountBalanceCalculator _personAccountBalanceCalculator;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private PersonRequestFactory _personRequestFactory;

        [SetUp]
        public void Setup()
        {
            _target = new PersonAccountBalanceValidator();
            _mocks = new MockRepository();
            DateTimePeriod schedulingDateTimePeriod = new DateTimePeriod(2010, 02, 01, 2010, 02, 28);
            _schedulingResultStateHolder = SchedulingResultStateHolderFactory.Create(schedulingDateTimePeriod);
            _personAccountBalanceCalculator = _mocks.StrictMock<IPersonAccountBalanceCalculator>();
            _personRequestFactory = new PersonRequestFactory();
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(UserTexts.Resources.Yes, _target.DisplayText);
        }

        [Test]
        public void CanValidate()
        {
            DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
            IAbsenceRequest absenceRequest = _personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod);
            
            using (_mocks.Record())
            {
                Expect.Call(
                    _personAccountBalanceCalculator.CheckBalance(
                        _schedulingResultStateHolder.Schedules[absenceRequest.Person],
                        requestedDateTimePeriod.ToDateOnlyPeriod(
                            absenceRequest.Person.PermissionInformation.DefaultTimeZone()))).Return(true);

            }

            var result = _target.Validate(absenceRequest,
                                          new RequiredForHandlingAbsenceRequest(_schedulingResultStateHolder,
                                                                                _personAccountBalanceCalculator, null,
                                                                                null, null));
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ShouldReturnValidationErrorIfNotValidatedSuccessfully()
        {
            DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
            IAbsenceRequest absenceRequest = _personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod);
            
            using (_mocks.Record())
            {
                Expect.Call(
                    _personAccountBalanceCalculator.CheckBalance(
                        _schedulingResultStateHolder.Schedules[absenceRequest.Person],
                        requestedDateTimePeriod.ToDateOnlyPeriod(
                            absenceRequest.Person.PermissionInformation.DefaultTimeZone()))).Return(false);

            }

            var result = _target.Validate(absenceRequest,
                                          new RequiredForHandlingAbsenceRequest(_schedulingResultStateHolder,
                                                                                _personAccountBalanceCalculator, null,
                                                                                null, null));
            Assert.IsFalse(result.IsValid);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void VerifySchedulingResultStateHolderCannotBeNull()
        {
            DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");

            _target.Validate(_personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod),
                             new RequiredForHandlingAbsenceRequest(null, _personAccountBalanceCalculator, null, null,
                                                                   null));
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void VerifyPersonAccountBalanceCalculatorCannotBeNull()
        {
            DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");

            _target.Validate(_personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod),
                             new RequiredForHandlingAbsenceRequest(_schedulingResultStateHolder, null, null, null, null));
        }

        [Test]
        public void VerifyReasonText()
        {
            Assert.AreEqual("RequestDenyReasonPersonAccount", _target.InvalidReason);
        }

        [Test]
        public void VerifyCanCreateNewInstance()
        {
            var newInstance = _target.CreateInstance();
            Assert.AreNotSame(_target, newInstance);
            Assert.IsTrue(typeof(PersonAccountBalanceValidator).IsInstanceOfType(newInstance));
        }

        [Test]
        public void VerifyEquals()
        {
            var otherValidatorOfSameKind = new PersonAccountBalanceValidator();
            var otherValidator = new AbsenceRequestNoneValidator();

            Assert.IsTrue(otherValidatorOfSameKind.Equals(_target));
            Assert.IsFalse(_target.Equals(otherValidator));
        }

        [Test]
        public void ShouldGetHashCodeInReturn()
        {
            var result = _target.GetHashCode();
            Assert.IsNotNull(result);
        }
    }
}
