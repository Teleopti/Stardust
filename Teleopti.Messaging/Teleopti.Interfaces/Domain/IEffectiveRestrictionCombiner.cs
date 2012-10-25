using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Combines effective restrictions a given effective restriction
	/// </summary>
	public interface IEffectiveRestrictionCombiner
	{
		/// <summary>
		/// Combines effective restrictions to into the given effective restriction
		/// </summary>
		/// <param name="effectiveRestrictions"></param>
		/// <param name="effectiveRestriction"></param>
		/// <returns></returns>
		IEffectiveRestriction CombineEffectiveRestrictions(IEnumerable<IEffectiveRestriction> effectiveRestrictions, IEffectiveRestriction effectiveRestriction);
	}
}