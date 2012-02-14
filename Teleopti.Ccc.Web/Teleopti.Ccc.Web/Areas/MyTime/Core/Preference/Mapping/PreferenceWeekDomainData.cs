using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceWeekDomainData
	{
		public DateOnly FirstDayOfWeek { get; set; }
		public DateOnlyPeriod Period { get; set; }
		public IEnumerable<IPreferenceDay> PreferenceDays { get; set; }
		public IWorkflowControlSet WorkflowControlSet { get; set; }
		public IEnumerable<WorkTimeMinMaxDomainData> WorkTimeMinMax { get; set; }
	}
}