using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Interface for restrictions
    /// </summary>
    public interface IRestriction<T> where T : IAggregateRoot
    {
        /// <summary>
        /// Checks the entity.
        /// </summary>
        /// <param name="entityToCheck">The entity to check.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-07
        /// </remarks>
        void CheckEntity(T entityToCheck);
    }
}