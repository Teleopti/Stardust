using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
    [TestFixture]
    public class RequestTransitionsTest
    {
        private IPersonRequest _target;
        private MockRepository mocks;
        private IPersonRequestCheckAuthorization _authorization;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            _authorization = mocks.DynamicMock<IPersonRequestCheckAuthorization>();
            _target = new PersonRequestFactory().CreateNewPersonRequest();
        }

        [Test]
        public void CanMoveFromNewToPending()
        {
            Assert.IsTrue(_target.IsNew);
            Assert.IsFalse(_target.IsApproved);
            Assert.IsFalse(_target.IsDenied);
            Assert.IsFalse(_target.IsPending);
            _target.Pending();
            Assert.IsFalse(_target.IsNew);
            Assert.IsFalse(_target.IsApproved);
            Assert.IsFalse(_target.IsDenied);
            Assert.IsTrue(_target.IsPending);
        }

        [Test]
        public void CanMoveFromNewToDenied()
        {
            using (mocks.Record())
            {
            }
            using (mocks.Playback())
            {
                Assert.IsTrue(_target.IsNew);
                _target.Deny(null, null, _authorization);
                Assert.IsTrue(_target.IsDenied);
            }
        }

        [Test]
        public void CanMoveFromPendingToApproved()
        {
            using (mocks.Record())
            {
            }
            using (mocks.Playback())
            {
                _target.Pending();
                Assert.IsTrue(_target.IsPending);
                _target.Approve(new ApprovalServiceForTest(),_authorization);
                Assert.IsTrue(_target.IsApproved);
            }
        }

        [Test]
        public void CanMoveFromPendingToNew()
        {
            using (mocks.Record())
            {
            }
            using (mocks.Playback())
            {
                _target.Pending();
                Assert.IsTrue(_target.IsPending);
                _target.SetNew();
                Assert.IsTrue(_target.IsNew);
            }
        }

        [Test]
        public void CanMoveFromPendingToDenied()
        {
            using (mocks.Record())
            {
            }
            using (mocks.Playback())
            {
                _target.Pending();
                Assert.IsTrue(_target.IsPending);
                _target.Deny(null, null, _authorization);
                Assert.IsTrue(_target.IsDenied);
            }
        }

        [Test, ExpectedException(typeof(InvalidRequestStateTransitionException))]
        public void CannotMoveFromNewToApproved()
        {
            using (mocks.Record())
            {
            }
            using (mocks.Playback())
            {
                Assert.IsTrue(_target.IsNew);
                _target.Approve(new ApprovalServiceForTest(),_authorization);
            }
        }

        [Test, ExpectedException(typeof(InvalidRequestStateTransitionException))]
        public void CannotMoveFromApprovedToDenied()
        {
            using (mocks.Record())
            {
                Expect.Call(()=>_authorization.VerifyEditRequestPermission(_target)).Repeat.AtLeastOnce();
            }
            using (mocks.Playback())
            {
                Assert.IsTrue(_target.IsNew);
                _target.Pending();
                Assert.IsTrue(_target.IsPending);
                _target.Approve(new ApprovalServiceForTest(), _authorization);
                Assert.IsTrue(_target.IsApproved);
                _target.Deny(null, null, _authorization);
            }
        }
    }
}
