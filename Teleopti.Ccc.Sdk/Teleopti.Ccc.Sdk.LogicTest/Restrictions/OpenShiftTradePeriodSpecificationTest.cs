using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Sdk.Logic.Restrictions;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.Restrictions
{
    [TestFixture]
    public class OpenShiftTradePeriodSpecificationTest
    {
        private OpenShiftTradePeriodSpecification _target;
        private MockRepository _mocks;
        private IShiftTradeAvailableCheckItem _checkItem;
        private IPerson _personFrom;
        private IPerson _personTo;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new OpenShiftTradePeriodSpecification();
            _checkItem = _mocks.StrictMock<IShiftTradeAvailableCheckItem>();
            var wcs = new WorkflowControlSet("wcs") {ShiftTradeOpenPeriodDaysForward = new MinMax<int>(1, 99)};
            _personFrom = PersonFactory.CreatePerson("test person from");
            _personFrom.WorkflowControlSet = wcs;
            _personTo = PersonFactory.CreatePerson("test person to");
            _personTo.WorkflowControlSet = wcs;
        }

        [Test]
        public void ShouldBeWrongIfOutsideOfOpenPeriod()
        {
            using (_mocks.Record())
            {
                Expect.Call(_checkItem.PersonFrom).Return(_personFrom).Repeat.Any();
                Expect.Call(_checkItem.PersonTo).Return(_personTo).Repeat.Any();
                Expect.Call(_checkItem.DateOnly).Return(new DateOnly(DateTime.Today));
            }
            using (_mocks.Playback())
            {
                Assert.That(_target.IsSatisfiedBy(_checkItem), Is.False);
            }
        }

        [Test]
        public void ShouldBeRightIfInsideOfOpenPeriod()
        {
            using (_mocks.Record())
            {
                Expect.Call(_checkItem.PersonFrom).Return(_personFrom).Repeat.Any();
                Expect.Call(_checkItem.PersonTo).Return(_personTo).Repeat.Any();
                Expect.Call(_checkItem.DateOnly).Return(new DateOnly(DateTime.Today.AddDays(1))).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                Assert.That(_target.IsSatisfiedBy(_checkItem), Is.True);
            }
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
    }
}
