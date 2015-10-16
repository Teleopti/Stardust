using System;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Ccc.WinCode.Grouping.Commands;

namespace Teleopti.Ccc.WinCodeTest.Grouping.Commands
{
    [TestFixture]
    public class RenameGroupPageCommandTest
    {
        private MockRepository _mocks;
        private RenameGroupPageCommand _target;
        private IPersonSelectorView _view;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IPersonSelectorView>();
            _target = new RenameGroupPageCommand(_view);
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
        public void ShouldReturnFalseIfGroupPageNotUserDefined()
        {
            //var command = _mocks.StrictMock<ILoadUserDefinedTabsCommand>();
            var tab = new TabPageAdv();
            Expect.Call(_view.SelectedTab).Return(tab);
            _mocks.ReplayAll();
            Assert.That(_target.CanExecute(), Is.False);
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
        public void ShouldReturnTrueIfGroupPageUserDefined()
        {
            
            var command = _mocks.StrictMock<ILoadUserDefinedTabsCommand>();
            var tab = new TabPageAdv{Tag = command};
            Expect.Call(_view.SelectedTab).Return(tab);
            
            _mocks.ReplayAll();
            Assert.That(_target.CanExecute(), Is.True);
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
        public void ShouldCallViewIfCanExecute()
        {
            var id = Guid.NewGuid();
            var command = _mocks.StrictMock<ILoadUserDefinedTabsCommand>();
            var tab = new TabPageAdv("oldName") { Tag = command };
            Expect.Call(_view.SelectedTab).Return(tab).Repeat.Twice();
            Expect.Call(command.Id).Return(id);
            Expect.Call(() =>_view.RenameGroupPage(id,"oldName"));
            _mocks.ReplayAll();
            _target.Execute();
            _mocks.VerifyAll();
        }
    }

}