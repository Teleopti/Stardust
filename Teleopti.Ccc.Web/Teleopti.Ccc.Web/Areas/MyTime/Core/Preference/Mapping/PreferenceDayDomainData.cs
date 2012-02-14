using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceDayDomainData
	{
		public PreferenceDayDomainData(DateOnly dateOnly, DateOnlyPeriod period, IShiftCategory shiftCategory, IDayOffTemplate dayOffTemplate, IAbsence absence, IWorkflowControlSet workflowControlSet, IWorkTimeMinMax workTimeMaxMin)
		{
			Date = dateOnly;
			Period = period;
			ShiftCategory = shiftCategory;
			DayOffTemplate = dayOffTemplate;
		    Absence = absence;
			WorkflowControlSet = workflowControlSet;
			WorkTimeMinMax = workTimeMaxMin;
		}

		public DateOnly Date { get; set; }
		public DateOnlyPeriod Period { get; set; }
		public IShiftCategory ShiftCategory { get; set; }
		public IDayOffTemplate DayOffTemplate { get; set; }
        public IAbsence Absence { get; set; }
		public IWorkflowControlSet WorkflowControlSet { get; set; }
		public IWorkTimeMinMax WorkTimeMinMax { get; set; }
	}
}