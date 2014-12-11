using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Combines restrictions of different types into a given effective restriction
	/// </summary>
	public interface IRestrictionCombiner
	{
		/// <summary>
		/// Combines preference restrictions to into the given effective restriction
		/// </summary>
		/// <param name="preferenceRestrictions"></param>
		/// <param name="effectiveRestriction"></param>
		/// <param name="mustHavesOnly"></param>
		/// <returns></returns>
		IEffectiveRestriction CombinePreferenceRestrictions(IEnumerable<IPreferenceRestriction> preferenceRestrictions, IEffectiveRestriction effectiveRestriction, bool mustHavesOnly);
		
		/// <summary>
		/// Combines availability restrictions to into the given effective restriction
		/// </summary>
		/// <param name="availabilityRestrictions"></param>
		/// <param name="effectiveRestriction"></param>
		/// <returns></returns>
		IEffectiveRestriction CombineAvailabilityRestrictions(IEnumerable<IAvailabilityRestriction> availabilityRestrictions, IEffectiveRestriction effectiveRestriction);
		
		/// <summary>
		/// Combines student availability days restrictions to into the given effective restriction
		/// </summary>
		/// <param name="studentAvailabilityDays"></param>
		/// <param name="effectiveRestriction"></param>
		/// <returns></returns>
		IEffectiveRestriction CombineStudentAvailabilityDays(IEnumerable<IStudentAvailabilityDay> studentAvailabilityDays, IEffectiveRestriction effectiveRestriction);

		/// <summary>
		/// Combines rotation restrictions to into the given effective restriction
		/// </summary>
		/// <param name="rotationRestrictions"></param>
		/// <param name="effectiveRestriction"></param>
		/// <returns></returns>
		IEffectiveRestriction CombineRotationRestrictions(IEnumerable<IRotationRestriction> rotationRestrictions, IEffectiveRestriction effectiveRestriction);

		/// <summary>
		/// Combines student availability restrictions into the given effective restriction
		/// </summary>
		/// <param name="studentAvailabilityRestrictions"></param>
		/// <param name="effectiveRestriction"></param>
		/// <returns></returns>
		IEffectiveRestriction CombineStudentAvailabilityRestrictions(IEnumerable<IStudentAvailabilityRestriction> studentAvailabilityRestrictions, IEffectiveRestriction effectiveRestriction);
		
	}
}