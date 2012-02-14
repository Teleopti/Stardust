using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Base class for restriction sets
    /// </summary>
    public abstract class RestrictionSet<T> : IRestrictionSet, IRestrictionSet<T> where T : IAggregateRoot
    {
        /// <summary>
        /// Collection of restrictions
        /// </summary>
        private readonly IList<IRestriction<T>> _restrictions;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestrictionSet&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="restrictions">The restrictions.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-07
        /// </remarks>
        protected RestrictionSet(IEnumerable<IRestriction<T>> restrictions)
        {
            _restrictions = new List<IRestriction<T>>(restrictions);
        }

        /// <summary>
        /// Checks the entity.
        /// </summary>
        /// <param name="entityToCheck">The entity to check.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-07
        /// </remarks>
        public void CheckEntity(object entityToCheck)
        {
            if (entityToCheck is T)
                CheckEntity((T) entityToCheck);
        }

        /// <summary>
        /// Checks the entity.
        /// </summary>
        /// <param name="entityToCheck">The entity to check.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-07
        /// </remarks>
        public void CheckEntity(T entityToCheck)
        {
            foreach (IRestriction<T> restriction in _restrictions)
                restriction.CheckEntity(entityToCheck);
        }
    }
}