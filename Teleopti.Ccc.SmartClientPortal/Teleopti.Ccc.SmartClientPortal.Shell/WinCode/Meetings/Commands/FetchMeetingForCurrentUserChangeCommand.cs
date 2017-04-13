using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.WinCode.Meetings.Overview;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Meetings.Commands
{
	public interface IFetchMeetingForCurrentUserChangeCommand : IExecutableCommand, ICanExecute
	{

	}

	public class FetchMeetingForCurrentUserChangeCommand : IFetchMeetingForCurrentUserChangeCommand
	{
		private readonly IMeetingOverviewView _meetingOverviewView;
		private readonly IMeetingOverviewViewModel _model;

		public FetchMeetingForCurrentUserChangeCommand(IMeetingOverviewView meetingOverviewView, IMeetingOverviewViewModel model)
		{
			_meetingOverviewView = meetingOverviewView;
			_model = model;
		}

		public void Execute()
		{
			_model.IncludeForOrganizer = _meetingOverviewView.FetchForCurrentUser;
			_meetingOverviewView.ReloadMeetings();
		}

		public bool CanExecute()
		{
			return true;
		}
	}
}