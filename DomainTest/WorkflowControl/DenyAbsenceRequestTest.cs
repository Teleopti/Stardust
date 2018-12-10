using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
    [TestFixture]
    public class DenyAbsenceRequestTest
    {
        private IProcessAbsenceRequest _target;
        private DateTimePeriod _period;
        private IAbsence _absence;
        private IPersonRequest _personRequest;
        private MockRepository _mocks;
        private IAbsenceRequest _absenceRequest;
        private IPersonRequestCheckAuthorization _authorization;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _period = new DateTimePeriod(2010, 4, 21, 2010, 4, 22);
            _absence = AbsenceFactory.CreateAbsence("Holiday");
            _absenceRequest = new PersonRequestFactory().CreateNewAbsenceRequest(_absence, _period);
            _personRequest = (IPersonRequest)_absenceRequest.Parent;
            _authorization = _mocks.StrictMock<IPersonRequestCheckAuthorization>();
            _target = new DenyAbsenceRequest();
        }

        [Test]
        public void VerifyDenyRequestIfNotValid()
        {
            using (_mocks.Record())
            {
                _authorization.VerifyEditRequestPermission(_personRequest);
            }
            using (_mocks.Playback())
            {
                Assert.IsTrue(_personRequest.IsNew);

                _target.Process(_absenceRequest, new RequiredForProcessingAbsenceRequest(null,null,_authorization), new RequiredForHandlingAbsenceRequest(), null);

                Assert.IsTrue(_personRequest.IsDenied);
            }
        }

        [Test]
        public void VerifyGrantRequestIfValid()
        {
            using (_mocks.Record())
            {
                _authorization.VerifyEditRequestPermission(_personRequest);
            }
            using (_mocks.Playback())
            {
                Assert.IsTrue(_personRequest.IsNew);

                _target.Process(_absenceRequest, new RequiredForProcessingAbsenceRequest(null, null, _authorization), new RequiredForHandlingAbsenceRequest(), null);

                Assert.IsTrue(_personRequest.IsDenied);
            }
        }

        [Test]
        public void VerifyDenyRequestAndRollbackIfNotValid()
        {
            IUndoRedoContainer undoRedoContainer = _mocks.StrictMock<IUndoRedoContainer>();
            
            using (_mocks.Record())
            {
                _authorization.VerifyEditRequestPermission(_personRequest);
                undoRedoContainer.UndoAll();
            }

            _mocks.ReplayAll();
            Assert.IsTrue(_personRequest.IsNew);

            _target.Process(_absenceRequest, new RequiredForProcessingAbsenceRequest(undoRedoContainer, null, _authorization), new RequiredForHandlingAbsenceRequest(), null);

            Assert.IsTrue(_personRequest.IsDenied);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyRollbackAndPendRequestIfValid()
        {
            IUndoRedoContainer undoRedoContainer = _mocks.StrictMock<IUndoRedoContainer>();

            using (_mocks.Record())
            {
                _authorization.VerifyEditRequestPermission(_personRequest);
                undoRedoContainer.UndoAll();
            }

            _mocks.ReplayAll();
            Assert.IsTrue(_personRequest.IsNew);

            _target.Process(_absenceRequest, new RequiredForProcessingAbsenceRequest(undoRedoContainer,null,_authorization), new RequiredForHandlingAbsenceRequest(), null);

            Assert.IsTrue(_personRequest.IsDenied);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanCreateInstance()
        {
            var newInstance = _target.CreateInstance();
            Assert.IsTrue(typeof(DenyAbsenceRequest).IsInstanceOfType(newInstance));
            Assert.AreEqual(UserTexts.Resources.Deny,_target.DisplayText);
        }

        [Test]
        public void VerifyEquals()
        {
            var otherProcessOfSameKind = new DenyAbsenceRequest();
            var otherProcess = new GrantAbsenceRequest();

            Assert.IsTrue(otherProcessOfSameKind.Equals(_target));
            Assert.IsFalse(_target.Equals(otherProcess));
        }

        [Test]
        public void VerifyCanSetOtherDenyReason()
        {
            using (_mocks.Record())
            {
                _authorization.VerifyEditRequestPermission(_personRequest);
            }
            using (_mocks.Playback())
            {
                var denyAbsenceRequest = (DenyAbsenceRequest) _target;
                Assert.AreEqual("RequestDenyReasonAutodeny", denyAbsenceRequest.DenyReason);
                denyAbsenceRequest.DenyReason = "MyKeyForAnotherReason";

                _target.Process(_absenceRequest, new RequiredForProcessingAbsenceRequest(null, null, _authorization), new RequiredForHandlingAbsenceRequest(), null);

                Assert.IsTrue(_personRequest.IsDenied);
                Assert.AreEqual("MyKeyForAnotherReason", _personRequest.DenyReason);

                DenyAbsenceRequest copy = (DenyAbsenceRequest) denyAbsenceRequest.CreateInstance();
                Assert.AreEqual("MyKeyForAnotherReason", copy.DenyReason);
            }
        }

        [Test]
        public void ShouldGetHashCodeInReturn()
        {
            var result = _target.GetHashCode();
            Assert.IsNotNull(result);

        }
    }
}
