using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Ccc.WinCode.Grouping.Commands;

namespace Teleopti.Ccc.WinCodeTest.Grouping.Commands
{
    [TestFixture]
    public class AddGroupPageCommandTest
    {
        private MockRepository _mocks;
        private IAddGroupPageCommand _target;
        private IPersonSelectorView _personSelectorView;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _personSelectorView = _mocks.StrictMock<IPersonSelectorView>();
            _target = new AddGroupPageCommand(_personSelectorView);

        }

        [Test]
        public void ShouldReturnFalseModifyNotAllowed()
        {
            using (new CustomAuthorizationContext(new PrincipalAuthorizationWithNoPermission()))
            {
               Assert.That(_target.CanExecute(), Is.False);  
            }
        }

        [Test]
        public void ShouldReturnTrueModifyAllowed()
        {
              Assert.That(_target.CanExecute(), Is.True);
        }

        [Test]
        public void ShouldCallViewToAddGroupPage()
        {
            Expect.Call(() => _personSelectorView.AddNewGroupPage());
            _mocks.ReplayAll();
            _target.Execute();
            _mocks.VerifyAll();
        }
    }


}