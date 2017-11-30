using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class AbsencePreferenceScheduling
	{
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IAbsencePreferenceFullDayLayerCreator _absencePreferenceFullDayLayerCreator;

		public AbsencePreferenceScheduling(IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IAbsencePreferenceFullDayLayerCreator absencePreferenceFullDayLayerCreator)
		{
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_absencePreferenceFullDayLayerCreator = absencePreferenceFullDayLayerCreator;
		}

		public void AddPreferredAbsence(ISchedulingCallback schedulingCallback, IEnumerable<IScheduleMatrixPro> matrixList,
			SchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
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
					schedulePartModifyAndRollbackService.Modify(part);

					schedulingCallback.Scheduled(new SchedulingCallbackInfo(part, true));
					if (schedulingCallback.IsCancelled) return;
				}
			}
		}
	}
}