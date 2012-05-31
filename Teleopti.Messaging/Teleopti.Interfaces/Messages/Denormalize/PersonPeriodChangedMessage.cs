
using System;

namespace Teleopti.Interfaces.Messages.Denormalize
{
	/// <summary>
	/// Denormalize the person period message.
	/// </summary>
	[Serializable]
	public class PersonPeriodChangedMessage : RaptorDomainMessage
	{
		private readonly Guid _messageId = Guid.NewGuid();

		/// <summary>
		/// Gets the message identity.
		/// </summary>
		public override Guid Identity
		{
			get { return _messageId; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Guid[] Ids { get; set; }

	}
}
