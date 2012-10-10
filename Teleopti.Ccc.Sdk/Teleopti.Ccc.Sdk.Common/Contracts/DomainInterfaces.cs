//keep this namespace for not breaking compatibility
namespace Teleopti.Ccc.Sdk.Common.Contracts
{
	///<summary>
	/// Detail level for job results
	///</summary>
	public enum DetailLevel
	{
		///<summary>
		/// Information
		///</summary>
		Info,

		///<summary>
		/// Warning
		///</summary>
		Warning,

		///<summary>
		/// Error
		///</summary>
		Error
	}

	/// <summary>
	/// Types of employments
	/// </summary>
	public enum EmploymentType
	{
		/// <summary>
		/// Employed as fixed staff with normal work time
		/// </summary>
		FixedStaffNormalWorkTime = 0,
		/// <summary>
		/// Employed at hourly basis
		/// </summary>
		HourlyStaff = 1,
		/// <summary>
		/// Employed as fixed staff with normal work time
		/// </summary>
		FixedStaffDayWorkTime = 3
	}

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