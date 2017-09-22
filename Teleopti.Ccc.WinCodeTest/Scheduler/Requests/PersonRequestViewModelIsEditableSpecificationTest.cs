using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests;
using Teleopti.Ccc.TestCommon.Services;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Requests
{
    [TestFixture]
    public class PersonRequestViewModelIsEditableSpecificationTest
    {
        private PersonRequestViewModelIsEditableSpecification _target;
        private MockRepository _mocks;
        private IPersonRequest _personRequest;


        [SetUp]
        public void Setup()
        {
             _target = new PersonRequestViewModelIsEditableSpecification(new PersonRequestAuthorizationCheckerForTest());
            _mocks = new MockRepository();
            _personRequest = _mocks.StrictMock<IPersonRequest>();
        }

        [Test]
        public void VerifyPersonRequestMustBeEditable()
        {
            IPersonRequestViewModel model = _mocks.StrictMock<IPersonRequestViewModel>();
          
            using(_mocks.Record())
            {
                Expect.Call(model.IsSelected).Return(true).Repeat.AtLeastOnce();
                Expect.Call(model.IsWithinSchedulePeriod).Return(true).Repeat.AtLeastOnce();
                Expect.Call(model.IsEditable).Return(false);
                Expect.Call(model.IsEditable).Return(true);
                Expect.Call(model.PersonRequest).Return(_personRequest);

            }
            using(_mocks.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(model));
                Assert.IsTrue(_target.IsSatisfiedBy(model));
            }
        }

        [Test]
        public void VerifyPersonRequestMustBeSelected()
        {
            IPersonRequestViewModel model = _mocks.StrictMock<IPersonRequestViewModel>();

            using (_mocks.Record())
            {
                Expect.Call(model.IsEditable).Return(true).Repeat.AtLeastOnce();
                Expect.Call(model.IsWithinSchedulePeriod).Return(true).Repeat.AtLeastOnce();
                Expect.Call(model.IsSelected).Return(false);
                Expect.Call(model.IsSelected).Return(true);
                Expect.Call(model.PersonRequest).Return(_personRequest);

            }
            using (_mocks.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(model));
                Assert.IsTrue(_target.IsSatisfiedBy(model));
            }
        }

        [Test]
        public void VerifyModelMustBeWithinTheSchedulePeriod()
        {
            IPersonRequestViewModel model = _mocks.StrictMock<IPersonRequestViewModel>();


            using (_mocks.Record())
            {
                Expect.Call(model.IsEditable).Return(true).Repeat.AtLeastOnce();
                Expect.Call(model.IsSelected).Return(true).Repeat.AtLeastOnce();
                Expect.Call(model.IsWithinSchedulePeriod).Return(false);
                Expect.Call(model.IsWithinSchedulePeriod).Return(true);
                Expect.Call(model.PersonRequest).Return(_personRequest);

            }
            using (_mocks.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(model));
                Assert.IsTrue(_target.IsSatisfiedBy(model));
            }
        }




    }
}
