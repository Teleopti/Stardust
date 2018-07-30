using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.WeekSchedule;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	public interface IAbsenceReportPersister
	{
		AbsenceReportViewModel Persist(AbsenceReportInput input);
	}
}