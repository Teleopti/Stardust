namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Interface for holding optional column.
	/// </summary>
	/// <remarks>
	/// Created by: Dinesh Ranasinghe
	/// Created date: 1/13/2009
	/// </remarks>
	public interface IOptionalColumn : IAggregateRoot,
													IChangeInfo
	{
		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 1/13/2009
		/// </remarks>
		string Name { get; set; }

		bool AvailableAsGroupPage { get; set; }

		/// <summary>
		/// Gets or sets the name of the table.
		/// </summary>
		/// <value>The name of the table.</value>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 1/13/2009
		/// </remarks>
		string TableName { get; set; }
	}
}
