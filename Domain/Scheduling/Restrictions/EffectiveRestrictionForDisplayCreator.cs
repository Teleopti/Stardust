using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class EffectiveRestrictionForDisplayCreator : IEffectiveRestrictionForDisplayCreator
	{
		private readonly IRestrictionCombiner _restrictionCombiner;
		private readonly IRestrictionRetrivalOperation _retrivalOperation;

		public EffectiveRestrictionForDisplayCreator(IRestrictionCombiner restrictionCombiner, IRestrictionRetrivalOperation retrivalOperation)
		{
			_restrictionCombiner = restrictionCombiner;
			_retrivalOperation = retrivalOperation;
		}

		public IEffectiveRestriction GetEffectiveRestrictionForDisplay(IScheduleDay scheduleDay, IEffectiveRestrictionOptions effectiveRestrictionOptions)
		{
			var startTime = new TimeSpan(0, 0, 0);
			var endTime = new TimeSpan(23, 59, 59);
			var endEndTime = new TimeSpan(1, 23, 59, 59);

			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(startTime, endTime), new EndTimeLimitation(startTime, endEndTime), new WorkTimeLimitation(startTime, endTime), null, null, null, new List<IActivityRestriction>());

			if (scheduleDay != null && effectiveRestrictionOptions != null)
			{
				if (effectiveRestrictionOptions.UsePreference)
				{
					effectiveRestriction = _restrictionCombiner.CombinePreferenceRestrictions(
						_retrivalOperation.GetPreferenceRestrictions(scheduleDay.RestrictionCollection()),
						effectiveRestriction, false);
				}

				if (effectiveRestrictionOptions.UseAvailability)
				{
					effectiveRestriction = _restrictionCombiner.CombineAvailabilityRestrictions(
						_retrivalOperation.GetAvailabilityRestrictions(scheduleDay.RestrictionCollection()),
						effectiveRestriction);
				}
			}

			return effectiveRestriction;
		}
	}
}