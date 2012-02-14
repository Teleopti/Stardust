using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;


namespace Teleopti.Ccc.Infrastructure.Licensing
{
    /// <summary>
    /// Used for checking license limitations related to the Person Aggregate Root, i.e. maximum number of active agents.
    /// </summary>
    /// <remarks>
    /// Created by: Klas
    /// Created date: 2008-12-03
    /// </remarks>
    public class CheckLicenseForPerson : ICheckLicenseForRootType
    {
        public static void Verify(IPersonRepository personRepository, ILicenseActivator license)
        {
            if (license != null)
            {
                int numberOfActiveAgents = personRepository.NumberOfActiveAgents();
                if (license.IsThisTooManyActiveAgents(numberOfActiveAgents))
                {
                	if (license.LicenseType.Equals(LicenseType.Agent))
					{
						throw new TooManyActiveAgentsException(license.MaxActiveAgents, numberOfActiveAgents, license.LicenseType);
					}
                	throw new TooManyActiveAgentsException(license.MaxSeats, numberOfActiveAgents, license.LicenseType);
                }
            }
        }

        /// <summary>
        /// Verifies that the specified unit of work does not break any license when called.
        /// </summary>
        /// <param name="unitOfWork">The unit of work tested.</param>
        /// <remarks>
        /// A null license is ok, the reason that we allow it is so that conversion will work
        /// Created by: Klas
        /// Created date: 2008-12-03
        /// </remarks>
        public void Verify(IUnitOfWork unitOfWork)
        {
            Verify(new PersonRepository(unitOfWork), DefinedLicenseDataFactory.LicenseActivator);
        }
    }
}