using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationSteps
{
    /// <summary>
    /// Authorization entity provider interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAuthorizationEntityProvider<T> where T : IAuthorizationEntity
    {
        /// <summary>
        /// Gets the result entity list.
        /// </summary>
        /// <value>The result entity list.</value>
        IList<T> ResultEntityList { get; }
        
        /// <summary>
        /// Sets the parent entity list.
        /// </summary>
        /// <value>The parent entity list.</value>
        /// <remarks>
        /// Used for setting the parent result when needed for the operation.
        /// </remarks>
        IList<IAuthorizationEntity> InputEntityList { set;}
        
    }

}
