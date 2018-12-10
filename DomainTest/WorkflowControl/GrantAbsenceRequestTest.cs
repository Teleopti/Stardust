using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;


namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
    public class GrantAbsenceRequestTest
    {
        private IProcessAbsenceRequest _target;
        private DateTimePeriod _period;
        private IAbsence _absence;
        private IPersonRequest _personRequest;
        private IList<IAbsenceRequestValidator> _validators;
        private IAbsenceRequestValidator _validator;
        private IAbsenceRequest _absenceRequest;
        private ApprovalServiceForTest _requestApprovalService;
        private IPersonRequestCheckAuthorization _authorization;

        [SetUp]
        public void Setup()
        {
            _period = new DateTimePeriod(2010, 4, 21, 2010, 4, 22);
            _absence = AbsenceFactory.CreateAbsence("Holiday");
            _absenceRequest = new PersonRequestFactory().CreateNewAbsenceRequest(_absence, _period);
            _personRequest = (IPersonRequest)_absenceRequest.Parent;
            _validator = new AbsenceRequestNoneValidator();
            _requestApprovalService = new ApprovalServiceForTest();
            _validators = new List<IAbsenceRequestValidator>{_validator};
            _authorization = new PersonRequestAuthorizationCheckerForTest();
            _target = new GrantAbsenceRequest();
        }

        [Test]
        public void VerifyDenyRequestIfNotValid()
        {
            var handling = new RequiredForHandlingAbsenceRequest();
            
            Assert.IsTrue(_personRequest.IsNew);

            _target.Process(_absenceRequest, new RequiredForProcessingAbsenceRequest(null, _requestApprovalService,_authorization), handling, new List<IAbsenceRequestValidator> {new AbsenceRequestDenyValidator() });

            Assert.IsTrue(_personRequest.IsDenied);
        }

        [Test]
        public void VerifyRollbackBeforeDenyRequestIfNotValidAndUndoRedoContainerSet()
		{
			var handling = new RequiredForHandlingAbsenceRequest();

			Assert.IsTrue(_personRequest.IsNew);

			_target.Process(_absenceRequest, new RequiredForProcessingAbsenceRequest(new UndoRedoContainer(), _requestApprovalService, _authorization), handling, new List<IAbsenceRequestValidator> { new AbsenceRequestDenyValidator() });

			Assert.IsTrue(_personRequest.IsDenied);
		}

        [Test]
        public void VerifyRequestApprovalServiceIsSet()
        {
            var handling = new RequiredForHandlingAbsenceRequest();
           Assert.Throws<ArgumentNullException>(() => _target.Process(_absenceRequest, new RequiredForProcessingAbsenceRequest(null, null, _authorization), handling, _validators));
        }

        [Test]
        public void VerifyGrantRequestIfValid()
        {
            var handling = new RequiredForHandlingAbsenceRequest();

            Assert.IsTrue(_personRequest.IsNew);

            _target.Process(_absenceRequest,new RequiredForProcessingAbsenceRequest(null,_requestApprovalService,_authorization), handling, _validators);

            Assert.IsTrue(_personRequest.IsApproved);
        }

        [Test]
        public void VerifyRollbackAndGrantRequestIfValid()
        {
            var handling = new RequiredForHandlingAbsenceRequest();
            var undoRedoContainer = new UndoRedoContainer();

            Assert.IsTrue(_personRequest.IsNew);

            _target.Process(_absenceRequest, new RequiredForProcessingAbsenceRequest(undoRedoContainer,_requestApprovalService,_authorization), handling, _validators);

            Assert.IsTrue(_personRequest.IsApproved);
        }

        [Test]
        public void VerifyRollbackAndGrantRequestIfNotValid()
        {
            var handling = new RequiredForHandlingAbsenceRequest();
            IUndoRedoContainer undoRedoContainer = new UndoRedoContainer();
            
            var person = new Person();
            var start = new DateTime(2007,1,1,0,0,0,DateTimeKind.Utc);
            var dateOnly = new DateOnly(2007, 1, 1);
            var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
            var businessRuleResponse = new BusinessRuleResponse(typeof(string), "An error has occurred!", true, false, new DateTimePeriod(start, start), person, dateOnlyPeriod, "tjillevippen");
            
			_requestApprovalService.SetBusinessRuleResponse(businessRuleResponse);
			
            Assert.IsTrue(_personRequest.IsNew);

            _target.Process(_absenceRequest, new RequiredForProcessingAbsenceRequest(undoRedoContainer, _requestApprovalService, _authorization), handling, _validators);

            Assert.IsFalse(_personRequest.IsApproved);
        }

		[Test]
		public void VerifyCallback()
		{
			var handling = new RequiredForHandlingAbsenceRequest();
			IUndoRedoContainer undoRedoContainer = new UndoRedoContainer();
			
			var person = new Person();
			var start = new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var dateOnly = new DateOnly(2007, 1, 1);
			var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var businessRuleResponse = new BusinessRuleResponse(typeof(string), "An error has occurred!", true, false, new DateTimePeriod(start, start), person, dateOnlyPeriod, "tjillevippen");
			_requestApprovalService.SetBusinessRuleResponse(businessRuleResponse);
			
			var afterCallback = false;
			_target.Process(_absenceRequest, new RequiredForProcessingAbsenceRequest(undoRedoContainer, _requestApprovalService, _authorization,
			                                                                               () =>
				                                                                               { afterCallback = true; }), handling, _validators);

			Assert.IsTrue(afterCallback);
		}		

        [Test]
        public void VerifyCanCreateInstance()
        {
            var newInstance = _target.CreateInstance();
            Assert.IsTrue(typeof(GrantAbsenceRequest).IsInstanceOfType(newInstance));
            Assert.AreEqual(UserTexts.Resources.Yes,_target.DisplayText);
        }

        [Test]
        public void VerifyEquals()
        {
            var otherProcessOfSameKind = new GrantAbsenceRequest();
            var otherProcess = new PendingAbsenceRequest();

            Assert.IsTrue(otherProcessOfSameKind.Equals(_target));
            Assert.IsFalse(_target.Equals(otherProcess));
        }

        [Test]
        public void ShouldGetHashCodeInReturn()
        {
            var result = _target.GetHashCode();
            Assert.IsNotNull(result);
        }
    }

	public class AbsenceRequestDenyValidator : IAbsenceRequestValidator
	{
		
		public IBudgetGroupHeadCountSpecification BudgetGroupHeadCountSpecification { get; set; }

		public string DisplayText => UserTexts.Resources.No;

		public IValidatedRequest Validate(IAbsenceRequest absenceRequest,
			RequiredForHandlingAbsenceRequest requiredForHandlingAbsenceRequest)
		{
			return new ValidatedRequest
			{
				IsValid = false
			};
		}

		public IAbsenceRequestValidator CreateInstance()
		{
			return new AbsenceRequestDenyValidator();
		}

		public override bool Equals(object obj)
		{
			var validator = obj as AbsenceRequestDenyValidator;
			return validator != null;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (GetType().GetHashCode());
				result = (result * 397) ^ (BudgetGroupHeadCountSpecification != null ? BudgetGroupHeadCountSpecification.GetHashCode() : 0);
				return result;
			}
		}
	}
}
