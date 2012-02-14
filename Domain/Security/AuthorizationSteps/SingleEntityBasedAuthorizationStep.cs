using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationSteps
{
    /// <summary>
    /// Authorization step that gets the data based on a Domain entity.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 11/26/2007
    /// </remarks>
    public class SingleEntityBasedAuthorizationStep<T> : AuthorizationStep where  T : IAuthorizationEntity
    {

        private readonly IAuthorizationEntityProvider<T> _entityProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleEntityBasedAuthorizationStep{T}"/> class.
        /// </summary>
        /// <param name="entityProvider">The entity provider.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="stepName">Name of the panel.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/26/2007
        /// </remarks>
        public SingleEntityBasedAuthorizationStep(IAuthorizationEntityProvider<T> entityProvider, IAuthorizationStep parent, string stepName)
            : base(parent, stepName)
        {
            _entityProvider = entityProvider;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleEntityBasedAuthorizationStep{T}"/> class.
        /// </summary>
        /// <param name="entityProvider">The entity provider.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="stepName">Name of the panel.</param>
        /// <param name="description">The description.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/26/2007
        /// </remarks>
        public SingleEntityBasedAuthorizationStep(IAuthorizationEntityProvider<T> entityProvider, IAuthorizationStep parent,
            string stepName, string description) : base(parent, stepName, description)
        {
            _entityProvider = entityProvider;
        }

        /// <summary>
        /// Refreshes the own list. Template abstact method
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2007-10-31
        /// </remarks>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/26/2007
        /// </remarks>
        protected override IList<IAuthorizationEntity> RefreshOwnList()
        {
            if (Parents != null && Parents.Count > 0)
                _entityProvider.InputEntityList = Parents[0].ProvidedList<IAuthorizationEntity>();
            IList<T> result = _entityProvider.ResultEntityList;
            if (result == null)
                return null;
            else
                return new List<IAuthorizationEntity>(result.OfType<IAuthorizationEntity>());
        }
    }
}
