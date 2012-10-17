using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class EffectiveRestrictionForDisplayCreator : IEffectiveRestrictionForDisplayCreator
	{
		private readonly IRestrictionCombiner _restrictionCombiner;
		private readonly IRestrictionRetrievalOperation _retrievalOperation;

		public EffectiveRestrictionForDisplayCreator(IRestrictionCombiner restrictionCombiner, IRestrictionRetrievalOperation retrievalOperation)
		{
			_restrictionCombiner = restrictionCombiner;
			_retrievalOperation = retrievalOperation;
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
						_retrievalOperation.GetPreferenceRestrictions(scheduleDay.RestrictionCollection()),
						effectiveRestriction, false);
				}

				if (effectiveRestrictionOptions.UseAvailability)
				{
					effectiveRestriction = _restrictionCombiner.CombineAvailabilityRestrictions(
						_retrievalOperation.GetAvailabilityRestrictions(scheduleDay.RestrictionCollection()),
						effectiveRestriction);
				}
			}

			return effectiveRestriction;
		}
	}
}