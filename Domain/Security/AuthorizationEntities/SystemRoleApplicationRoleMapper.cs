using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
    /// <summary>
    /// A system role and application role mapper.
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 2008-03-17
    /// </remarks>
    public class SystemRoleApplicationRoleMapper :
        AggregateRootWithBusinessUnit,
        IAuthorizationEntity, IDeleteTag

    {
        private string _systemRoleLongName;
        private IApplicationRole _applicationRole;
        private string _systemName;
        private bool _isDeleted;

        /// <summary>
        /// Gets or sets the system role name.
        /// </summary>
        /// <value>The system role name.</value>
        public virtual string SystemRoleLongName
        {
            get { return _systemRoleLongName; }
            set { _systemRoleLongName = value; }
        }

        /// <summary>
        /// Gets or sets the application role.
        /// </summary>
        /// <value>The application role.</value>
        public virtual IApplicationRole ApplicationRole
        {
            get { return _applicationRole; }
            set { _applicationRole = value; }
        }

        /// <summary>
        /// Gets or sets the name of the system in which the SystemRole equals an ApplicationRole.
        /// </summary>
        /// <value>The name of the system.</value>
        public virtual string SystemName
        {
            get { return _systemName; }
            set { _systemName = value; }
        }

        #region IAuthorizationEntity Members

        /// <summary>
        /// Gets the authorization unique key. This must be unique and that is used for any
        /// comparisons within the authorization related stuff.
        /// </summary>
        /// <value>The authorization key.</value>
        public virtual string AuthorizationKey
        {
            get { return SystemRoleLongName; }
        }

        /// <summary>
        /// Gets the Name value. Usually this is the key.
        /// </summary>
        /// <value>The name field.</value>
        public virtual string AuthorizationName
        {
            get
            {
                return SystemRoleLongName;
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
                return ApplicationRole.Name;
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
                return string.Empty;
            }
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        #endregion

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
    }

}
