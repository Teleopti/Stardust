using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationData
{

    /// <summary>
    /// Info object to the current authorization settings.
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 11/28/2007
    /// </remarks>
    internal static class DefaultAuthorizationSettings
    {

        #region Variables

        private const bool _useDatabaseDefinedRoles = true;
        private const bool _useActiveDirectoryDefinedRoles = false;
        private const PermissionOption _permissionType = PermissionOption.Generous;

        #endregion

        #region Interface

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
        internal static bool UseDatabaseDefinedRoles
        {
            get { return _useDatabaseDefinedRoles; }
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
        internal static bool UseActiveDirectoryDefinedRoles
        {
            get { return _useActiveDirectoryDefinedRoles; }
        }

        /// <summary>
        /// Gets or sets the type of the permission.
        /// </summary>
        /// <value>The type of the permission.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/28/2007
        /// </remarks>
        internal static PermissionOption PermissionType
        {
            get { return _permissionType; }
        }

        #endregion
    }
}
