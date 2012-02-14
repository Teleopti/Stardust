namespace Teleopti.Interfaces.Infrastructure
{
    /// <summary>
    /// Defines a queryfilter.
    /// Used to enable/disable filter on SELECT clauses.
    /// To get an IQueryFilter instance, use QueryFilter.Xxx
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2009-05-04
    /// </remarks>
    public interface IQueryFilter
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-05-04
        /// </remarks>
        string Name { get; }
    }
}
