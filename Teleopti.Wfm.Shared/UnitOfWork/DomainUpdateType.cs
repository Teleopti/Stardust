namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Domain Update Type
	/// </summary>
	public enum DomainUpdateType
	{
		/// <summary>
		/// An insert has value zero.
		/// </summary>
		Insert = 0,
		/// <summary>
		/// An update has value one.
		/// </summary>
		Update = 1,
		/// <summary>
		/// An delete has value two.
		/// </summary>
		Delete = 2,
		/// <summary>
		/// If insert update and delete is not applicable, value 3.
		/// </summary>
		NotApplicable = 3
	}
}