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
    public class DeleteGroupPageCommandTest
    {
        [Test]
        public void ShouldReturnFalseModifyNotAllowed()
		{
			var personSelectorView = MockRepository.GenerateMock<IPersonSelectorView>();
			var target = new DeleteGroupPageCommand(personSelectorView);
            using (CurrentAuthorization.ThreadlyUse(new NoPermission()))
            {
               Assert.That(target.CanExecute(), Is.False);  
            }
        }

        [Test]
        public void ShouldReturnTrueModifyAllowed()
		{
			var personSelectorView = MockRepository.GenerateMock<IPersonSelectorView>();
			var target = new DeleteGroupPageCommand(personSelectorView);
            var command = MockRepository.GenerateMock<ILoadUserDefinedTabsCommand>();
            var tab = new TabPageAdv("userDefined"){Tag = command};
            personSelectorView.Stub(x => x.SelectedTab).Return(tab);

            Assert.That(target.CanExecute(), Is.True);
        }

        [Test]
        public void ShouldCallViewToDeleteGroupPage()
		{
			var personSelectorView = MockRepository.GenerateMock<IPersonSelectorView>();
			var target = new DeleteGroupPageCommand(personSelectorView);
            var command = MockRepository.GenerateMock<ILoadUserDefinedTabsCommand>();
            var tab = new TabPageAdv("userDefined") { Tag = command };
			var id = Guid.NewGuid();
			
			personSelectorView.Stub(x => x.SelectedTab).Return(tab);
	        command.Stub(x => x.Id).Return(id);
            
             target.Execute();

	        personSelectorView.AssertWasCalled(x => x.DeleteGroupPage(id, "userDefined"));
        }
    }
}