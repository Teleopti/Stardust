namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Templates for multisite periods
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-02-11
    /// </remarks>
    public interface ITemplateMultisitePeriod : IMultisiteData, IAggregateEntity, ICloneableEntity<ITemplateMultisitePeriod>
    {
        /// <summary>
        /// Gets the version of this entity.
        /// </summary>
        /// <value>The version.</value>
        int? Version { get; }
		bool IsDistributionChangeNotAllowed { get; set; }
	}
}