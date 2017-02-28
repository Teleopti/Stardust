using System;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Layer that can be used by IEffectiveRestriction.VisualLayerCollectionSatisfiesActivityRestriction
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Restrictable")]
	public interface IActivityRestrictableVisualLayer : IPeriodized
	{
		/// <summary>
		/// The activity id
		/// </summary>
		Guid ActivityId { get; }
	}
}