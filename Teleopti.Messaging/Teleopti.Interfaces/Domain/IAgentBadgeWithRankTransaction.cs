using System;

namespace Teleopti.Interfaces.Domain
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
		BadgeType BadgeType { get; set; }

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
	}
}