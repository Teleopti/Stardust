using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class RestrictionCombiner : IRestrictionCombiner, IEffectiveRestrictionCombiner
	{
		public IEffectiveRestriction CombinePreferenceRestrictions(IEnumerable<IPreferenceRestriction> preferenceRestrictions, IEffectiveRestriction effectiveRestriction, bool mustHavesOnly)
		{
			preferenceRestrictions = from r in preferenceRestrictions where r.IsRestriction() select r;
			if (mustHavesOnly)
				preferenceRestrictions = from r in preferenceRestrictions where r.MustHave select r;

			var asEffectiveRestrictions = from r in preferenceRestrictions
			                              select new EffectiveRestriction(
			                                     	r.StartTimeLimitation,
			                                     	r.EndTimeLimitation,
			                                     	r.WorkTimeLimitation,
			                                     	r.ShiftCategory,
			                                     	r.DayOffTemplate,
			                                     	r.Absence,
			                                     	r.ActivityRestrictionCollection
			                                     	)
			                                     	{
			                                     		IsPreferenceDay = true
			                                     	} as IEffectiveRestriction;

			return CombineEffectiveRestrictions(asEffectiveRestrictions, effectiveRestriction);
		}

		public IEffectiveRestriction CombineAvailabilityRestrictions(IEnumerable<IAvailabilityRestriction> availabilityRestrictions, IEffectiveRestriction effectiveRestriction)
		{
			availabilityRestrictions = from r in availabilityRestrictions where r.IsRestriction() select r;

			var asEffectiveRestrictions = from r in availabilityRestrictions
			                              select (IEffectiveRestriction) new EffectiveRestriction(
			                                                             	r.StartTimeLimitation,
			                                                             	r.EndTimeLimitation,
			                                                             	r.WorkTimeLimitation,
			                                                             	null,
			                                                             	null,
			                                                             	null,
			                                                             	new List<IActivityRestriction>()
			                                                             	)
			                                                             	{
			                                                             		IsAvailabilityDay = true,
			                                                             	    NotAvailable = r.NotAvailable,
                                                                            };
			return CombineEffectiveRestrictions(asEffectiveRestrictions, effectiveRestriction);
		}

		public IEffectiveRestriction CombineStudentAvailabilityRestrictions(IEnumerable<IStudentAvailabilityRestriction> studentAvailabilityRestrictions, IEffectiveRestriction effectiveRestriction)
		{
			studentAvailabilityRestrictions = from r in studentAvailabilityRestrictions where r.IsRestriction() select r;

			var asEffectiveRestrictions = from r in studentAvailabilityRestrictions
										  select (IEffectiveRestriction)new EffectiveRestriction(
																			r.StartTimeLimitation,
																			r.EndTimeLimitation,
																			r.WorkTimeLimitation,
																			null,
																			null,
																			null,
																			new List<IActivityRestriction>()
																			)
										  {
											  IsAvailabilityDay = true,
										  };
			return CombineEffectiveRestrictions(asEffectiveRestrictions, effectiveRestriction);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public IEffectiveRestriction CombineStudentAvailabilityDays(IEnumerable<IStudentAvailabilityDay> studentAvailabilityDays, IEffectiveRestriction effectiveRestriction)
		{
			if (studentAvailabilityDays.IsEmpty())
			{
				effectiveRestriction.NotAvailable = true;
				return effectiveRestriction;
			}

			var studentAvailabilityRestrictions = from d in studentAvailabilityDays
			                                      let r = d.RestrictionCollection.FirstOrDefault()
			                                      where r != null
			                                      select r;

			var asEffectiveRestrictions = from r in studentAvailabilityRestrictions
			                              select new EffectiveRestriction(
			                                     	r.StartTimeLimitation,
			                                     	r.EndTimeLimitation,
			                                     	r.WorkTimeLimitation,
			                                     	null,
			                                     	null,
			                                     	null,
			                                     	new List<IActivityRestriction>()
			                                     	)
			                                     	{
			                                     		IsStudentAvailabilityDay = true
			                                     	} as IEffectiveRestriction;

			return CombineEffectiveRestrictions(asEffectiveRestrictions, effectiveRestriction);
		}

		public IEffectiveRestriction CombineRotationRestrictions(IEnumerable<IRotationRestriction> rotationRestrictions, IEffectiveRestriction effectiveRestriction)
		{
			rotationRestrictions = from r in rotationRestrictions where r.IsRestriction() select r;

			var asEffectiveRestrictions = from r in rotationRestrictions
			                              select new EffectiveRestriction(
			                                     	r.StartTimeLimitation,
			                                     	r.EndTimeLimitation,
			                                     	r.WorkTimeLimitation,
			                                     	r.ShiftCategory,
			                                     	r.DayOffTemplate,
			                                     	null,
			                                     	new List<IActivityRestriction>())
			                                     	{
			                                     		IsRotationDay = true
			                                     	}
			                                     as IEffectiveRestriction
				;

			return CombineEffectiveRestrictions(asEffectiveRestrictions, effectiveRestriction);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IEffectiveRestriction CombineEffectiveRestrictions(IEnumerable<IEffectiveRestriction> effectiveRestrictions, IEffectiveRestriction  effectiveRestriction)
		{
			foreach (var restriction in effectiveRestrictions)
			{
				effectiveRestriction = effectiveRestriction.Combine(restriction);
				if (effectiveRestriction == null) return null;
			}
			return effectiveRestriction;
		}
	}
}