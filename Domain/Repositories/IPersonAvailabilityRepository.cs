using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	/// <summary>
	///  Interface for AgentAvailabilityRepository
	/// </summary>
	public interface IPersonAvailabilityRepository : IRepository<IPersonAvailability>
	{
		/// <summary>
		/// Finds the specified availability periods.
		/// </summary>
		/// <param name="persons">The agents.</param>
		/// <param name="period">The period.</param>
		/// <returns></returns>
		ICollection<IPersonAvailability> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period);

		/// <summary>
		/// Loads availabilities with hierarchy data for persons.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IPersonAvailability> LoadPersonAvailabilityWithHierarchyData(IEnumerable<IPerson> persons, DateOnly startDate);
	}
}
