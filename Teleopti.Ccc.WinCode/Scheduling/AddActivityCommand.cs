using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

		public static DateTimePeriod GetDefaultPeriodFromPart(IScheduleDay part)
		{
			DateTimePeriod defaultPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(part.Period.LocalStartDateTime.Add(TimeSpan.FromHours(8)), part.Period.LocalStartDateTime.Add(TimeSpan.FromHours(17)));

			if (part.SignificantPart() == SchedulePartView.MainShift)
			{
				IPersonAssignment pa = part.AssignmentHighZOrder();
				defaultPeriod = pa.Period;
			}

			return defaultPeriod;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public override void Execute()
		{
			var filteredScheduleParts = SchedulePartsOnePerAgent();

			if (!VerifySelectedSchedule(filteredScheduleParts)) return;

			var nonDeletedShiftCategories = (from c in SchedulerStateHolder.CommonStateHolder.ShiftCategories
											 where ((IDeleteTag)c).IsDeleted == false
											 select c).ToList();


			DateTimePeriod defaultDateTimePeriod = DefaultPeriod ?? filteredScheduleParts[0].Period;

			//Pick the first one instead (alphabetic)
			if (nonDeletedShiftCategories.Count > 0)
			{
				nonDeletedShiftCategories = new List<IShiftCategory> { nonDeletedShiftCategories.OrderBy(s => s.Description.Name).First() };
			}

			IAddActivityViewModel dialog1 =
				ScheduleViewBase.CreateAddActivityViewModel(SchedulerStateHolder.CommonStateHolder.ActiveActivities,
															nonDeletedShiftCategories,
															defaultDateTimePeriod,
															SchedulerStateHolder.TimeZoneInfo);

			bool result = dialog1.Result;

			DateTimePeriod period;

			if (!result) return;

			IActivity activity = dialog1.SelectedItem;
			IShiftCategory shiftCategory = dialog1.SelectedShiftCategory;

			period = dialog1.SelectedPeriod;

			foreach (IScheduleDay part in filteredScheduleParts)
			{
				MainShiftActivityLayer mainShiftActivityLayer = new MainShiftActivityLayer(activity, period);

				part.CreateAndAddActivity(mainShiftActivityLayer, shiftCategory);

				foreach (IPersonAssignment assignment in part.PersonAssignmentCollection())
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