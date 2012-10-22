using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Commands
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