using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IPersonRotationRepository : IRepository<IPersonRotation>
    {
        /// <summary>
        /// Finds the person rotations for one person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-01
        /// </remarks>
        IList<IPersonRotation> Find(IPerson person);

        /// <summary>
        /// Finds the person rotations by persons.
        /// </summary>
        /// <param name="persons">The persons.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-07-01
        /// </remarks>
        IList<IPersonRotation> Find(IList<IPerson> persons);

		/// <summary>
		/// Loads person rotations with underlying objects
		/// </summary>
		/// <param name="persons"></param>
		/// <param name="startDate"></param>
		/// <returns></returns>
		IEnumerable<IPersonRotation> LoadPersonRotationsWithHierarchyData(IEnumerable<IPerson> persons, DateOnly startDate);
    }
}