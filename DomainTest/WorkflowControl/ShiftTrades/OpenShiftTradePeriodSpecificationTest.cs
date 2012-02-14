using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
    [TestFixture]
    public class OpenShiftTradePeriodSpecificationTest
    {
        IOpenShiftTradePeriodSpecification _target;
        private MockRepository _mock;

        private IPerson _personFrom;
        private IPerson _personTo;
        private IWorkflowControlSet _workflowControlSetFrom;
        private IWorkflowControlSet _workflowControlSetTo;

        [SetUp]
        public void Setup()
        {
            _target = new OpenShiftTradePeriodSpecification();
            _mock = new MockRepository();
            _personFrom = _mock.StrictMock<IPerson>();
            _personTo = _mock.StrictMock<IPerson>();
            _workflowControlSetFrom = _mock.StrictMock<IWorkflowControlSet>();
            _workflowControlSetTo = _mock.StrictMock<IWorkflowControlSet>();
        }

        [Test]
        public void NoWorkflowControlSetReturnsFalse()
        {

            using (_mock.Record())
            {
                Expect.Call(_personFrom.WorkflowControlSet).Return(null).Repeat.AtLeastOnce();
                Expect.Call(_personTo.WorkflowControlSet).Return(_workflowControlSetTo).Repeat.AtLeastOnce();
            }

            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(tradeDetails()));
            }

        }

        [Test]
        public void CorrectOpenPeriodReturnsTrue()
        {
            using (_mock.Record())
            {
                Expect.Call(_personFrom.WorkflowControlSet).Return(_workflowControlSetFrom).Repeat.AtLeastOnce();
                Expect.Call(_personTo.WorkflowControlSet).Return(_workflowControlSetTo).Repeat.AtLeastOnce();
                Expect.Call(_workflowControlSetFrom.ShiftTradeOpenPeriodDaysForward).Return(new MinMax<int>(3, 10)).Repeat.AtLeastOnce();
                Expect.Call(_workflowControlSetTo.ShiftTradeOpenPeriodDaysForward).Return(new MinMax<int>(3, 10)).Repeat.AtLeastOnce();
            }

            using (_mock.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(tradeDetails()));
            }
        }

        [Test]
        public void PersonFromHasNotOpenPeriodReturnsFalse()
        {
            using (_mock.Record())
            {
                Expect.Call(_personFrom.WorkflowControlSet).Return(_workflowControlSetFrom).Repeat.AtLeastOnce();
                Expect.Call(_personTo.WorkflowControlSet).Return(_workflowControlSetTo).Repeat.AtLeastOnce();
                Expect.Call(_workflowControlSetFrom.ShiftTradeOpenPeriodDaysForward).Return(new MinMax<int>(10, 15)).Repeat.AtLeastOnce();
                Expect.Call(_workflowControlSetTo.ShiftTradeOpenPeriodDaysForward).Return(new MinMax<int>(3, 10)).Repeat.AtLeastOnce();
            }

            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(tradeDetails()));
            }
        }

        [Test]
        public void PersonToHasNotOpenPeriodReturnsFalse()
        {
            using (_mock.Record())
            {
                Expect.Call(_personFrom.WorkflowControlSet).Return(_workflowControlSetFrom).Repeat.AtLeastOnce();
                Expect.Call(_personTo.WorkflowControlSet).Return(_workflowControlSetTo).Repeat.AtLeastOnce();
                Expect.Call(_workflowControlSetFrom.ShiftTradeOpenPeriodDaysForward).Return(new MinMax<int>(3, 15)).Repeat.AtLeastOnce();
                Expect.Call(_workflowControlSetTo.ShiftTradeOpenPeriodDaysForward).Return(new MinMax<int>(10, 20)).Repeat.AtLeastOnce();
            }

            using (_mock.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(tradeDetails()));
            }
        }

        [Test]
        public void VerifyDenyReason()
        {
            Assert.AreEqual(_target.DenyReason, "OpenShiftTradePeriodDenyReason");
            Assert.IsNotNull(UserTexts.Resources.OpenShiftTradePeriodDenyReason);
        }

        private IList<IShiftTradeSwapDetail> tradeDetails()
        {
            IList<IShiftTradeSwapDetail> ret = new List<IShiftTradeSwapDetail>();
            IShiftTradeSwapDetail detail1 = new ShiftTradeSwapDetail(_personFrom, _personTo, new DateOnly(DateTime.Now.AddDays(5)), new DateOnly(DateTime.Now.AddDays(5)));

            ret.Add(detail1);
            return ret;
        }

    }

    

    
}
