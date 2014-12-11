using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class EffectiveRestrictionForDisplayCreator : IEffectiveRestrictionForDisplayCreator
	{
		private readonly IRestrictionRetrievalOperation _retrievalOperation;
		private readonly IRestrictionCombiner _restrictionCombiner;

		public EffectiveRestrictionForDisplayCreator(IRestrictionRetrievalOperation retrievalOperation, IRestrictionCombiner restrictionCombiner)
		{
			_retrievalOperation = retrievalOperation;
			_restrictionCombiner = restrictionCombiner;
		}

		public IEffectiveRestriction MakeEffectiveRestriction(IScheduleDay scheduleDay, IEffectiveRestrictionOptions effectiveRestrictionOptions)
		{
			var startTime = new TimeSpan(0, 0, 0);
			var endTime = new TimeSpan(23, 59, 59);
			var endEndTime = new TimeSpan(1, 23, 59, 59);

			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(startTime, endTime), new EndTimeLimitation(startTime, endEndTime), new WorkTimeLimitation(startTime, endTime), null, null, null, new List<IActivityRestriction>());

			if (scheduleDay != null && effectiveRestrictionOptions != null)
			{
				var restrictions = scheduleDay.RestrictionCollection();

				if (effectiveRestrictionOptions.UsePreference)
				{
					if (restrictions != null)
						effectiveRestriction = _restrictionCombiner.CombinePreferenceRestrictions(
							_retrievalOperation.GetPreferenceRestrictions(restrictions),
							effectiveRestriction, false);
				}

				if (effectiveRestrictionOptions.UseAvailability)
				{
					if (restrictions != null)
						effectiveRestriction = _restrictionCombiner.CombineAvailabilityRestrictions(
							_retrievalOperation.GetAvailabilityRestrictions(restrictions),
							effectiveRestriction);
				}

				if (effectiveRestrictionOptions.UseStudentAvailability)
				{
					if (restrictions != null)
						effectiveRestriction = _restrictionCombiner.CombineStudentAvailabilityRestrictions(
							_retrievalOperation.GetStudentAvailabilityRestrictions(restrictions),
							effectiveRestriction);
				}

			}

			return effectiveRestriction;
		}
	}
}