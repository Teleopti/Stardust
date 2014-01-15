using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{

    /// <summary>
    /// Simple authorization entity implementation.
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 11/23/2007
    /// </remarks>
    public class AuthorizationEntity : Entity, IAuthorizationEntity
    {

        private string _authorizationKey;
        private string _authorizationName;
        private string _authorizationDescription;
        private string _authorizationValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationEntity"/> class.
        /// </summary>
        /// <param name="authorizationKey">The authorization key.</param>
        /// <param name="authorizationName">The name value.</param>
        /// <param name="authorizationDescription">The description value.</param>
        /// <param name="authorizationValue">The authorization value.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/23/2007
        /// </remarks>
        public AuthorizationEntity(string authorizationKey, string authorizationName, string authorizationDescription, string authorizationValue)
        {
            _authorizationKey = authorizationKey;
            _authorizationName = authorizationName;
            _authorizationDescription = authorizationDescription;
            _authorizationValue = authorizationValue;
        }

        #region IAuthorizationEntity Members

        /// <summary>
        /// Gets the Name value. Usually this is the key.
        /// </summary>
        /// <value>The name field.</value>
        public virtual string AuthorizationKey
        {
            get
            {
                return _authorizationKey;
            }
            set
            {
                _authorizationKey = value;
            }
        }

        /// <summary>
        /// Gets the Name value. Usually this is the key.
        /// </summary>
        /// <value>The name field.</value>
        public virtual string AuthorizationName
        {
            get
            {
                return _authorizationName;
            }
            set
            {
                _authorizationName = value;
            }
        }

        /// <summary>
        /// Gets the description or additional info value. Usually that is
        /// a longer description about the authorization entity.
        /// </summary>
        /// <value>The description field.</value>
        /// <remarks>
        /// Usually this value goes to the tooltip to the control.
        /// </remarks>
        public virtual string AuthorizationDescription
        {
            get
            {
                return _authorizationDescription;
            }
            set
            {
                _authorizationDescription = value;
            }
        }

        /// <summary>
        /// Gets any additional value connected to the authorization
        /// </summary>
        /// <value>The value field.</value>
        /// <remarks>
        /// Usually this value holds some numeric data.
        /// </remarks>
        public virtual string AuthorizationValue
        {
            get
            {
                return _authorizationValue;
            }
            set
            {
                _authorizationValue = value;
            }
        }

        #endregion
    }
}
