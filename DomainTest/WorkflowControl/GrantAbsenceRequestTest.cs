using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
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
            _target = new GrantAbsenceRequest();
            _target.RequestApprovalService = _requestApprovalService;
        }

        [Test]
        public void VerifyDenyRequestIfNotValid()
        {
            using (_mocks.Record())
            {
                Expect.Call(_validator.Validate(_absenceRequest)).Return(false);
                Expect.Call(_validator.InvalidReason).Return("KeyForInvalidRequest");
                _authorization.VerifyEditRequestPermission(_personRequest);
            }

            Assert.IsTrue(_personRequest.IsNew);

            _target.Process(null, _absenceRequest, _authorization, _validators);

            Assert.IsTrue(_personRequest.IsDenied);
        }

        [Test]
        public void VerifyRollbackBeforeDenyRequestIfNotValidAndUndoRedoContainerSet()
        {
            IUndoRedoContainer undoRedoContainer = _mocks.StrictMock<IUndoRedoContainer>();
            using (_mocks.Ordered())
            {
                Expect.Call(_validator.Validate(_absenceRequest)).Return(false);
                Expect.Call(_validator.InvalidReason).Return("KeyForInvalidRequest");
                undoRedoContainer.UndoAll();
                Expect.Call(() => _authorization.VerifyEditRequestPermission(_personRequest));
            }

            _mocks.ReplayAll();
            Assert.IsTrue(_personRequest.IsNew);

            _target.UndoRedoContainer = undoRedoContainer;
            _target.Process(null, _absenceRequest, _authorization, _validators);

            Assert.IsTrue(_personRequest.IsDenied);
            _mocks.VerifyAll();
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void VerifyRequestApprovalServiceIsSet()
        {
            _target.RequestApprovalService = null;
            _target.Process(null, _absenceRequest, _authorization,_validators);
        }

        [Test]
        public void VerifyGrantRequestIfValid()
        {
            using (_mocks.Record())
            {
                Expect.Call(_validator.Validate(_absenceRequest)).Return(true);
                Expect.Call(_requestApprovalService.ApproveAbsence(_absence, _period, _absenceRequest.Person)).Return(
                    new List<IBusinessRuleResponse>());
                _authorization.VerifyEditRequestPermission(_personRequest);
            }

            Assert.IsTrue(_personRequest.IsNew);

            _target.Process(null, _absenceRequest,_authorization, _validators);

            Assert.IsTrue(_personRequest.IsApproved);
        }

        [Test]
        public void VerifyRollbackAndGrantRequestIfValid()
        {
            IUndoRedoContainer undoRedoContainer = _mocks.StrictMock<IUndoRedoContainer>();
            using (_mocks.Ordered())
            {
                Expect.Call(_validator.Validate(_absenceRequest)).Return(true);
                undoRedoContainer.UndoAll();
                _authorization.VerifyEditRequestPermission(_personRequest);
                Expect.Call(_requestApprovalService.ApproveAbsence(_absence, _period, _absenceRequest.Person)).Return(
                    new List<IBusinessRuleResponse>());
            }

            _mocks.ReplayAll();
            Assert.IsTrue(_personRequest.IsNew);

            _target.UndoRedoContainer = undoRedoContainer;
            _target.Process(null, _absenceRequest, _authorization, _validators);

            Assert.IsTrue(_personRequest.IsApproved);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanCreateInstance()
        {
            var newInstance = _target.CreateInstance();
            Assert.IsTrue(typeof(GrantAbsenceRequest).IsInstanceOfType(newInstance));
            Assert.IsNull(newInstance.RequestApprovalService);
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
    }
}
