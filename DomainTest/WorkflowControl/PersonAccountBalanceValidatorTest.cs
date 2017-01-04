﻿using System;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
	[DomainTest]
	public class PersonAccountBalanceValidatorTest : ISetup
    {
        private IAbsenceRequestValidator _target;
        private IPersonAccountBalanceCalculator _personAccountBalanceCalculator;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private PersonRequestFactory _personRequestFactory;

	    public MutableNow Now;

        [SetUp]
        public void Setup()
        {
            _target = new PersonAccountBalanceValidator();
            DateTimePeriod schedulingDateTimePeriod = new DateTimePeriod(2010, 02, 01, 2010, 02, 28);
            _schedulingResultStateHolder = SchedulingResultStateHolderFactory.Create(schedulingDateTimePeriod);
            _personAccountBalanceCalculator = MockRepository.GenerateMock<IPersonAccountBalanceCalculator>();
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
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
            IAbsenceRequest absenceRequest = _personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod);

			_personAccountBalanceCalculator.Stub(x => x.CheckBalance(
				_schedulingResultStateHolder.Schedules[absenceRequest.Person],
				requestedDateTimePeriod.ToDateOnlyPeriod(
					absenceRequest.Person.PermissionInformation.DefaultTimeZone()))).Return(true);
			
            var result = _target.Validate(absenceRequest,
                                          new RequiredForHandlingAbsenceRequest(_schedulingResultStateHolder,
                                                                                _personAccountBalanceCalculator, null,
                                                                                null, null));
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ShouldReturnValidationErrorIfNotValidatedSuccessfully()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
            IAbsenceRequest absenceRequest = _personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod);

			_personAccountBalanceCalculator.Stub(x => x.CheckBalance(
				_schedulingResultStateHolder.Schedules[absenceRequest.Person],
				requestedDateTimePeriod.ToDateOnlyPeriod(
					absenceRequest.Person.PermissionInformation.DefaultTimeZone()))).Return(false);
			
            var result = _target.Validate(absenceRequest,
                                          new RequiredForHandlingAbsenceRequest(_schedulingResultStateHolder,
                                                                                _personAccountBalanceCalculator, null,
                                                                                null, null));
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void VerifySchedulingResultStateHolderCannotBeNull()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");

			Assert.Throws<ArgumentNullException>(() => _target.Validate(_personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod),
                             new RequiredForHandlingAbsenceRequest(null, _personAccountBalanceCalculator, null, null,
                                                                   null)));
        }

        [Test]
        public void VerifyPersonAccountBalanceCalculatorCannotBeNull()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			DateTimePeriod requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 1);
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");

            Assert.Throws<ArgumentNullException>(() => _target.Validate(_personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod),
							 new RequiredForHandlingAbsenceRequest(_schedulingResultStateHolder, null, null, null, null)));
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

		[Test]
	    public void ShouldGetDenyReasonForSpecifiedCulture()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var requestedDateTimePeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2010, 02, 01, 0, 0, 0, DateTimeKind.Utc), 100);
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var absenceRequest = _personRequestFactory.CreateAbsenceRequest(absence, requestedDateTimePeriod);
			var languageCulture = CultureInfo.GetCultureInfo("zh-CN");
			absenceRequest.Person.PermissionInformation.SetUICulture(languageCulture);

			_personAccountBalanceCalculator.Stub(x => x.CheckBalance(
				_schedulingResultStateHolder.Schedules[absenceRequest.Person],
				requestedDateTimePeriod.ToDateOnlyPeriod(
					absenceRequest.Person.PermissionInformation.DefaultTimeZone()))).Return(false);
			
			var result = _target.Validate(absenceRequest,
										  new RequiredForHandlingAbsenceRequest(_schedulingResultStateHolder,
																				_personAccountBalanceCalculator, null,
																				null, null));

			var expect = UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonPersonAccount", languageCulture);

			Assert.IsTrue(expect.Equals(result.ValidationErrors));
	    }

	    [Test]
		public void ShouldGetWaitlistedInvalidReason()
	    {
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = new Absence().WithId();
			var person = PersonFactory.CreatePersonWithId();
			person.WorkflowControlSet = WorkflowControlSetFactory.CreateWorkFlowControlSet(absence, new GrantAbsenceRequest(),
				true);
			var period = new DateTimePeriod(2016, 9, 9, 9, 2016, 9, 9, 17);
			var absenceRequest = new AbsenceRequest(absence, period);
			var personRequest = new PersonRequest(person, absenceRequest);

			var schedulingResultStateHolder = new SchedulingResultStateHolder();
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			var absenceLayer = new AbsenceLayer(absence, period);
			var personAbsence = new PersonAbsence(person, scenario, absenceLayer).WithId();

		    var scheduleDictionary = new ScheduleDictionaryForTest(scenario, period);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(person,
				scenario, period);
			scheduleDictionary.AddPersonAssignment(assignment);
		    scheduleDictionary.AddPersonAbsence(personAbsence);
			schedulingResultStateHolder.Schedules = scheduleDictionary;

			var accountDay = AbsenceAccountFactory.CreateAbsenceAccountDays(person, absence, new DateOnly(2016, 1, 1),
				TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);
			var personAccountBalanceCalculator = new PersonAccountBalanceCalculator(new[] {accountDay});
			var requiredForHandlingAbsenceRequest = new RequiredForHandlingAbsenceRequest(schedulingResultStateHolder,
				personAccountBalanceCalculator, null, null);

			var result = _target.Validate(absenceRequest, requiredForHandlingAbsenceRequest);

			Assert.IsTrue(_target.InvalidReason.Equals("RequestWaitlistedReasonPersonAccount"));
			Assert.IsFalse(result.IsValid);

			var errorMessage = Resources.RequestWaitlistedReasonPersonAccount;
			Assert.IsTrue(result.ValidationErrors.Equals(errorMessage));
		}

	    public void Setup(ISystem system, IIocConfiguration configuration)
	    {
	    }
    }
}
