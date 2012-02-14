using System;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Ccc.WinCode.Grouping.Commands;

namespace Teleopti.Ccc.WinCodeTest.Grouping.Commands
{
    [TestFixture]
    public class ModifyPageCommandTest
    {
        private MockRepository _mocks;
        private IModifyGroupPageCommand _target;
        private IPersonSelectorView _personSelectorView;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _personSelectorView = _mocks.StrictMock<IPersonSelectorView>();
            _target = new ModifyGroupPageCommand(_personSelectorView);

        }

        [Test]
        public void ShouldReturnFalseModifyNotAllowed()
        {

            using (new CustomAuthorizationContext(new PrincipalAuthorizationWithNoPermission()))
            {
               Assert.That(_target.CanExecute(), Is.False);  
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
        public void ShouldReturnFalseIfSelectedTabNotIsUserDefined()
        {
            var tab = new TabPageAdv();
            Expect.Call(_personSelectorView.SelectedTab).Return(tab);
            _mocks.ReplayAll();
            Assert.That(_target.CanExecute(), Is.False);
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
        public void ShouldReturnTrueIfSelectedTabIsUserDefined()
        {
            var command = _mocks.StrictMock<ILoadUserDefinedTabsCommand>();
            var tab = new TabPageAdv{Tag = command};
            Expect.Call(_personSelectorView.SelectedTab).Return(tab);
            _mocks.ReplayAll();
            Assert.That(_target.CanExecute(), Is.True);
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
        public void ShouldCallViewToModifyGroupPage()
        {
            var command = _mocks.StrictMock<ILoadUserDefinedTabsCommand>();
            var tab = new TabPageAdv("userDefined") { Tag = command };
            Expect.Call(_personSelectorView.SelectedTab).Return(tab).Repeat.Twice();
            Expect.Call(command.Id).Return(new Guid());
            Expect.Call(() => _personSelectorView.ModifyGroupPage(new Guid()));
            _mocks.ReplayAll();

            _target.Execute();

            _mocks.VerifyAll();
        }
    }


}