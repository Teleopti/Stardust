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
		public IEnumerable<PreferenceDayDomainData> Days { get; set; }
	}

	public class PreferenceDayDomainData
	{
		public DateOnly Date { get; set; }
		public IPreferenceDay PreferenceDay { get; set; }
		public IWorkTimeMinMax WorkTimeMinMax { get; set; }
	}

}