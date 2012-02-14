using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.AuthorizationSteps;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.Security
{
    /// <summary>
    /// Testable Authorization step that gets the data based on a Domain entity.
    /// </summary>
    public class SingleEntityBasedAuthorizationStepTestClass : SingleEntityBasedAuthorizationStep<AuthorizationEntity>
    {

        private IList<IAuthorizationEntity> _storedResultListFromRefreshOwnListMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleEntityBasedAuthorizationStepTestClass"/> class.
        /// </summary>
        /// <param name="entityProvider">The entity provider.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="stepName">Name of the step.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public SingleEntityBasedAuthorizationStepTestClass(
            IAuthorizationEntityProvider<AuthorizationEntity> entityProvider,
            IAuthorizationStep parent,
            string stepName)
            : base(entityProvider, parent, stepName)
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleEntityBasedAuthorizationStepTestClass"/> class.
        /// </summary>
        /// <param name="entityProvider">The entity provider.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="stepName">Name of the step.</param>
        /// <param name="description">The description.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public SingleEntityBasedAuthorizationStepTestClass(
            IAuthorizationEntityProvider<AuthorizationEntity> entityProvider,
            IAuthorizationStep parent,
            string stepName, 
            string description)
            : base(entityProvider, parent, stepName, description)
        {
            //
        }

        /// <summary>
        /// Refreshes the own list. Template abstact method
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Testable modifications.
        /// </remarks>
        protected override IList<IAuthorizationEntity> RefreshOwnList()
        {
            StoredResultListFromRefreshOwnListMethod = base.RefreshOwnList();
            return StoredResultListFromRefreshOwnListMethod;
        }

        /// <summary>
        /// Gets or sets the stored result list from refresh own list method.
        /// </summary>
        /// <value>The stored result list from refresh own list method.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public IList<IAuthorizationEntity> StoredResultListFromRefreshOwnListMethod
        {
            get { return _storedResultListFromRefreshOwnListMethod; }
            set { _storedResultListFromRefreshOwnListMethod = value; }
        }
    }
}
