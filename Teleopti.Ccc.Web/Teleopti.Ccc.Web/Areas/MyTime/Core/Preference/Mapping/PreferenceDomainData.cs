using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceDomainData
	{
		public DateOnly SelectedDate { get; set; }
		public DateOnlyPeriod Period { get; set; }
		public IWorkflowControlSet WorkflowControlSet { get; set; }
		public IEnumerable<WorkTimeMinMaxDomainData> WorkTimeMinMax { get; set; }

		public IEnumerable<PreferenceDayDomainData> Days { get; set; }
	}

	public class PreferenceDayDomainData
	{
		public DateOnly Date { get; set; }
		public IPreferenceDay PreferenceDay { get; set; }

		// IPreferenceDay PreferenceDay { get; set; }
	}

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