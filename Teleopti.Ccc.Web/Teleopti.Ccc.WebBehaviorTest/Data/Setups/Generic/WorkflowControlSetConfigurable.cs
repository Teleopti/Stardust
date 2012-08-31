using System;
using System.Linq;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class WorkflowControlSetConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string SchedulePublishedToDate { get; set; }
		public string AvailableShiftCategory { get; set; }
		public string AvailableDayOff { get; set; }
		public string AvailableAbsence{ get; set; }
		public string AvailableActivity { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			var workflowControlSet = new WorkflowControlSet(Name) {SchedulePublishedToDate = DateTime.Parse(SchedulePublishedToDate)};

			if (!string.IsNullOrEmpty(AvailableShiftCategory))
			{
				var shiftCategory = new ShiftCategoryRepository(uow).FindAll().Single(c => c.Description.Name == AvailableShiftCategory);
				workflowControlSet.AddAllowedPreferenceShiftCategory(shiftCategory);
			}

			if (!string.IsNullOrEmpty(AvailableDayOff))
			{
				var dayOffTemplate = new DayOffRepository(uow).FindAllDayOffsSortByDescription().Single(c => c.Description.Name == AvailableDayOff);
				workflowControlSet.AddAllowedPreferenceDayOff(dayOffTemplate);
			}

			if (!string.IsNullOrEmpty(AvailableAbsence))
			{
				var absence = new AbsenceRepository(uow).LoadAll().Single(c => c.Description.Name == AvailableAbsence);
				workflowControlSet.AddAllowedPreferenceAbsence(absence);
			}

			if (!string.IsNullOrEmpty(AvailableActivity))
			{
				var activity = new ActivityRepository(uow).LoadAll().Single(c => c.Description.Name == AvailableActivity);
				workflowControlSet.AllowedPreferenceActivity = activity;
			}


			var repository = new WorkflowControlSetRepository(uow);
			repository.Add(workflowControlSet);
		}
	}
}