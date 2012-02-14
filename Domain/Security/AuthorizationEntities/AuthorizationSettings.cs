using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{

    public class AuthorizationSettings : AggregateRootWithBusinessUnit, IAuthorizationSettings, IDeleteTag
    {

        #region Variables

        private bool _useDatabaseDefinedRoles;
        private bool _useActiveDirectoryDefinedRoles;
        private PermissionOption _permissionType;

        #endregion

        #region Interface

        #region Singleton for default and current

        private static AuthorizationSettings _default;

        /// <summary>
        /// Creates a object with default authorization settings.
        /// </summary>
        /// <returns></returns>
        public static AuthorizationSettings Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new AuthorizationSettings();
                    _default.UseActiveDirectoryDefinedRoles = DefaultAuthorizationSettings.UseActiveDirectoryDefinedRoles;
                    _default.UseDatabaseDefinedRoles = DefaultAuthorizationSettings.UseDatabaseDefinedRoles;
                    _default.PermissionType = DefaultAuthorizationSettings.PermissionType;
                }
                return _default;
            }
        }

        private static AuthorizationSettings _current;
        private bool _isDeleted;

        /// <summary>
        /// Gets the singleton current instance.
        /// </summary>
        /// <value>The instance.</value>
        public static AuthorizationSettings Current
        {
            get
            {
                if (_current == null)
                    _current = Default;
                return _current;
            }
            set { _current = value; }
        }

        #endregion

        /// <summary>
        /// Gets or sets a value indicating whether to use database defined roles.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if use database defined roles; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2007-11-01
        /// </remarks>
        public bool UseDatabaseDefinedRoles
        {
            get { return _useDatabaseDefinedRoles; }
            set { _useDatabaseDefinedRoles = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use active directory roles.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if use active directory roles; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2007-11-01
        /// </remarks>
        public bool UseActiveDirectoryDefinedRoles
        {
            get { return _useActiveDirectoryDefinedRoles; }
            set { _useActiveDirectoryDefinedRoles = value; }
        }

        /// <summary>
        /// Gets or sets the type of the permission.
        /// </summary>
        /// <value>The type of the permission.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/28/2007
        /// </remarks>
        public PermissionOption PermissionType
        {
            get { return _permissionType; }
            set { _permissionType = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the permission type is rigid.
        /// </summary>
        /// <value><c>true</c> if the permission type is rigid; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/28/2007
        /// </remarks>
        public bool PermissionTypeRigid
        {
            get { return (PermissionType == PermissionOption.Rigid); }
            set
            {
                if (value)
                    PermissionType = PermissionOption.Rigid;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the permission type is generous.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the permission type is generous; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/28/2007
        /// </remarks>
        public bool PermissionTypeGenerous
        {
            get { return (PermissionType == PermissionOption.Generous); }
            set
            {
                if (value)
                    PermissionType = PermissionOption.Generous;
            }
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }

        #endregion

    }
}
