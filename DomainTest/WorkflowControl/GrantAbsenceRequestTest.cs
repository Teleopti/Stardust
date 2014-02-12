using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

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
        private MockRepository _mocks;
        private IAbsenceRequestValidator _validator;
        private IAbsenceRequest _absenceRequest;
        private IRequestApprovalService _requestApprovalService;
        private IPersonRequestCheckAuthorization _authorization;
        private IValidatedRequest _validatedRequest;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _period = new DateTimePeriod(2010, 4, 21, 2010, 4, 22);
            _absence = AbsenceFactory.CreateAbsence("Holiday");
            _absenceRequest = new PersonRequestFactory().CreateNewAbsenceRequest(_absence, _period);
            _personRequest = (IPersonRequest)_absenceRequest.Parent;
            _validator = _mocks.StrictMock<IAbsenceRequestValidator>();
            _requestApprovalService = _mocks.StrictMock<IRequestApprovalService>();
            _validators = new List<IAbsenceRequestValidator>{_validator};
            _authorization = _mocks.StrictMock<IPersonRequestCheckAuthorization>();
            _validatedRequest = new ValidatedRequest {IsValid = true, ValidationErrors = ""};
            _target = new GrantAbsenceRequest();
        }

        [Test]
        public void VerifyDenyRequestIfNotValid()
        {
            var handling = new RequiredForHandlingAbsenceRequest();
            using (_mocks.Record())
            {
                _validatedRequest.IsValid = false;
                _validatedRequest.ValidationErrors = "KeyForInvalidRequest";
                Expect.Call(_validator.Validate(_absenceRequest, handling)).IgnoreArguments().Return(_validatedRequest);
                Expect.Call(_validator.InvalidReason).Return("KeyForInvalidRequest");
                _authorization.VerifyEditRequestPermission(_personRequest);
            }

            Assert.IsTrue(_personRequest.IsNew);

            _target.Process(null, _absenceRequest, new RequiredForProcessingAbsenceRequest(null,_requestApprovalService,_authorization), handling, _validators);

            Assert.IsTrue(_personRequest.IsDenied);
        }

        [Test]
        public void VerifyRollbackBeforeDenyRequestIfNotValidAndUndoRedoContainerSet()
        {
            var handling = new RequiredForHandlingAbsenceRequest();
            IUndoRedoContainer undoRedoContainer = _mocks.StrictMock<IUndoRedoContainer>();
            _validatedRequest.IsValid = false;
            _validatedRequest.ValidationErrors = "KeyForInvalidRequest";

            using (_mocks.Ordered())
            {
                Expect.Call(_validator.Validate(_absenceRequest,handling)).Return(_validatedRequest);
                undoRedoContainer.UndoAll();
                Expect.Call(() => _authorization.VerifyEditRequestPermission(_personRequest));
            }

            _mocks.ReplayAll();
            Assert.IsTrue(_personRequest.IsNew);

            _target.Process(null, _absenceRequest, new RequiredForProcessingAbsenceRequest(undoRedoContainer,_requestApprovalService,_authorization), handling, _validators);

            Assert.IsTrue(_personRequest.IsDenied);
            _mocks.VerifyAll();
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void VerifyRequestApprovalServiceIsSet()
        {
            var handling = new RequiredForHandlingAbsenceRequest();
            _target.Process(null, _absenceRequest, new RequiredForProcessingAbsenceRequest(null,null,_authorization), handling, _validators);
        }

        [Test]
        public void VerifyGrantRequestIfValid()
        {
            var handling = new RequiredForHandlingAbsenceRequest();
            _validatedRequest.IsValid = true;
            _validatedRequest.ValidationErrors = "";
            using (_mocks.Record())
            {
                Expect.Call(_validator.Validate(_absenceRequest,handling)).Return(_validatedRequest);
                Expect.Call(_requestApprovalService.ApproveAbsence(_absence, _period, _absenceRequest.Person)).Return(
                    new List<IBusinessRuleResponse>());
                _authorization.VerifyEditRequestPermission(_personRequest);
            }

            Assert.IsTrue(_personRequest.IsNew);

            _target.Process(null, _absenceRequest,new RequiredForProcessingAbsenceRequest(null,_requestApprovalService,_authorization), handling, _validators);

            Assert.IsTrue(_personRequest.IsApproved);
        }

        [Test]
        public void VerifyRollbackAndGrantRequestIfValid()
        {
            var handling = new RequiredForHandlingAbsenceRequest();
            IUndoRedoContainer undoRedoContainer = _mocks.StrictMock<IUndoRedoContainer>();
            _validatedRequest.IsValid = true;
            _validatedRequest.ValidationErrors = "";

            using (_mocks.Ordered())
            {
                Expect.Call(_validator.Validate(_absenceRequest, handling)).Return(_validatedRequest);
                undoRedoContainer.UndoAll();
                _authorization.VerifyEditRequestPermission(_personRequest);
                Expect.Call(_requestApprovalService.ApproveAbsence(_absence, _period, _absenceRequest.Person)).Return(
                    new List<IBusinessRuleResponse>());
            }

            _mocks.ReplayAll();
            Assert.IsTrue(_personRequest.IsNew);

            _target.Process(null, _absenceRequest, new RequiredForProcessingAbsenceRequest(undoRedoContainer,_requestApprovalService,_authorization), handling, _validators);

            Assert.IsTrue(_personRequest.IsApproved);
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Scheduling.Rules.BusinessRuleResponse.#ctor(System.Type,System.String,System.Boolean,System.Boolean,Teleopti.Interfaces.Domain.DateTimePeriod,Teleopti.Interfaces.Domain.IPerson,Teleopti.Interfaces.Domain.DateOnlyPeriod)"), Test]
        public void VerifyRollbackAndGrantRequestIfNotValid()
        {
            var handling = new RequiredForHandlingAbsenceRequest();
            IUndoRedoContainer undoRedoContainer = _mocks.StrictMock<IUndoRedoContainer>();
            _validatedRequest.IsValid = true;
            _validatedRequest.ValidationErrors = "";


            var person = new Person();
            var start = new DateTime(2007,1,1,0,0,0,DateTimeKind.Utc);
            var dateOnly = new DateOnly(2007, 1, 1);
            var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
            var businessRuleResponse = new BusinessRuleResponse(typeof(string), "An error has occurred!", true, false, new DateTimePeriod(start, start), person, dateOnlyPeriod);
            

            using (_mocks.Ordered())
            {
                Expect.Call(_validator.Validate(_absenceRequest, handling)).Return(_validatedRequest);
                undoRedoContainer.UndoAll();
                _authorization.VerifyEditRequestPermission(_personRequest);
                Expect.Call(_requestApprovalService.ApproveAbsence(_absence, _period, _absenceRequest.Person)).Return(
                    new List<IBusinessRuleResponse>{businessRuleResponse});
            }

            _mocks.ReplayAll();
            Assert.IsTrue(_personRequest.IsNew);

            _target.Process(null, _absenceRequest, new RequiredForProcessingAbsenceRequest(undoRedoContainer, _requestApprovalService, _authorization), handling, _validators);

            Assert.IsFalse(_personRequest.IsApproved);
            _mocks.VerifyAll();
        }

		[Test]
		public void VerifyCallback()
		{
			var handling = new RequiredForHandlingAbsenceRequest();
			IUndoRedoContainer undoRedoContainer = _mocks.DynamicMock<IUndoRedoContainer>();
			_validatedRequest.IsValid = true;
			_validatedRequest.ValidationErrors = "";

			var person = new Person();
			var start = new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var dateOnly = new DateOnly(2007, 1, 1);
			var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var businessRuleResponse = new BusinessRuleResponse(typeof(string), "An error has occurred!", true, false, new DateTimePeriod(start, start), person, dateOnlyPeriod);


			using (_mocks.Ordered())
			{
				Expect.Call(_validator.Validate(_absenceRequest, handling)).Return(_validatedRequest);
				undoRedoContainer.UndoAll();
				_authorization.VerifyEditRequestPermission(_personRequest);
				Expect.Call(_requestApprovalService.ApproveAbsence(_absence, _period, _absenceRequest.Person)).Return(
					new List<IBusinessRuleResponse> { businessRuleResponse });
			}

			_mocks.ReplayAll();

			var afterCallback = false;
			_target.Process(null, _absenceRequest, new RequiredForProcessingAbsenceRequest(undoRedoContainer, _requestApprovalService, _authorization,
			                                                                               () =>
				                                                                               { afterCallback = true; }), handling, _validators);

			Assert.IsTrue(afterCallback);
			_mocks.VerifyAll();
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
}
