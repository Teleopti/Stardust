using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Absence;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class AbsenceRequestValidatorProviderTest
	{
		private readonly IAbsence _absence = AbsenceFactory.CreateAbsence("holiday");

		[Test]
		public void ShouldStaffingThresholdValidatorByIntradayValidatorFlag()
		{
			var staffingThresholdValidator = getStaffingThresholdValidator(false);
			Assert.IsNotNull(staffingThresholdValidator);
			Assert.IsTrue(staffingThresholdValidator.GetType() == typeof(StaffingThresholdValidator));
		}

		[Test]
		public void ShouldGetStaffingThresholdWithShrinkageValidatorByIntradayValidatorFlag()
		{
			var staffingThresholdValidator = getStaffingThresholdValidator(true);
			Assert.IsNotNull(staffingThresholdValidator);
			Assert.IsTrue(staffingThresholdValidator.GetType() == typeof(StaffingThresholdWithShrinkageValidator));
		}

		[Test]
		public void ShouldGetStaffingThresholdValidatorWhenCheckStaffingIsNoByIntradayValidatorFlag()
		{
			var workflowControlSet = WorkflowControlSetFactory.CreateWorkFlowControlSet(_absence, new GrantAbsenceRequest(), false);
			workflowControlSet.AbsenceRequestOpenPeriods[0].StaffingThresholdValidator = new AbsenceRequestNoneValidator();
			var staffingThresholdValidator = getStaffingThresholdValidator(false, workflowControlSet);
			Assert.IsNotNull(staffingThresholdValidator);
			Assert.IsTrue(staffingThresholdValidator.GetType() == typeof(StaffingThresholdValidator));
		}

		private IAbsenceRequestValidator getStaffingThresholdValidator(bool useShrinkage,
			IWorkflowControlSet workflowControlSet = null)
		{
			var absenceRequestValidatorProvider = new AbsenceRequestValidatorProvider(null);

			var person = createPerson();
			person.WorkflowControlSet = workflowControlSet ?? createWorkflowControlSet(_absence, useShrinkage);
			var personRequest = createPersonRequest(person, _absence);

			var validatorList = absenceRequestValidatorProvider.GetValidatorList(personRequest,
				RequestValidatorsFlag.IntradayValidator);
			return validatorList.FirstOrDefault();
		}

		private IPersonRequest createPersonRequest(IPerson person, IAbsence absence)
		{
			var personRequestFactory = new PersonRequestFactory();
			var personRequest = personRequestFactory.CreatePersonRequest(person).WithId();
			var absenceRequest = personRequestFactory.CreateAbsenceRequest(absence,
				new DateTimePeriod(DateTime.Now.AddDays(1).Date.ToUniversalTime(), DateTime.Now.AddDays(2).Date.ToUniversalTime()))
				.WithId();
			personRequest.Request = absenceRequest;
			absenceRequest.SetParent(personRequest);
			return personRequest;
		}

		private IPerson createPerson()
		{
			var person = PersonFactory.CreatePerson().WithId();
			return person;
		}

		private IWorkflowControlSet createWorkflowControlSet(IAbsence absence, bool useShrinkage)
		{
			var workflowControlSet = WorkflowControlSetFactory.CreateWorkFlowControlSet(absence, new GrantAbsenceRequest(), false);
			workflowControlSet.AbsenceRequestOpenPeriods[0].StaffingThresholdValidator = !useShrinkage
				? new StaffingThresholdValidator()
				: new StaffingThresholdWithShrinkageValidator();
			return workflowControlSet;
		}
	}
}
