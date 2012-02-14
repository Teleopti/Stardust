using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationSteps
{
    /// <summary>
    /// Application role entity provider
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 11/27/2007
    /// </remarks>
    public class AllDefinedApplicationFunctionsProvider : IAuthorizationEntityProvider<IApplicationFunction>
    {
        private readonly IApplicationFunctionRepository _rep;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllDefinedApplicationFunctionsProvider"/> class.
        /// </summary>
        /// <param name="repository">The rep factory.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public AllDefinedApplicationFunctionsProvider(IApplicationFunctionRepository repository)
        {
            _rep = repository;
        }

        #region IAuthorizationEntityProvider<ApplicationFunction> Members

        /// <summary>
        /// Gets the result entity list.
        /// </summary>
        /// <value>The result entity list.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public IList<IApplicationFunction> ResultEntityList
        {
            get 
            {
                IList<IApplicationFunction> resultList = _rep.GetAllApplicationFunctionSortedByCode();

                return resultList;
            }
        }

        /// <summary>
        /// Sets the parent entity list.
        /// </summary>
        /// <value>The parent entity list.</value>
        /// <remarks>
        /// Used for setting the parent result when needed for the operation.
        /// </remarks>
        public IList<IAuthorizationEntity> InputEntityList
        {
            set { throw new NotImplementedException(); }
        }

        #endregion
    }
}
