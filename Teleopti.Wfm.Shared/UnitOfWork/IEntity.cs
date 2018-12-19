using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Interface for all entities.
	/// Holding an unique guid.
	/// </summary>
	public interface IEntity : IEquatable<IEntity>
	{
		/// <summary>
		/// Gets the unique id for this entity.
		/// </summary>
		/// <value>The id.</value>
		Guid? Id { get; }

		/// <summary>
		/// Sets the id.
		/// </summary>
		/// <param name="newId">The new ID.</param>
		void SetId(Guid? newId);

		/// <summary>
		/// Clear the id. If aggregate root - also clears createby, createdon, updatedby, updatedon
		/// </summary>
		void ClearId();
	}
}