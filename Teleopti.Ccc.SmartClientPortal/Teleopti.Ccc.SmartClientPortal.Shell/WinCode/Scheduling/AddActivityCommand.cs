using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public class AddActivityCommand : AddLayerCommand
	{
		private readonly SchedulePresenterBase _presenter;

		public AddActivityCommand(ISchedulerStateHolder schedulerStateHolder, IScheduleViewBase scheduleViewBase, SchedulePresenterBase presenter, IList<IScheduleDay> scheduleParts)
			: base(schedulerStateHolder, scheduleViewBase, presenter, scheduleParts ?? scheduleViewBase.SelectedSchedules())
		{
			_presenter = presenter;
			if (ScheduleParts.Count > 0 && ScheduleParts[0] != null)
			{
				DefaultPeriod = GetDefaultPeriodFromPart(ScheduleParts[0]);
			}
		}

		public IActivity DefaultActivity { get; set; }

		public static DateTimePeriod GetDefaultPeriodFromPart(IScheduleDay part)
		{
			var startDateTimeLocal = part.Period.StartDateTimeLocal(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
			DateTimePeriod defaultPeriod =
				TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
					startDateTimeLocal.Add(TimeSpan.FromHours(DefaultSchedulePeriodProvider.DefaultStartHour)),
					startDateTimeLocal.Add(TimeSpan.FromHours(DefaultSchedulePeriodProvider.DefaultEndHour)), TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);

			if (part.SignificantPart() == SchedulePartView.MainShift)
			{
				IPersonAssignment pa = part.PersonAssignment();
				defaultPeriod = pa.Period;
			}

			return defaultPeriod;
		}

		public override void Execute()
		{
			var filteredScheduleParts = SchedulePartsOnePerAgent();

			if (!VerifySelectedSchedule(filteredScheduleParts)) return;

			DateTimePeriod defaultDateTimePeriod = DefaultPeriod ?? filteredScheduleParts[0].Period;

			//Pick the first one instead (alphabetic)
			var activeShiftCategories = SchedulerStateHolder.CommonStateHolder.ShiftCategories.NonDeleted().Take(1).ToArray();

			IAddActivityViewModel dialog1 =
				ScheduleViewBase.CreateAddActivityViewModel(SchedulerStateHolder.CommonStateHolder.Activities.NonDeleted(),
															activeShiftCategories,
															defaultDateTimePeriod,
															SchedulerStateHolder.TimeZoneInfo,
															DefaultActivity);



			bool result = dialog1.Result;

			DateTimePeriod period;

			if (!result) return;

			IActivity activity = dialog1.SelectedItem;
			_presenter.DefaultActivity = activity;
			IShiftCategory shiftCategory = dialog1.SelectedShiftCategory;

			period = dialog1.SelectedPeriod;

			foreach (IScheduleDay part in filteredScheduleParts)
			{
				part.CreateAndAddActivity(activity, period, shiftCategory);

				IPersonAssignment assignment = part.PersonAssignment();
				assignment?.CheckRestrictions();
			}
			
			Presenter.ModifySchedulePart(filteredScheduleParts);
			
			//modify only refresh date for SchedulePart, we need to refresh whole absence period
			foreach (IScheduleDay part in filteredScheduleParts)
			{
				ScheduleViewBase.RefreshRangeForAgentPeriod(part.Person, period);
			}
		}
	}
}