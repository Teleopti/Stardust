using System;

namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// Authentication type enumeration. 
    ///</summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames"), Flags]
	public enum AuthenticationTypeOption
	{

		/// <summary>
		/// Authentication type is unknown or not logged on
		/// </summary>
		Unknown = 1,

		/// <summary>
		/// Authentication type is application: via username - password
		/// </summary>
		Application = 2,

		/// <summary>
		/// Authentication type is windows: via username in domain
		/// </summary>
		Windows = 4
	}
}