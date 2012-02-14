using System;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Audit revision
	/// </summary>
	public interface IRevision
	{
		/// <summary>
		/// Revision number
		/// </summary>
		long Id { get; }

		/// <summary>
		/// When was this revision modified?
		/// </summary>
		DateTime ModifiedAt { get; }

		/// <summary>
		/// Who persisted this revision?
		/// </summary>
		IPerson ModifiedBy { get; }
	}
}