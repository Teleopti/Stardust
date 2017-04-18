using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Commands
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface ISendInstantMessageEnableCommand : ICanExecute
	{
	}

	public class SendInstantMessageEnableCommand : ISendInstantMessageEnableCommand
	{
		private readonly IPersonSelectorView _personSelectorView;

		public SendInstantMessageEnableCommand(IPersonSelectorView personSelectorView)
		{
			_personSelectorView = personSelectorView;
		}

		public bool CanExecute()
		{
			return _personSelectorView.SelectedNodes.Count > 0;
		}
	}
}