namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces
{
	 public interface IMeetingDetailView
	 {
		void OnParticipantsSet();
		void OnDisableWhileLoadingStateHolder();
		void OnEnableAfterLoadingStateHolder();
		void OnMeetingDatesChanged();
		void OnMeetingTimeChanged();
		IMeetingDetailPresenter Presenter { get; }
		void ResetSelection();
	 }
}