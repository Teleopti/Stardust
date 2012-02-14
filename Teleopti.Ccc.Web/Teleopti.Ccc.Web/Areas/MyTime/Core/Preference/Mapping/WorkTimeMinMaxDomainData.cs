using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class WorkTimeMinMaxDomainData
	{
		public IWorkTimeMinMax WorkTimeMinMax { get; set; }
		public DateOnly Date { get; set; }

		public WorkTimeMinMaxDomainData(IWorkTimeMinMax workTimeMinMax, DateOnly date)
		{
			WorkTimeMinMax = workTimeMinMax;
			Date = date;
		}
	}
}