using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	//copied from AbsencePreferenceScheduler
	public class AbsencePreferenceScheduling
	{
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly Func<ISchedulePartModifyAndRollbackService> _schedulePartModifyAndRollbackService;
		private readonly IAbsencePreferenceFullDayLayerCreator _absencePreferenceFullDayLayerCreator;

		public AbsencePreferenceScheduling(IEffectiveRestrictionCreator effectiveRestrictionCreator,
			Func<ISchedulePartModifyAndRollbackService> schedulePartModifyAndRollbackService,
			IAbsencePreferenceFullDayLayerCreator absencePreferenceFullDayLayerCreator)
		{
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_absencePreferenceFullDayLayerCreator = absencePreferenceFullDayLayerCreator;
		}

		public void AddPreferredAbsence(ISchedulingCallback schedulingCallback, IEnumerable<IScheduleMatrixPro> matrixList, SchedulingOptions schedulingOptions)
		{
			if (matrixList == null) throw new ArgumentNullException(nameof(matrixList));

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
					_schedulePartModifyAndRollbackService().Modify(part);

					schedulingCallback.Scheduled(new SchedulingCallbackInfo(part, true));
					if (schedulingCallback.IsCancelled) return;
				}
			}
		}
	}
}