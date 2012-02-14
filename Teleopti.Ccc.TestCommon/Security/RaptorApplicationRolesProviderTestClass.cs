using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationSteps;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.Security
{
    /// <summary>
    /// Testable Authorization step that gets the data based on a Domain entity.
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 11/26/2007
    /// </remarks>
    public class RaptorApplicationRolesProviderTestClass : RaptorApplicationRolesProvider
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="RaptorApplicationRolesProviderTestClass"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public RaptorApplicationRolesProviderTestClass(IPerson person, IPersonRepository repository)
            : base(person, repository)
        {
            //
        }

        /// <summary>
        /// Gets the protected InputEntityList in the base to be testable.
        /// </summary>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public IList<IAuthorizationEntity> GetInputEntityList()
        {
           return InputEntityList;
        }

    }
}
