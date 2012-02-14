using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
    [TestFixture]
    public class IsWorkflowControlSetNullSpecificationTest
    {
        private IsWorkflowControlSetNullSpecification _target; 
        private IPerson _mockedPerson;
        private MockRepository _mocker;
        private WorkflowControlSet _workflowControlSet;
        private IShiftTradeSwapDetail _mockedSwapDetail;

        [SetUp]
        public void Setup()
        {
            _mocker = new MockRepository();
            _mockedPerson = _mocker.StrictMock<IPerson>();
            _workflowControlSet = new WorkflowControlSet("wf");
            _target = new IsWorkflowControlSetNullSpecification();
            _mockedSwapDetail = _mocker.StrictMock<IShiftTradeSwapDetail>();
        }

        [Test]
        public void VerifyControlSetMustNotBeNull()
        {
            IList<IShiftTradeSwapDetail> swapDetails = new List<IShiftTradeSwapDetail> { _mockedSwapDetail };

            using(_mocker.Record())
            {
                Expect.Call(_mockedSwapDetail.PersonFrom).Return(_mockedPerson).Repeat.Any();
                Expect.Call(_mockedSwapDetail.PersonTo).Return(_mockedPerson).Repeat.Any();
                //First call
                Expect.Call(_mockedPerson.WorkflowControlSet).Return(null);
                
                //Second call
                Expect.Call(_mockedPerson.WorkflowControlSet).Return(_workflowControlSet);
                Expect.Call(_mockedPerson.WorkflowControlSet).Return(_workflowControlSet);
            }
            using(_mocker.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(swapDetails),"Should be false, WorkflowControlSet is null");
                Assert.IsTrue(_target.IsSatisfiedBy(swapDetails), "Should be false, WorkflowControlSet is not null");
            }
        }

        [Test]
        public void VerifyDenyReason()
        {
            Assert.IsNotNull(UserTexts.Resources.WorkflowControlSetNotSetDenyReason);
            Assert.AreEqual(_target.DenyReason, "WorkflowControlSetNotSetDenyReason");
        }
    }
}
