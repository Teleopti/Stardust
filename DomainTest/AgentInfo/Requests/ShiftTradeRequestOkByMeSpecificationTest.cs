using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
    [TestFixture]
    public class ShiftTradeRequestOkByMeSpecificationTest
    {
        private ShiftTradeRequestOkByMeSpecification _target;
        private MockRepository _mockRepository;
        private IPersonRequest _personRequest;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            IShiftTradeRequestStatusChecker shiftTradeRequestStatusChecker = new ShiftTradeRequestStatusCheckerForTestDoesNothing();
            _target = new ShiftTradeRequestOkByMeSpecification(shiftTradeRequestStatusChecker);
            _personRequest = _mockRepository.StrictMock<IPersonRequest>();
        }

        [Test]
        public void VerifyShiftTradeRequestOkByBothPartsReturnsTrue()
        {
            IShiftTradeRequest shiftTradeRequest = _mockRepository.StrictMock<IShiftTradeRequest>();
            Expect.Call(_personRequest.Request).Return(shiftTradeRequest);
            Expect.Call(shiftTradeRequest.GetShiftTradeStatus(null)).IgnoreArguments().Return(ShiftTradeStatus.OkByBothParts);

            _mockRepository.ReplayAll();
            Assert.IsFalse(_target.IsSatisfiedBy(_personRequest));
            _mockRepository.VerifyAll();
        }

        [Test]
        public void VerifyShiftTradeRequestOkByBothPartsReturnsFalse()
        {
            IShiftTradeRequest shiftTradeRequest = _mockRepository.StrictMock<IShiftTradeRequest>();
            Expect.Call(_personRequest.Request).Return(shiftTradeRequest);
            Expect.Call(shiftTradeRequest.GetShiftTradeStatus(null)).IgnoreArguments().Return(ShiftTradeStatus.OkByMe);

            _mockRepository.ReplayAll();
            Assert.IsTrue(_target.IsSatisfiedBy(_personRequest));
            _mockRepository.VerifyAll();
        }
    }
}
