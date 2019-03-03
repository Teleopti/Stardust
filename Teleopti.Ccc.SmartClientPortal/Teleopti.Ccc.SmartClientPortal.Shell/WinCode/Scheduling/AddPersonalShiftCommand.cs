using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public class AddPersonalShiftCommand : AddLayerCommand
    {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public AddPersonalShiftCommand(ISchedulerStateHolder schedulerStateHolder, IScheduleViewBase scheduleViewBase, SchedulePresenterBase presenter, IList<IScheduleDay> scheduleParts)
            : base(schedulerStateHolder, scheduleViewBase, presenter, scheduleParts ?? scheduleViewBase.SelectedSchedules())
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public override void Execute()
        {
			var filteredScheduleParts = SchedulePartsOnePerAgent();

			if (!VerifySelectedSchedule(filteredScheduleParts)) return;

			var fallbackDefaultHours = new SetupDateTimePeriodDefaultLocalHoursForActivities(filteredScheduleParts[0], UserTimeZone.Make());
			var periodFromSchedules = new SetupDateTimePeriodToSelectedSchedule(filteredScheduleParts[0], fallbackDefaultHours);
            ISetupDateTimePeriod periodSetup = new SetupDateTimePeriodToDefaultPeriod(DefaultPeriod, periodFromSchedules);

            IAddLayerViewModel<IActivity> dialog1 =
                ScheduleViewBase.CreateAddPersonalActivityViewModel(SchedulerStateHolder.CommonStateHolder.Activities.NonDeleted(),
                                                                    periodSetup.Period,
																	ScheduleViewBase.TimeZoneGuard.CurrentTimeZone());
            bool result = dialog1.Result;
            if (!result) return;

            IActivity activity = dialog1.SelectedItem;
            DateTimePeriod period = dialog1.SelectedPeriod;

			foreach (IScheduleDay part in filteredScheduleParts)
            {
								part.CreateAndAddPersonalActivity(activity, period);
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