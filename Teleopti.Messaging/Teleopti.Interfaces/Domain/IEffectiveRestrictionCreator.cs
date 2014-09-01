using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Extracts and creates selected restrictions
	/// </summary>
	public interface IEffectiveRestrictionCreator
	{
		/// <summary>
		/// Gets the effective restriction according to options.
		/// </summary>
		/// <param name="part">The part.</param>
		/// <param name="options">The options.</param>
		/// <returns></returns>
		IEffectiveRestriction GetEffectiveRestriction(IScheduleDay part, ISchedulingOptions options);

		/// <summary>
		/// Gets the effective restriction.
		/// </summary>
		/// <param name="groupPersons">The group persons.</param>
		/// <param name="dateOnly">The date only.</param>
		/// <param name="options">The options.</param>
		/// <param name="scheduleDictionary">The schedule dictionary.</param>
		/// <returns></returns>
		IEffectiveRestriction GetEffectiveRestriction(IEnumerable<IPerson> groupPersons, DateOnly dateOnly, ISchedulingOptions options, IScheduleDictionary scheduleDictionary);

		/// <summary>
		/// Get the effective restrictions for a person
		/// </summary>
		/// <param name="person">The person</param>
		/// <param name="dateOnly">The date only.</param>
		/// <param name="options">The options.</param>
		/// <param name="scheduleDictionary">The schedule dictionary.</param>
		/// <returns></returns>
		IEffectiveRestriction GetEffectiveRestrictionForSinglePerson(
			IPerson person,
			DateOnly dateOnly,
			ISchedulingOptions options,
			IScheduleDictionary scheduleDictionary);
	}
}