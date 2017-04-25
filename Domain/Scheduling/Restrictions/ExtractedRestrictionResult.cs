using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class ExtractedRestrictionResult : IExtractedRestrictionResult
	{
		private readonly IRestrictionCombiner _restrictionCombiner;

		public ExtractedRestrictionResult(IRestrictionCombiner restrictionCombiner, IEnumerable<IRotationRestriction> rotationRestrictions, IEnumerable<IAvailabilityRestriction> availabilityRestrictions, IEnumerable<IPreferenceRestriction> preferenceRestrictions, IEnumerable<IStudentAvailabilityDay> studentAvailabilityDays)
		{
			_restrictionCombiner = restrictionCombiner;
			AvailabilityList = availabilityRestrictions.ToList();
			RotationList = rotationRestrictions.ToList();
			StudentAvailabilityList = studentAvailabilityDays.ToList();
			PreferenceList = preferenceRestrictions.ToList();
		}

		/// <summary>
		/// Gets the availability list.
		/// </summary>
		/// <value>The availability list.</value>
		public IEnumerable<IAvailabilityRestriction> AvailabilityList { get; private set; }

		/// <summary>
		/// Gets the rotation list.
		/// </summary>
		/// <value>The rotation list.</value>
		public IEnumerable<IRotationRestriction> RotationList { get; private set; }

		/// <summary>
		/// Gets the student availability list.
		/// </summary>
		/// <value>The student availability list.</value>
		public IEnumerable<IStudentAvailabilityDay> StudentAvailabilityList { get; private set; }

		/// <summary>
		/// Gets the preference list.
		/// </summary>
		/// <value>The preference list.</value>
		public IEnumerable<IPreferenceRestriction> PreferenceList { get; private set; }

		/// <summary>
		/// Combineds the restriction.
		/// </summary>
		/// <param name="schedulingOptions">The scheduling options.</param>
		/// <returns></returns>
		public IEffectiveRestriction CombinedRestriction(SchedulingOptions schedulingOptions)
		{
			var start = new StartTimeLimitation();
			var end = new EndTimeLimitation();
			var time = new WorkTimeLimitation();

			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(start, end, time, null, null, null, new List<IActivityRestriction>());
			if (schedulingOptions.UseRotations)
			{
				effectiveRestriction = extractRotations(effectiveRestriction);
				if (effectiveRestriction == null) return effectiveRestriction;
			}

			if (schedulingOptions.UsePreferences)
			{
				effectiveRestriction = extractPreferences(effectiveRestriction, schedulingOptions);
				if (effectiveRestriction == null) return effectiveRestriction;
			}
			// if it IsLimitedWorkday at this point we shall not add a dayoff 
			if (isLimitedWorkday(effectiveRestriction))
				effectiveRestriction.NotAllowedForDayOffs = true;

			if (schedulingOptions.UseAvailability)
			{
				effectiveRestriction = extractAvailabilities(effectiveRestriction);
				if (effectiveRestriction == null) return effectiveRestriction;
			}

			if (schedulingOptions.UseStudentAvailability)
			{
				effectiveRestriction = extractStudentAvailabilities(effectiveRestriction);
				if (effectiveRestriction == null) return effectiveRestriction;
			}

			return effectiveRestriction;
		}

		private static bool isLimitedWorkday(IEffectiveRestriction effectiveRestriction)
		{
			if (effectiveRestriction.ShiftCategory != null)
				return true;
			if (effectiveRestriction.ActivityRestrictionCollection.Count > 0)
				return true;

			return (effectiveRestriction.StartTimeLimitation.HasValue() || effectiveRestriction.EndTimeLimitation.HasValue() || effectiveRestriction.WorkTimeLimitation.HasValue());
		}

		private IEffectiveRestriction extractStudentAvailabilities(IEffectiveRestriction effectiveRestriction)
		{
			return _restrictionCombiner.CombineStudentAvailabilityDays(StudentAvailabilityList, effectiveRestriction);
		}

		private IEffectiveRestriction extractAvailabilities(IEffectiveRestriction effectiveRestriction)
		{
			return _restrictionCombiner.CombineAvailabilityRestrictions(AvailabilityList, effectiveRestriction);
		}

		private IEffectiveRestriction extractPreferences(IEffectiveRestriction effectiveRestriction, SchedulingOptions schedulingOptions)
		{
			return _restrictionCombiner.CombinePreferenceRestrictions(PreferenceList, effectiveRestriction, schedulingOptions.UsePreferencesMustHaveOnly);
		}

		private IEffectiveRestriction extractRotations(IEffectiveRestriction effectiveRestriction)
		{
			return _restrictionCombiner.CombineRotationRestrictions(RotationList, effectiveRestriction);
		}
	}
}