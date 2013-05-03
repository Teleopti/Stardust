using System;
using System.Linq;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
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
		public bool PreferencePeriodIsClosed { get; set; }
		public bool StudentAvailabilityPeriodIsClosed { get; set; }
		public int ShiftTradeSlidingPeriodStart { get; set; }
		public int ShiftTradeSlidingPeriodEnd { get; set; }
		public string AbsenceRequestOpenPeriodStart { get; set; }
		public string AbsenceRequestOpenPeriodEnd { get; set; }
		public string AbsenceRequestPreferencePeriodStart { get; set; }
		public string AbsenceRequestPreferencePeriodEnd { get; set; }

		public void Apply(IUnitOfWork uow)
		{
			var workflowControlSet = new WorkflowControlSet(Name) {SchedulePublishedToDate = DateTime.Parse(SchedulePublishedToDate)};

			if (PreferencePeriodIsClosed)
				workflowControlSet.PreferencePeriod = new DateOnlyPeriod(1900, 1, 1, 1900, 1, 1);
			if (StudentAvailabilityPeriodIsClosed)
				workflowControlSet.StudentAvailabilityPeriod = new DateOnlyPeriod(1900, 1, 1, 1900, 1, 1);

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

				var absenceRequestOpenPeriodStart = String.IsNullOrEmpty(AbsenceRequestOpenPeriodStart)
					                                    ? new DateOnly(1900, 1, 1)
					                                    : new DateOnly(DateTime.Parse(AbsenceRequestOpenPeriodStart));

				var absenceRequestOpenPeriodEnd = String.IsNullOrEmpty(AbsenceRequestOpenPeriodEnd)
											? new DateOnly(2040, 12, 31)
											: new DateOnly(DateTime.Parse(AbsenceRequestOpenPeriodEnd));
				
				var absenceRequestPreferencePeriodStart = String.IsNullOrEmpty(AbsenceRequestPreferencePeriodStart)
					                                    ? new DateOnly(1900, 1, 1)
														: new DateOnly(DateTime.Parse(AbsenceRequestPreferencePeriodStart));

				var absenceRequestPreferencePeriodEnd = String.IsNullOrEmpty(AbsenceRequestPreferencePeriodEnd)
											? new DateOnly(2040, 12, 31)
											: new DateOnly(DateTime.Parse(AbsenceRequestPreferencePeriodEnd));
				
				var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod
				{
					Absence = absence,
					OpenForRequestsPeriod =
						new DateOnlyPeriod(absenceRequestOpenPeriodStart,absenceRequestOpenPeriodEnd),
					Period = new DateOnlyPeriod(absenceRequestPreferencePeriodStart, absenceRequestPreferencePeriodEnd)
				};
				workflowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenPeriod);
			}

			if (!string.IsNullOrEmpty(AvailableActivity))
			{
				var activity = new ActivityRepository(uow).LoadAll().Single(c => c.Description.Name == AvailableActivity);
				workflowControlSet.AllowedPreferenceActivity = activity;
			}

			if (ShiftTradeSlidingPeriodStart != 0 || ShiftTradeSlidingPeriodEnd != 0)
			{
				workflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(ShiftTradeSlidingPeriodStart, ShiftTradeSlidingPeriodEnd);
			}


			var repository = new WorkflowControlSetRepository(uow);
			repository.Add(workflowControlSet);
		}
	}
}