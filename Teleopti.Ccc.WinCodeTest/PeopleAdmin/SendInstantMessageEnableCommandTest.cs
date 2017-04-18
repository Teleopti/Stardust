using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Commands;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin
{
	[TestFixture]
	public class SendInstantMessageEnableCommandTest
	{
		private SendInstantMessageEnableCommand _target;

		[Test]
		public void ShouldReturnFalseIfNothingIsSelectedInView()
		{
			var personSelectorView = MockRepository.GenerateMock<IPersonSelectorView>();

			personSelectorView.Stub(x => x.SelectedNodes).Return(new List<TreeNodeAdv>());

			_target = new SendInstantMessageEnableCommand(personSelectorView);

			_target.CanExecute().Should().Be.False();
		}

		[Test]
		public void ShouldReturnTrueIfSomethingIsSelectedInView()
		{
			var personSelectorView = MockRepository.GenerateMock<IPersonSelectorView>();

			personSelectorView.Stub(x => x.SelectedNodes).Return(new List<TreeNodeAdv> {new TreeNodeAdv()});

			_target = new SendInstantMessageEnableCommand(personSelectorView);

			_target.CanExecute().Should().Be.True();
		}
	}
}
