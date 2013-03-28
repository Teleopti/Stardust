using System;

namespace Teleopti.Interfaces.Messages
{
	/// <summary>
	/// Info interface data
	/// </summary>
	public interface IRaptorDomainMessageInfo
	{
		/// <summary>
		/// Sourced data
		/// </summary>
		string Datasource { get; set; }
		/// <summary>
		/// A time stamped thing
		/// </summary>
		DateTime Timestamp { get; set; }
		/// <summary>
		/// United business
		/// </summary>
		Guid BusinessUnitId { get; set; }
	}
}