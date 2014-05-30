using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class WorkflowControlSetConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string SchedulePublishedToDate { get; set; }
		public string AvailableShiftCategory { get; set; }
		public string AvailableDayOff { get; set; }
		public string AvailableAbsence { get; set; }
		public string AvailableActivity { get; set; }
		public bool PreferencePeriodIsClosed { get; set; }
		public bool StudentAvailabilityPeriodIsClosed { get; set; }
		public int ShiftTradeSlidingPeriodStart { get; set; }
		public int ShiftTradeSlidingPeriodEnd { get; set; }
		public string AbsenceRequestOpenPeriodStart { get; set; }
		public string AbsenceRequestOpenPeriodEnd { get; set; }
		public string AbsenceRequestPreferencePeriodStart { get; set; }
		public string AbsenceRequestPreferencePeriodEnd { get; set; }
		public string StaffingCheck { get; set; }
		public string AutoGrant { get; set; }
		public double MinTimePerWeek { get; set; }

		public WorkflowControlSetConfigurable()
		{
			Name = DefaultName.Make("Workflow control set");
		}

		public void Apply(IUnitOfWork uow)
		{
			var workflowControlSet = new WorkflowControlSet(Name) { SchedulePublishedToDate = DateTime.Parse(SchedulePublishedToDate) };

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
				var dayOffTemplate = new DayOffTemplateRepository(uow).FindAllDayOffsSortByDescription().Single(c => c.Description.Name == AvailableDayOff);
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
						new DateOnlyPeriod(absenceRequestOpenPeriodStart, absenceRequestOpenPeriodEnd),
					Period = new DateOnlyPeriod(absenceRequestPreferencePeriodStart, absenceRequestPreferencePeriodEnd)
				};
				workflowControlSet.AddOpenAbsenceRequestPeriod(absenceRequestOpenPeriod);

				switch (StaffingCheck)
				{
					case null:
						workflowControlSet.AbsenceRequestOpenPeriods.First().StaffingThresholdValidator = new AbsenceRequestNoneValidator();
						break;
					case "intraday":
						workflowControlSet.AbsenceRequestOpenPeriods.First().StaffingThresholdValidator = new StaffingThresholdValidator();
						break;
					case "budgetgroup":
						workflowControlSet.AbsenceRequestOpenPeriods.First().StaffingThresholdValidator = new BudgetGroupAllowanceValidator();
						break;
					case "budgetgroup head count":
						workflowControlSet.AbsenceRequestOpenPeriods.First().StaffingThresholdValidator = new BudgetGroupHeadCountValidator();
						break;
					case "mix":
						var mixedList = new List<IAbsenceRequestValidator>(){
							new BudgetGroupAllowanceValidator(), 
							new BudgetGroupHeadCountValidator(),
							new StaffingThresholdValidator()
						};

						foreach (var absenceRequestValidator in mixedList)
						{
							workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
							{
								Absence = absence,
								OpenForRequestsPeriod =
									new DateOnlyPeriod(absenceRequestOpenPeriodStart, absenceRequestOpenPeriodEnd),
								Period = new DateOnlyPeriod(absenceRequestPreferencePeriodStart, absenceRequestPreferencePeriodEnd)
							});
							workflowControlSet.AbsenceRequestOpenPeriods[workflowControlSet.AbsenceRequestOpenPeriods.Count - 1].StaffingThresholdValidator = absenceRequestValidator;
						}
						break;
				}

				switch (AutoGrant)
				{
					case null:
						workflowControlSet.AbsenceRequestOpenPeriods.First().AbsenceRequestProcess = new PendingAbsenceRequest();
						break;
					case "yes":
						workflowControlSet.AbsenceRequestOpenPeriods.First().AbsenceRequestProcess = new GrantAbsenceRequest();
						break;
					case "deny":
						workflowControlSet.AbsenceRequestOpenPeriods.First().AbsenceRequestProcess = new DenyAbsenceRequest();
						break;
				}
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

			if (MinTimePerWeek > 0)
			{
				workflowControlSet.MinTimePerWeek = TimeSpan.FromHours(MinTimePerWeek);
			}

			var repository = new WorkflowControlSetRepository(uow);
			repository.Add(workflowControlSet);
		}
	}
}