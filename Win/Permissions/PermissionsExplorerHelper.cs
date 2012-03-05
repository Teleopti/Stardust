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

        public static IPerson GetPerson(Guid id)
        {
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var personRepository = new PersonRepository(uow);
                return personRepository.Load(id);
            }
        }

    	/// <summary>
        /// Loads the application function for role.
        /// </summary>
        /// <param name="selectedRole">The selected role.</param>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-09-09
        /// </remarks>
        public static IList<IApplicationFunction> LoadApplicationFunctionForRole(IApplicationRole selectedRole, IUnitOfWork unitOfWork)
        {
            IList<IApplicationFunction> applicationFunctionCollection;

            if (unitOfWork == null)
            {
                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    uow.Reassociate(selectedRole);
                    applicationFunctionCollection = selectedRole.ApplicationFunctionCollection.ToList();
                }
            }
            else
            {
                applicationFunctionCollection = selectedRole.ApplicationFunctionCollection.ToList();
            }

            return applicationFunctionCollection;
        }

        /// <summary>
        /// Gets the current team for person.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public static string GetCurrentTeamForPerson(Guid? id)
        {
            string teamName = string.Empty;

            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                IPersonRepository repository = new PersonRepository(uow);
                IPerson person = repository.Load(id.GetValueOrDefault());
                IPersonPeriod wrapper = person.Period(DateOnly.Today);
                if (wrapper!=null)
                {
                    teamName = wrapper.Team.Description.ToString();
                }
            }

            return teamName;
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
