using System.Drawing;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting
{
	public interface IScheduleReportDialogGraphicalView
	{
		void UserCancel();
		void UserOk();
		Color BackgroundColor { get; set; }
		void EnableSortOptions(bool enabled);
		void EnableSingleFile(bool enabled);
	    void EnableShowPublicNote(bool enabled);
		void UpdateFromModel(ScheduleReportDialogGraphicalModel model);
	}
}
