using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain
{
	/// <summary>
	/// Entitity that can provide extra details about the changes made.
	/// </summary>
	public interface IProvideCustomChangeInfo
	{
		/// <summary>
		/// Gets all the custom changes made to this instance.
		/// </summary>
		/// <param name="status">The update status for the aggregate root.</param>
		/// <returns>A list of zero or more custom changes this root reports.</returns>
		IEnumerable<IRootChangeInfo> CustomChanges(DomainUpdateType status);

		/// <summary>
		/// Gets the entity before applied changes.
		/// </summary>
		/// <returns>A clone of the entity before the applied changes.</returns>
		IAggregateRoot BeforeChanges();
	}
}