using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public class WeekPresenter : SchedulePresenterBase
    {
        public WeekPresenter(IScheduleViewBase view, ISchedulerStateHolder schedulerState, IGridlockManager lockManager,
            ClipHandler<IScheduleDay> clipHandler, SchedulePartFilter schedulePartFilter, IOverriddenBusinessRulesHolder overriddenBusinessRulesHolder,
            IScheduleDayChangeCallback scheduleDayChangeCallback, IScheduleTag defaultScheduleTag, IUndoRedoContainer undoRedoContainer, ITimeZoneGuard timeZoneGuard)
            : base(view, schedulerState, lockManager, clipHandler, schedulePartFilter, overriddenBusinessRulesHolder,scheduleDayChangeCallback,
            defaultScheduleTag, undoRedoContainer, timeZoneGuard)
        {
        }

        public static DateDateTimePeriodDictionary CreateSpanDictionaryFromSchedule(IScheduleDay schedulePart)
        {
            var timelineSpan = new DateDateTimePeriodDictionary();
            DateTime start = schedulePart.Period.StartDateTimeLocal(schedulePart.TimeZone).Date.AddHours(8);
            DateTime end = schedulePart.Period.StartDateTimeLocal(schedulePart.TimeZone).Date.AddHours(17);
	        IPersonAssignment personAssignment;

            switch (schedulePart.SignificantPartForDisplay())
            {
                case SchedulePartView.MainShift:
                    personAssignment = schedulePart.PersonAssignment();
		            start = personAssignment.Period.StartDateTimeLocal(schedulePart.TimeZone);
		            end = personAssignment.Period.EndDateTimeLocal(schedulePart.TimeZone);
                    break;
				case SchedulePartView.ContractDayOff:
                case SchedulePartView.FullDayAbsence:
                    IVisualLayerCollection layerCollection = schedulePart.ProjectionService().CreateProjection();
                    if (layerCollection.Any())
                    {
                        start = layerCollection.ElementAt(0).Period.StartDateTimeLocal(schedulePart.TimeZone);
                        end = layerCollection.ElementAt(0).Period.EndDateTimeLocal(schedulePart.TimeZone);
                    }
                    break;
                case SchedulePartView.PersonalShift:
					personAssignment = schedulePart.PersonAssignment();
		            var layer = personAssignment.PersonalActivities().FirstOrDefault();
                    if (layer != null)
                    {
                        start = layer.Period.StartDateTimeLocal(schedulePart.TimeZone);
                        end = layer.Period.EndDateTimeLocal(schedulePart.TimeZone);
                    }
                    break;
            }

            start = start.AddMinutes(-start.Minute).AddHours(-1);
            end = end.AddMinutes(-end.Minute).AddHours(1);

            var dp = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(start, schedulePart.TimeZone), TimeZoneHelper.ConvertToUtc(end, schedulePart.TimeZone));
            timelineSpan.Add(new DateOnly(schedulePart.Period.StartDateTimeLocal(schedulePart.TimeZone)), dp);

            return timelineSpan;
        }
    }
}