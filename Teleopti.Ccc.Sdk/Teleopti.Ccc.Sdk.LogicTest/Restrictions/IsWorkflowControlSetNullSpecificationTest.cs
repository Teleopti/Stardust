using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Sdk.Logic.Restrictions;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.Restrictions
{
    [TestFixture]
    public class IsWorkflowControlSetNullSpecificationTest
    {
        private IsWorkflowControlSetNullSpecification _target;
        private MockRepository _mocks;
        private IShiftTradeAvailableCheckItem _checkItem;
        private IPerson _personFrom;
        private IPerson _personTo;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new IsWorkflowControlSetNullSpecification();
            _checkItem = _mocks.StrictMock<IShiftTradeAvailableCheckItem>();
            _personFrom = PersonFactory.CreatePerson("test person from");
            _personTo = PersonFactory.CreatePerson("test person to");
        }

        [Test]
        public void ShouldFailIfOneHasNoWorkflowControlSet()
        {
            _personFrom.WorkflowControlSet = null;
            _personTo.WorkflowControlSet = new WorkflowControlSet();
            using (_mocks.Record())
            {
                Expect.Call(_checkItem.PersonFrom).Return(_personFrom).Repeat.Any();
                Expect.Call(_checkItem.PersonTo).Return(_personTo).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                Assert.That(_target.IsSatisfiedBy(_checkItem), Is.False);
            }
        }
        
        [Test]
        public void ShouldPassIfBothHaveWorkflowControlSet()
        {
            _personFrom.WorkflowControlSet = new WorkflowControlSet();
            _personTo.WorkflowControlSet = new WorkflowControlSet();
            using (_mocks.Record())
            {
                Expect.Call(_checkItem.PersonFrom).Return(_personFrom).Repeat.Any();
                Expect.Call(_checkItem.PersonTo).Return(_personTo).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                Assert.That(_target.IsSatisfiedBy(_checkItem), Is.True);
            }
        }
    }
}
