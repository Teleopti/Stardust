using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IExtractedRestrictionResult
	{
		/// <summary>
		/// Gets the availability list.
		/// </summary>
		/// <value>The availability list.</value>
		IEnumerable<IAvailabilityRestriction> AvailabilityList { get; }

		/// <summary>
		/// Gets the rotation list.
		/// </summary>
		/// <value>The rotation list.</value>
		IEnumerable<IRotationRestriction> RotationList { get; }

		/// <summary>
		/// Gets the student availability list.
		/// </summary>
		/// <value>The student availability list.</value>
		IEnumerable<IStudentAvailabilityDay> StudentAvailabilityList { get; }

		/// <summary>
		/// Gets the preference list.
		/// </summary>
		/// <value>The preference list.</value>
		IEnumerable<IPreferenceRestriction> PreferenceList { get; }

		/// <summary>
		/// Combineds the restriction.
		/// </summary>
		/// <param name="schedulingOptions">The scheduling options.</param>
		/// <returns></returns>
		IEffectiveRestriction CombinedRestriction(SchedulingOptions schedulingOptions);
	}
}