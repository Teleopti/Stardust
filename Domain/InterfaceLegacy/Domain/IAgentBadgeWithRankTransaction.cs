using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAgentBadgeWithRankTransaction : IAggregateRoot
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
		/// Bronze amount in this transaction
		/// </summary>
		int BronzeBadgeAmount { get; set; }

		/// <summary>
		/// Silver amount in this transaction
		/// </summary>
		int SilverBadgeAmount { get; set; }

		/// <summary>
		/// Bronze amount in this transaction
		/// </summary>
		int GoldBadgeAmount { get; set; }

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