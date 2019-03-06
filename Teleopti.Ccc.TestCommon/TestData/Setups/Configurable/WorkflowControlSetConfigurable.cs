using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class WorkflowControlSetConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public string SchedulePublishedToDate { get; set; }
		public string AvailableShiftCategory { get; set; }
		public string AvailableDayOff { get; set; }
		public string AvailableAbsence { get; set; }
		public string ReportableAbsence { get; set; }
		public string AvailableActivity { get; set; }
		public bool PreferencePeriodIsClosed { get; set; }
		public string PreferencePeriodStart { get; set; }
		public string PreferencePeriodEnd { get; set; }
		public bool StudentAvailabilityPeriodIsClosed { get; set; }
		public string StudentAvailabilityPeriodStart { get; set; }
		public string StudentAvailabilityPeriodEnd { get; set; }
		public int ShiftTradeSlidingPeriodStart { get; set; }
		public int ShiftTradeSlidingPeriodEnd { get; set; }
		public string AbsenceRequestOpenPeriodStart { get; set; }
		public string AbsenceRequestOpenPeriodEnd { get; set; }
		public int OvertimeRequestOpenPeriodRollingStart { get; set; }
		public int OvertimeRequestOpenPeriodRollingEnd { get; set; }
		public bool OvertimeRequestAutoApprove { get; set; }
		public string AbsenceRequestPreferencePeriodStart { get; set; }
		public string AbsenceRequestPreferencePeriodEnd { get; set; }
		public string StaffingCheck { get; set; }
		public string AutoGrant { get; set; }
		public string BusinessUnit { get; set; }
		public bool AnonymousTrading { get; set; }
		public bool AbsenceRequestWaitlistEnabled { get; set; }
		public int? AbsenceRequestExpiredThreshold { get; set; }

		public bool AbsenceProbabilityEnabled { get; set; }
		public bool OvertimeProbabilityEnabled { get; set; }
		public int MaximumWorkdays { get; set; }
		public int ShiftTradeFlexibilityDays { get; set; }
		public bool AutoGrantShiftTradeRequest { get; set; }

		public WorkflowControlSetConfigurable()
		{
			Name = RandomName.Make("Workflow control set");
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var workflowControlSet = new WorkflowControlSet(Name)
			{
				SchedulePublishedToDate =
					!string.IsNullOrEmpty(SchedulePublishedToDate) ? DateTime.Parse(SchedulePublishedToDate) : (DateTime?) null,
				AbsenceRequestExpiredThreshold = AbsenceRequestExpiredThreshold ?? 15,
				AutoGrantShiftTradeRequest = AutoGrantShiftTradeRequest
			};

			if (MaximumWorkdays > 0) workflowControlSet.MaximumConsecutiveWorkingDays = MaximumWorkdays;
			if (ShiftTradeFlexibilityDays > 0) workflowControlSet.ShiftTradeTargetTimeFlexibility = TimeSpan.FromDays(ShiftTradeFlexibilityDays);

			if (!string.IsNullOrEmpty(StudentAvailabilityPeriodStart) && !string.IsNullOrEmpty(StudentAvailabilityPeriodEnd))
			{
				var studentAvailabilityPeriod = new DateOnlyPeriod(new DateOnly(DateTime.Parse(StudentAvailabilityPeriodStart)), new DateOnly(DateTime.Parse(StudentAvailabilityPeriodEnd)));
				workflowControlSet.StudentAvailabilityPeriod = studentAvailabilityPeriod;
				workflowControlSet.StudentAvailabilityInputPeriod = new DateOnlyPeriod(studentAvailabilityPeriod.StartDate.AddDays(-50),studentAvailabilityPeriod.StartDate);
			}

			if (!string.IsNullOrEmpty(PreferencePeriodStart) && !string.IsNullOrEmpty(PreferencePeriodEnd))
			{
				var preferencePeriod = new DateOnlyPeriod(new DateOnly(DateTime.Parse(PreferencePeriodStart)), new DateOnly(DateTime.Parse(PreferencePeriodEnd)));
				workflowControlSet.PreferencePeriod = preferencePeriod;
				workflowControlSet.PreferenceInputPeriod = new DateOnlyPeriod(preferencePeriod.StartDate.AddDays(-50), preferencePeriod.StartDate);
			}
			if (PreferencePeriodIsClosed)
				workflowControlSet.PreferencePeriod = new DateOnlyPeriod(1900, 1, 1, 1900, 1, 1);
			if (StudentAvailabilityPeriodIsClosed)
				workflowControlSet.StudentAvailabilityPeriod = new DateOnlyPeriod(1900, 1, 1, 1900, 1, 1);

			if (!string.IsNullOrEmpty(AvailableShiftCategory))
			{
				var shiftCategory = ShiftCategoryRepository.DONT_USE_CTOR(currentUnitOfWork).FindAll().Single(c => c.Description.Name == AvailableShiftCategory);
				workflowControlSet.AddAllowedPreferenceShiftCategory(shiftCategory);
			}

			if (!string.IsNullOrEmpty(AvailableDayOff))
			{
				var dayOffTemplate = DayOffTemplateRepository.DONT_USE_CTOR2(currentUnitOfWork).FindAllDayOffsSortByDescription().Single(c => c.Description.Name == AvailableDayOff);
				workflowControlSet.AddAllowedPreferenceDayOff(dayOffTemplate);
			}

			if (!string.IsNullOrEmpty(AvailableAbsence))
			{
				var absence = AbsenceRepository.DONT_USE_CTOR(currentUnitOfWork).LoadAll().Single(c => c.Description.Name == AvailableAbsence);
				workflowControlSet.AddAllowedPreferenceAbsence(absence);
				workflowControlSet.AbsenceRequestWaitlistEnabled = AbsenceRequestWaitlistEnabled;
				workflowControlSet.AbsenceProbabilityEnabled = AbsenceProbabilityEnabled;
				workflowControlSet.OvertimeProbabilityEnabled = OvertimeProbabilityEnabled;

				var absenceRequestOpenPeriodStart = string.IsNullOrEmpty(AbsenceRequestOpenPeriodStart)
														? new DateOnly(1900, 1, 1)
														: new DateOnly(DateTime.Parse(AbsenceRequestOpenPeriodStart));

				var absenceRequestOpenPeriodEnd = string.IsNullOrEmpty(AbsenceRequestOpenPeriodEnd)
											? new DateOnly(2040, 12, 31)
											: new DateOnly(DateTime.Parse(AbsenceRequestOpenPeriodEnd));

				var absenceRequestPreferencePeriodStart = string.IsNullOrEmpty(AbsenceRequestPreferencePeriodStart)
														? new DateOnly(1900, 1, 1)
														: new DateOnly(DateTime.Parse(AbsenceRequestPreferencePeriodStart));

				var absenceRequestPreferencePeriodEnd = string.IsNullOrEmpty(AbsenceRequestPreferencePeriodEnd)
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

					case "intraday with shrinkage":
						workflowControlSet.AbsenceRequestOpenPeriods.First().StaffingThresholdValidator = new StaffingThresholdWithShrinkageValidator();
						break;

					case "mix":
						var mixedList = new List<IAbsenceRequestValidator>{
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
				var activity = ActivityRepository.DONT_USE_CTOR(currentUnitOfWork, null, null).LoadAll().Single(c => c.Description.Name == AvailableActivity);
				workflowControlSet.AllowedPreferenceActivity = activity;
			}

			if (!string.IsNullOrEmpty(ReportableAbsence))
			{
				var absence = AbsenceRepository.DONT_USE_CTOR(currentUnitOfWork).LoadAll().Single(c => c.Description.Name == ReportableAbsence);
				workflowControlSet.AddAllowedAbsenceForReport(absence);
			}

			if (ShiftTradeSlidingPeriodStart != 0 || ShiftTradeSlidingPeriodEnd != 0)
			{
				workflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(ShiftTradeSlidingPeriodStart, ShiftTradeSlidingPeriodEnd);
			}

			if (!string.IsNullOrEmpty(BusinessUnit))
				workflowControlSet.SetBusinessUnit(BusinessUnitRepository.DONT_USE_CTOR(currentUnitOfWork, null, null).LoadAll().Single(b => b.Name == BusinessUnit));

			workflowControlSet.AnonymousTrading = AnonymousTrading;

			if (OvertimeRequestOpenPeriodRollingEnd > 0 && OvertimeRequestOpenPeriodRollingEnd >= OvertimeRequestOpenPeriodRollingStart)
			{
				if (OvertimeRequestAutoApprove)
				{
					workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod { AutoGrantType = OvertimeRequestAutoGrantType.Yes, BetweenDays = new MinMax<int>(OvertimeRequestOpenPeriodRollingStart, OvertimeRequestOpenPeriodRollingEnd) });

				}
				else
				{
					workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod { AutoGrantType = OvertimeRequestAutoGrantType.No, BetweenDays = new MinMax<int>(OvertimeRequestOpenPeriodRollingStart, OvertimeRequestOpenPeriodRollingEnd) });

				}
			}

			var repository = WorkflowControlSetRepository.DONT_USE_CTOR(currentUnitOfWork);
			repository.Add(workflowControlSet);
		}
	}
}