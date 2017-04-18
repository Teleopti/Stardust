using System;
using NUnit.Framework;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping.Commands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.Grouping.Commands
{
    [TestFixture]
    public class ModifyPageCommandTest
    {
        [Test]
        public void ShouldReturnFalseModifyNotAllowed()
        {
			var personSelectorView = MockRepository.GenerateMock<IPersonSelectorView>();
			var target = new ModifyGroupPageCommand(personSelectorView);
            using (CurrentAuthorization.ThreadlyUse(new NoPermission()))
            {
               Assert.That(target.CanExecute(), Is.False);  
            }
        }

        [Test]
        public void ShouldReturnFalseIfSelectedTabNotIsUserDefined()
        {
			var personSelectorView = MockRepository.GenerateMock<IPersonSelectorView>();
			var target = new ModifyGroupPageCommand(personSelectorView);
	        using (var tab = new TabPageAdv())
	        {
		        personSelectorView.Stub(x => x.SelectedTab).Return(tab);

		        Assert.That(target.CanExecute(), Is.False);
	        }
        }

        [Test]
        public void ShouldReturnTrueIfSelectedTabIsUserDefined()
        {
			var personSelectorView = MockRepository.GenerateMock<IPersonSelectorView>();
			var target = new ModifyGroupPageCommand(personSelectorView);
            var command = MockRepository.GenerateMock<ILoadUserDefinedTabsCommand>();
	        using (var tab = new TabPageAdv {Tag = command})
	        {
		        personSelectorView.Stub(x => x.SelectedTab).Return(tab);
		        
				Assert.That(target.CanExecute(), Is.True);
	        }
        }

        [Test]
        public void ShouldCallViewToModifyGroupPage()
        {
			var personSelectorView = MockRepository.GenerateMock<IPersonSelectorView>();
			var target = new ModifyGroupPageCommand(personSelectorView);
			var command = MockRepository.GenerateMock<ILoadUserDefinedTabsCommand>();
			var id = Guid.NewGuid();
			
			using (var tab = new TabPageAdv("userDefined") { Tag = command })
	        {
				personSelectorView.Stub(x => x.SelectedTab).Return(tab);
		        command.Stub(x => x.Id).Return(id);
		        
		        target.Execute();

				personSelectorView.AssertWasCalled(x => x.ModifyGroupPage(id));
	        }
        }
    }
}