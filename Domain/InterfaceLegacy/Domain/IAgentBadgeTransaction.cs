using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAgentBadgeTransaction : IAggregateRoot
	{
		/// <summary>
		/// Badge owner
		/// </summary>
		IPerson Person { get; set; }

		/// <summary>
		/// Badge type
		/// </summary>
		int BadgeType { get; set; }

		/// <summary>
		/// Amount in this transaction
		/// </summary>
		int Amount { get; set; }

		/// <summary>
		/// Badge calcualted date
		/// </summary>
		DateOnly CalculatedDate { get; set; }

		/// <summary>
		/// Description
		/// </summary>
		string Description { get; set; }

		/// <summary>
		/// Transaction created
		/// </summary>
		DateTime InsertedOn { get; set; }

		/// <summary>
		/// Whether the performance type is imported from external
		/// </summary>
		bool IsExternal { get; set; }
	}
}