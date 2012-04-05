using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting
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
