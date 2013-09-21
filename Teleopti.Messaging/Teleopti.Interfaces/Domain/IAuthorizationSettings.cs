namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Info object to the current authorization settings.
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 11/28/2007
    /// </remarks>
    public interface IAuthorizationSettings : IAggregateRoot
    {
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
        bool UseDatabaseDefinedRoles { get; set; }

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
        bool UseActiveDirectoryDefinedRoles { get; set; }

        /// <summary>
        /// Gets or sets the type of the permission.
        /// </summary>
        /// <value>The type of the permission.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/28/2007
        /// </remarks>
        PermissionOption PermissionType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the permission type is rigid.
        /// </summary>
        /// <value><c>true</c> if the permission type is rigid; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/28/2007
        /// </remarks>
        bool PermissionTypeRigid { get; set; }

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
        bool PermissionTypeGenerous { get; set; }
    }
}
