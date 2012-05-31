using System;

namespace Teleopti.Interfaces.Messages.Denormalize
{
	/// <summary>
	/// Denormalize the grouping message.
	/// </summary>
	[Serializable]
	public class GroupPageChangedMessage : RaptorDomainMessage
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
