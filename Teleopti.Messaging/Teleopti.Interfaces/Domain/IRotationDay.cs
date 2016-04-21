using System.Collections.ObjectModel;


namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRotationDay
    {
        /// <summary>
        /// Gets the restriction collection.
        /// </summary>
        /// <value>The restriction collection.</value>
        ReadOnlyCollection<IRotationRestriction> RestrictionCollection { get; }

        /// <summary>
        /// Gets the index.
        /// </summary>
        /// <value>The index.</value>
        int Index { get; }

        /// <summary>
        /// Significants the restriction.
        /// </summary>
        /// <returns></returns>
        IRotationRestriction SignificantRestriction();
    }
}
