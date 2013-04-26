using System;

namespace Teleopti.Interfaces.Messages
{
	/// <summary>
	/// Info interface data
	/// </summary>
	public interface IRaptorDomainMessageInfo
	{
		/// <summary>
		/// A time stamped thing
		/// </summary>
		DateTime Timestamp { get; set; }
		/// <summary>
		/// Sourced data
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Datasource")]
		string Datasource { get; set; }
		/// <summary>
		/// United business
		/// </summary>
		Guid BusinessUnitId { get; set; }
	}
}