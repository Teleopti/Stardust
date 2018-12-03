using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
    public class PendingAbsenceRequestTest
    {
        private IProcessAbsenceRequest _target;
        private DateTimePeriod _period;
        private IAbsence _absence;
        private IPersonRequest _personRequest;
        private IList<IAbsenceRequestValidator> _validators;
        private MockRepository _mocks;
        private IAbsenceRequestValidator _validator;
        private IAbsenceRequest _absenceRequest;
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
            _validators = new List<IAbsenceRequestValidator>{_validator};
            _validatedRequest = new ValidatedRequest{IsValid = true, ValidationErrors = ""};
            _authorization = _mocks.StrictMock<IPersonRequestCheckAuthorization>();
            _target = new PendingAbsenceRequest();
        }

        [Test]
        public void VerifyDenyRequestIfNotValid()
        {
            _validatedRequest.IsValid = false;
           
            var handling = new RequiredForHandlingAbsenceRequest();
            using (_mocks.Record())
            {
                Expect.Call(_validator.Validate(_absenceRequest,handling)).Return(_validatedRequest);
                
                _authorization.VerifyEditRequestPermission(_personRequest);
            }

            Assert.IsTrue(_personRequest.IsNew);

            _target.Process(_absenceRequest, new RequiredForProcessingAbsenceRequest(null,null,_authorization), handling, _validators);

            Assert.IsTrue(_personRequest.IsDenied);
        }

        [Test]
        public void VerifyGrantRequestIfValid()
        {
            _validatedRequest.IsValid = true;
            _validatedRequest.ValidationErrors = "";

            var handling = new RequiredForHandlingAbsenceRequest();
            using (_mocks.Record())
            {
                Expect.Call(_validator.Validate(_absenceRequest, handling)).Return(_validatedRequest);
                _authorization.VerifyEditRequestPermission(_personRequest);
            }

            Assert.IsTrue(_personRequest.IsNew);

            _target.Process(_absenceRequest, new RequiredForProcessingAbsenceRequest(null,null,_authorization), handling, _validators);

            Assert.IsTrue(_personRequest.IsPending);
        }

        [Test]
        public void VerifyDenyRequestAndRollbackIfNotValid()
        {
            IUndoRedoContainer undoRedoContainer = _mocks.StrictMock<IUndoRedoContainer>();
            _validatedRequest.IsValid = false;
            _validatedRequest.ValidationErrors = "KeyForInvalidRequest";

            var handling = new RequiredForHandlingAbsenceRequest();
            using (_mocks.Ordered())
            {
                Expect.Call(_validator.Validate(_absenceRequest, handling)).Return(_validatedRequest);
                undoRedoContainer.UndoAll();
                Expect.Call(() => _authorization.VerifyEditRequestPermission(_personRequest));
            }

            _mocks.ReplayAll();
            Assert.IsTrue(_personRequest.IsNew);

            _target.Process(_absenceRequest, new RequiredForProcessingAbsenceRequest(undoRedoContainer,null,_authorization), handling, _validators);

            Assert.IsTrue(_personRequest.IsDenied);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyRollbackAndPendRequestIfValid()
        {
            IUndoRedoContainer undoRedoContainer = _mocks.StrictMock<IUndoRedoContainer>();
            _validatedRequest.IsValid = true;
            _validatedRequest.ValidationErrors = "";

            var handling = new RequiredForHandlingAbsenceRequest();
            using (_mocks.Ordered())
            {
                Expect.Call(_validator.Validate(_absenceRequest, handling)).Return(_validatedRequest);
                undoRedoContainer.UndoAll();
            }

            _mocks.ReplayAll();
            Assert.IsTrue(_personRequest.IsNew);

            _target.Process(_absenceRequest, new RequiredForProcessingAbsenceRequest(undoRedoContainer, null, _authorization), handling, _validators);

            Assert.IsTrue(_personRequest.IsPending);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanCreateInstance()
        {
            var newInstance = _target.CreateInstance();
            Assert.IsTrue(typeof(PendingAbsenceRequest).IsInstanceOfType(newInstance));
            Assert.AreEqual(UserTexts.Resources.No,_target.DisplayText);
        }

        [Test]
        public void VerifyEquals()
        {
            var otherProcessOfSameKind = new PendingAbsenceRequest();
            var otherProcess = new GrantAbsenceRequest();

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
