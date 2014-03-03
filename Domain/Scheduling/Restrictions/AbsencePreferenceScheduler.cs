using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public interface IAbsencePreferenceScheduler
	{
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
        void AddPreferredAbsence(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions);
	}

	public class AbsencePreferenceScheduler : IAbsencePreferenceScheduler
	{
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private readonly IAbsencePreferenceFullDayLayerCreator _absencePreferenceFullDayLayerCreator;

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public AbsencePreferenceScheduler(IEffectiveRestrictionCreator effectiveRestrictionCreator,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IAbsencePreferenceFullDayLayerCreator absencePreferenceFullDayLayerCreator)
		{
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_absencePreferenceFullDayLayerCreator = absencePreferenceFullDayLayerCreator;
		}

        public void AddPreferredAbsence(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions)
        {
            if(matrixList == null) throw new ArgumentNullException("matrixList");

            foreach (var scheduleMatrixPro in matrixList)
            {
                foreach (var scheduleDayPro in scheduleMatrixPro.UnlockedDays)
                {
                    var part = scheduleDayPro.DaySchedulePart();
                    var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(part, schedulingOptions);

                    // there should not be an absence if it is no preferenceday or if we dont't use preference when scheduling but...
                    if (effectiveRestriction != null && !effectiveRestriction.IsPreferenceDay)
                        continue;

                    if (effectiveRestriction == null || effectiveRestriction.Absence == null) continue;
	                var layer = _absencePreferenceFullDayLayerCreator.Create(part, effectiveRestriction.Absence);
                    part.CreateAndAddAbsence(layer);
                    _schedulePartModifyAndRollbackService.Modify(part);

					var eventArgs = new SchedulingServiceSuccessfulEventArgs(part);
                    OnDayScheduled(eventArgs);
                    if (eventArgs.Cancel) return;
                }
            }
        }

		protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs scheduleServiceBaseEventArgs)
		{
			EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
			if (temp != null)
			{
				temp(this, scheduleServiceBaseEventArgs);
			}
		}
	}
}