using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public interface IAbsencePreferenceScheduler
	{
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
		void AddPreferredAbsence(IEnumerable<DateOnly> dates, IEnumerable<IPerson> persons);
	}

	public class AbsencePreferenceScheduler : IAbsencePreferenceScheduler
	{
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly ISchedulingOptions _schedulingOptions;
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public AbsencePreferenceScheduler(ISchedulingResultStateHolder schedulingResultStateHolder, IEffectiveRestrictionCreator effectiveRestrictionCreator,
			ISchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_schedulingOptions = schedulingOptions;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
		}

		public void AddPreferredAbsence(IEnumerable<DateOnly> dates, IEnumerable<IPerson> persons)
		{
			if(dates == null) return;
			if(persons == null) return;

			foreach (var date in dates)
			{
				foreach (var person in persons)
				{
					var part = _schedulingResultStateHolder.Schedules[person].ScheduledDay(date);

					var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(part, _schedulingOptions);

					// there should not be an absence if it is no preferenceday or if we dont't use preference when scheduling but...
					if (effectiveRestriction != null && !effectiveRestriction.IsPreferenceDay)
						continue;

					if (effectiveRestriction == null || effectiveRestriction.Absence == null) continue;
					var layer = new AbsenceLayer(effectiveRestriction.Absence, part.Period);
					part.CreateAndAddAbsence(layer);
					_schedulePartModifyAndRollbackService.Modify(part);

					var eventArgs = new SchedulingServiceBaseEventArgs(part);
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