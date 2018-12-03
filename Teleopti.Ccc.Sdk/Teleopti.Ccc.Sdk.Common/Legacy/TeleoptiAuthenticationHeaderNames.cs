// ReSharper disable once CheckNamespace
namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// Contants for header information when using header for authentication purposes
    ///</summary>
    public static class TeleoptiAuthenticationHeaderNames
    {
        ///<summary>
        /// Name of authentication header
        ///</summary>
        public const string TeleoptiAuthenticationHeaderName = "TeleoptiAuthenticationHeader";

        ///<summary>
        /// Key for user name information in header
        ///</summary>
        public const string UserNameKey = "UserName";

        ///<summary>
        /// Key for password information in header
        ///</summary>
        public const string PasswordKey = "Password";

        ///<summary>
        /// Key for data source information in header
        ///</summary>
        public const string DataSourceKey = "DataSource";

        ///<summary>
        /// Key for business unit id in header
        ///</summary>
        public const string BusinessUnitKey = "BusinessUnit";

        ///<summary>
        /// Namespace for information in header
        ///</summary>
        public const string TeleoptiAuthenticationHeaderNamespace = "http://schemas.ccc.teleopti.com/2011/02";

        ///<summary>
        /// Key for determining if windows identity should be used for authentication
        ///</summary>
        public const string UseWindowsIdentityKey = "UseWindowsIdentity";
    }
}