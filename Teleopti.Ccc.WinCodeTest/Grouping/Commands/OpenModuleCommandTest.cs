using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Commands;

namespace Teleopti.Ccc.WinCodeTest.Grouping.Commands
{
    [TestFixture]
    public class OpenModuleCommandTest
    {
        private MockRepository _mocks;
        private IPersonSelectorView _personSelectorView;
        private OpenModuleCommand _target;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _personSelectorView = _mocks.StrictMock<IPersonSelectorView>();

            _target = new OpenModuleCommand(_personSelectorView);
        }

        [Test]
        public void ShouldAskViewIfPersonsSelected()
        {
            Expect.Call(_personSelectorView.SelectedNodes).Return(new List<TreeNodeAdv>());
            _mocks.ReplayAll();
            Assert.That(_target.CanExecute(), Is.False);
            _mocks.VerifyAll();
        }
    }



    
}