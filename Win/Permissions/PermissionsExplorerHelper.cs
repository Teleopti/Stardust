using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Permissions
{
    /// <summary>
    /// Permissions Common State Holder
    /// </summary>
    /// <remarks>
    /// Created by: Muhamad Risath
    /// Created date: 2008-09-09
    /// </remarks>
    public static class PermissionsExplorerHelper
    {
        /// <summary>
        /// Loads all application functions.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-09-09
        /// </remarks>
        public static ICollection<IApplicationFunction> LoadAllApplicationFunctions()
        {
            ICollection<IApplicationFunction> applicationFunctionCollection;

            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                IApplicationFunctionRepository applicationFunctionRepository = new ApplicationFunctionRepository(uow);
                applicationFunctionCollection = applicationFunctionRepository.GetAllApplicationFunctionSortedByCode();
            }

            return applicationFunctionCollection;
        }

        /// <summary>
        /// Gets all available data for one business unit.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-09-09
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static ICollection<IAvailableData> GetAllAvailableDataForOneBusinessUnit()
        {
            ICollection<IAvailableData> availableDataCollection;

            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                // Get All Available Data
                IAvailableDataRepository availableDataRepository = new AvailableDataRepository(uow);
                availableDataCollection = availableDataRepository.LoadAllAvailableData();
            }

            return availableDataCollection;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static ICollection<IPersonInRole> LoadPeopleByApplicationRole(IApplicationRole selectedRole)
        {
        	using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
        	{
                var personRepository = new ApplicationRolePersonRepository(uow);
            	return personRepository.GetPersonsInRole(selectedRole.Id.Value);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static ICollection<IPersonInRole> LoadPeopleNotInApplicationRole(IApplicationRole selectedRole, ICollection<Guid> personsIds)
        {
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
            {
                var personRepository = new ApplicationRolePersonRepository(uow);
                return personRepository.GetPersonsNotInRole(selectedRole.Id.Value, personsIds);
            }
        }

        /// <summary>
        /// Gets the application role collection.
        /// </summary>
        /// <value>The application role collection.</value>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-09-09
        /// </remarks>
        public static ICollection<IApplicationRole> LoadAllApplicationRoles()
        {
            ICollection<IApplicationRole> applicationRoleCollection;

            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                IApplicationRoleRepository repository = new ApplicationRoleRepository(uow);
                applicationRoleCollection = repository.LoadAllApplicationRolesSortedByName();
            }

            return applicationRoleCollection;
        }
    }
}
