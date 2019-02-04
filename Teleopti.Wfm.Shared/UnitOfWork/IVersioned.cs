
namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// A version number and a bool telling if it's used.
    /// Primarly used for optimistic lock checks.
    /// Note - Don't use this interface for any business meaning!
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-09-25
    /// </remarks>
    public interface IVersioned
    {
        /// <summary>
        /// Gets the version of this entity.
        /// </summary>
        /// <value>The version.</value>
        int? Version { get; }

        /// <summary>
        /// Sets the version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-11-16
        /// </remarks>
        void SetVersion(int version);
    }
}
