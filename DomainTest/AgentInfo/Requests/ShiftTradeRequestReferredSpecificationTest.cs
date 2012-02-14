using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
    [TestFixture]
    public class ShiftTradeRequestReferredSpecificationTest
    {
        private ShiftTradeRequestReferredSpecification _target;
        private MockRepository _mockRepository;
        private IPersonRequest _personRequest;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            IShiftTradeRequestStatusChecker shiftTradeRequestStatusChecker = new ShiftTradeRequestStatusCheckerForTestDoesNothing();
            _target = new ShiftTradeRequestReferredSpecification(shiftTradeRequestStatusChecker);
            _personRequest = _mockRepository.StrictMock<IPersonRequest>();
        }

        [Test]
        public void VerifyNullReturnsFalse()
        {
            Assert.IsFalse(_target.IsSatisfiedBy(null));
        }

        [Test]
        public void VerifyAbsenceRequestReturnsFalse()
        {
            IAbsenceRequest absenceRequest = _mockRepository.StrictMock<IAbsenceRequest>();
            Expect.Call(_personRequest.Request).Return(absenceRequest);

            _mockRepository.ReplayAll();
            Assert.IsFalse(_target.IsSatisfiedBy(_personRequest));
            _mockRepository.VerifyAll();
        }

        [Test]
        public void VerifyShiftTradeRequestOkByBothPartsReturnsFalse()
        {
            IShiftTradeRequest shiftTradeRequest = _mockRepository.StrictMock<IShiftTradeRequest>();
            Expect.Call(_personRequest.Request).Return(shiftTradeRequest);
            Expect.Call(shiftTradeRequest.GetShiftTradeStatus(null)).IgnoreArguments().Return(ShiftTradeStatus.OkByBothParts);

            _mockRepository.ReplayAll();
            Assert.IsFalse(_target.IsSatisfiedBy(_personRequest));
            _mockRepository.VerifyAll();
        }

        [Test]
        public void VerifyShiftTradeRequestReferredReturnsTrue()
        {
            IShiftTradeRequest shiftTradeRequest = _mockRepository.StrictMock<IShiftTradeRequest>();
            Expect.Call(_personRequest.Request).Return(shiftTradeRequest);
            Expect.Call(shiftTradeRequest.GetShiftTradeStatus(null)).IgnoreArguments().Return(ShiftTradeStatus.Referred);

            _mockRepository.ReplayAll();
            Assert.IsTrue(_target.IsSatisfiedBy(_personRequest));
            _mockRepository.VerifyAll();
        }
    }
}
