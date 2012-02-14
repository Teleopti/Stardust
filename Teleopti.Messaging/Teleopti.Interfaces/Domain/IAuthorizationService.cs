using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for AuthorizationSerrvice
    /// </summary>
    public interface IAuthorizationService 
    {
        /// <summary>
        /// Loads the rights.
        /// </summary>
        /// <param name="person">The current person.</param>
        /// <param name="settings">The current settings.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/28/2007
        /// </remarks>
        void LoadPermissions(IPerson person, IAuthorizationSettings settings);

        /// <summary>
        /// Refreshes the permissions using the same person and authorization settings.
        /// </summary>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/28/2007
        /// </remarks>
        void RefreshPermissions();

        /// <summary>
        /// Gets or sets the granted and licensed application functions.
        /// </summary>
        /// <value>The permitted application functions.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/4/2007
        /// </remarks>
        IList<IApplicationFunction> PermittedApplicationFunctions { get; }

        /// <summary>
        /// Gets the granted and licensed application module functions. That is to return all the permitted functions 
        /// that has a hierachy level of 1 and has SortOrder value.
        /// </summary>
        /// <value>The permitted application module functions.</value>
        IList<IApplicationFunction> PermittedApplicationModules { get; }

        /// <summary>
        /// Gets or sets the granted application functions.
        /// </summary>
        /// <value>The granted application functions.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/4/2007
        /// </remarks>
        IList<IApplicationFunction> GrantedApplicationFunctions { get; }

        /// <summary>
        /// Gets the licensed application module functions. That is to return all the licensed application functions 
        /// that has a hierachy Level of 1 and has SortOrder value.
        /// </summary>
        /// <value>The available application module functions.</value>
        IList<IApplicationFunction> LicensedApplicationModules{ get;}

        /// <summary>
        /// Gets all the application modules. That is to return all the application functions 
        /// that has a hierachy Level of 1 and has SortOrder value.
        /// </summary>
        /// <value>All application modules.</value>
        IList<IApplicationFunction> AllApplicationModules { get; }

        /// <summary>
        /// Gets or sets the current authorization settings.
        /// </summary>
        /// <value>The current authorization settings.</value>
        /// <remarks>
        /// Created by: tamabs
        /// Created date: 11/28/2007
        /// </remarks>
        IAuthorizationSettings CurrentAuthorizationSettings { get; set; }

        /// <summary>
        /// Gets the collection authorization steps.
        /// </summary>
        /// <value>The authorization steps.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/28/2007
        /// </remarks>
        IDictionary<string, IAuthorizationStep> AuthorizationStepCollection { get; }

        /// <summary>
        /// Gets all the defined application functions (licensed + not licensed).
        /// </summary>
        /// <value>The application functions.</value>
        IList<IApplicationFunction> AllApplicationFunctions { get; }

        /// <summary>
        /// Gets the licensed application functions.
        /// </summary>
        /// <value>The licensed application functions.</value>
        IList<IApplicationFunction> LicensedApplicationFunctions { get; }

        /// <summary>
        /// Gets the aggregated permitted data made from the AvailableData lists and the specified application function.
        /// </summary>
        /// <param name="applicationFunction"></param>
        /// <returns></returns>
        /// <value>The permitted data list.</value>
        IAvailableData AggregatedPermittedData(IApplicationFunction applicationFunction);

        /// <summary>
        /// Finds the authorisation step provided list by the step name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/28/2007
        /// </remarks>
        IList<T> FindAuthorizationStepProvidedList<T>(string key) where T : IAuthorizationEntity;

        /// <summary>
        /// Starts a large number of queries. The AuthorizationService will get to an optimized mode which inner data caching which result in a
        /// quicker response time. As in cached mode the AuthorizationService does not follow the changes in the permission settings, always
        /// remember to stop the optimized mode with calling EndBunchQueries method.
        /// </summary>
        void StartBunchQueries();

        /// <summary>
        /// Stops a large number of queries. With the StartBunchQueries call, The AuthorizationService will get to an optimized mode which
        /// results in a quicker response time. EndBunchQueries method will stop the optimized mode. As in cached mode the AuthorizationService
        /// does not follow the changes in the permission settings, always remember to stop the optimized mode with calling EndBunchQueries method.
        /// </summary>
        void EndBunchQueries();

        /// <summary>
        /// Loads permissions
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="settings">settings.</param>
        /// <param name="unitOfWork">uow</param>
        /// <returns></returns>
        void LoadPermissions(IPerson person, IAuthorizationSettings settings, IUnitOfWork unitOfWork);
    }
}