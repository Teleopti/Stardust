#region Imports

using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Infrastructure;

#endregion

namespace Teleopti.Ccc.Infrastructure.Licensing
{
    /// <summary>
    /// NHibernate events uses this to check licensing limitations when persisting entities to the database
    /// </summary>
    /// <remarks>
    /// Created by: Klas
    /// Created date: 2008-12-03
    /// </remarks>
    public class CheckLicenseAtPersist : ICheckLicenseAtPersist
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckLicenseAtPersist"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-12-03
        /// </remarks>
        public CheckLicenseAtPersist()
        {
            RootTypesToCheck = new ReadOnlyDictionary<Type, ICheckLicenseForRootType>
                (
                new Dictionary<Type, ICheckLicenseForRootType>
                    {
                        {typeof (Person), new CheckLicenseForPerson()},
                        {typeof(PersonAssignment), new CheckLicenseForPerson()}
                    }
                );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckLicenseAtPersist"/> class.
        /// </summary>
        /// <param name="rootTypesToCheck">The root types to check.</param>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-12-03
        /// </remarks>
        public CheckLicenseAtPersist(IDictionary<Type, ICheckLicenseForRootType> rootTypesToCheck)
        {
            RootTypesToCheck = rootTypesToCheck;
        }

        /// <summary>
        /// Gets or sets the root types to check for license breakage
        /// </summary>
        /// <value>The root types to check.</value>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-12-03
        /// </remarks>
        public IDictionary<Type, ICheckLicenseForRootType> RootTypesToCheck { get; private set; }

        /// <summary>
        /// Verifies that the specified unit of work does not break any license when the modified roots are persisted.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="modifiedRoots">The modified roots.</param>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-12-03
        /// </remarks>
        public void Verify(IUnitOfWork unitOfWork, IEnumerable<IRootChangeInfo> modifiedRoots)
        {
            ICollection<Type> types = new HashSet<Type>();
            modifiedRoots.ForEach(mod => types.Add(mod.Root.GetType()));
            foreach (Type type in types)
            {
                if (RootTypesToCheck.ContainsKey(type))
                    RootTypesToCheck[type].Verify(unitOfWork);
            }
        }
    }
}