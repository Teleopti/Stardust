using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	public interface IAbsenceRequestProbability
	{
		DateOnly Date { get; set; }
		string CssClass { get; set; }
		string Text { get; set; }
		bool Availability { get; set; }
	}
}