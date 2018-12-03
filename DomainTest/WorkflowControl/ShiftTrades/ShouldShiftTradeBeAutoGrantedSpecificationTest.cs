using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
    [TestFixture]
    public class ShouldShiftTradeBeAutoGrantedSpecificationTest
    {
        private ISpecification<IShiftTradeRequest> _target;
        private IPerson _personFrom;
        private IPerson _personTo;
        private DateOnly _dateOnly;
        private IWorkflowControlSet _workflowControlSetFrom;
        private IWorkflowControlSet _workflowControlSetTo;
        private IShiftTradeRequest _shiftTradeRequest;

        [SetUp]
        public void Setup()
        {
            _personFrom = PersonFactory.CreatePerson();
            _personTo = PersonFactory.CreatePerson();
            _dateOnly = DateOnly.Today;
            _workflowControlSetFrom = new WorkflowControlSet("From");
            _workflowControlSetTo = new WorkflowControlSet("To");
            _personFrom.WorkflowControlSet = _workflowControlSetFrom;
            _personTo.WorkflowControlSet = _workflowControlSetTo;
            _shiftTradeRequest =
                new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
                                          {new ShiftTradeSwapDetail(_personFrom, _personTo, _dateOnly, _dateOnly)});
            _target = new ShouldShiftTradeBeAutoGrantedSpecification();
        }

        [Test]
        public void VerifyTrueWhenAllWorkflowControlSetsAreSetToAutoGrant()
        {
            _workflowControlSetFrom.AutoGrantShiftTradeRequest = true;
            _workflowControlSetTo.AutoGrantShiftTradeRequest = true;

            Assert.IsTrue(_target.IsSatisfiedBy(_shiftTradeRequest));
        }

        [Test]
        public void VerifyFalseWhenAllWorkflowControlSetsAreSetToNotAutoGrant()
        {
            Assert.IsFalse(_target.IsSatisfiedBy(_shiftTradeRequest));
        }

        [Test]
        public void VerifyFalseWhenAnyWorkflowControlSetIsSetToAutoGrant()
        {
            _workflowControlSetFrom.AutoGrantShiftTradeRequest = true;
            Assert.IsFalse(_target.IsSatisfiedBy(_shiftTradeRequest));
        }

        [Test]
        public void VerifyFalseWhenAnyWorkflowControlSetIsNull()
        {
            _workflowControlSetFrom.AutoGrantShiftTradeRequest = true;
            _personTo.WorkflowControlSet = null;
            Assert.IsFalse(_target.IsSatisfiedBy(_shiftTradeRequest));
        }
    }
}
