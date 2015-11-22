using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class AddActivityCommand : AddLayerCommand
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public AddActivityCommand(ISchedulerStateHolder schedulerStateHolder, IScheduleViewBase scheduleViewBase, SchedulePresenterBase presenter, IList<IScheduleDay> scheduleParts)
			: base(schedulerStateHolder, scheduleViewBase, presenter, scheduleParts ?? scheduleViewBase.SelectedSchedules())
		{
			if (ScheduleParts.Count > 0 && ScheduleParts[0] != null)
			{
				DefaultPeriod = GetDefaultPeriodFromPart(ScheduleParts[0]);
			}
		}

		public IActivity DefaultActivity { get; set; }

		public static DateTimePeriod GetDefaultPeriodFromPart(IScheduleDay part)
		{
			DateTimePeriod defaultPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(part.Period.LocalStartDateTime.Add(TimeSpan.FromHours(8)), part.Period.LocalStartDateTime.Add(TimeSpan.FromHours(17)));

			if (part.SignificantPart() == SchedulePartView.MainShift)
			{
				IPersonAssignment pa = part.PersonAssignment();
				defaultPeriod = pa.Period;
			}

			return defaultPeriod;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public override void Execute()
		{
			var filteredScheduleParts = SchedulePartsOnePerAgent();

			if (!VerifySelectedSchedule(filteredScheduleParts)) return;

			DateTimePeriod defaultDateTimePeriod = DefaultPeriod ?? filteredScheduleParts[0].Period;

			//Pick the first one instead (alphabetic)
			var activeShiftCategories = SchedulerStateHolder.CommonStateHolder.ActiveShiftCategories.Take(1).ToArray();

			IAddActivityViewModel dialog1 =
				ScheduleViewBase.CreateAddActivityViewModel(SchedulerStateHolder.CommonStateHolder.ActiveActivities,
															activeShiftCategories,
															defaultDateTimePeriod,
															SchedulerStateHolder.TimeZoneInfo,
															DefaultActivity);

			bool result = dialog1.Result;

			DateTimePeriod period;

			if (!result) return;

			IActivity activity = dialog1.SelectedItem;
			IShiftCategory shiftCategory = dialog1.SelectedShiftCategory;

			period = dialog1.SelectedPeriod;

			foreach (IScheduleDay part in filteredScheduleParts)
			{
				part.CreateAndAddActivity(activity, period, shiftCategory);

				IPersonAssignment assignment = part.PersonAssignment();
				if(assignment != null)
					assignment.CheckRestrictions();

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