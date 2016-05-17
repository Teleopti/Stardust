using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	/// <summary>
	/// 
	/// </summary>
	public class BadgeCalculationInitEvent :  EventWithInfrastructureContext
	{
		private readonly Guid _messageId = Guid.NewGuid();

		///<summary>
		/// Definies an identity for this message (typically the Id of the root this message refers to.
		///</summary>
		public Guid Identity
		{
			get { return _messageId; }
		}
	}
}
