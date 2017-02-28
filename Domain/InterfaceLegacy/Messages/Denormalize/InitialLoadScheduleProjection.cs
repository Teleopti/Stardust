using System;

namespace Teleopti.Interfaces.Messages.Denormalize
{
	/// <summary>
	/// Initial load of schedule projections
	/// </summary>
	public class InitialLoadScheduleProjection : MessageWithLogOnContext
	{
		private readonly Guid _messageId = Guid.NewGuid();

		/// <summary>
		/// Gets the message identity.
		/// </summary>
		public override Guid Identity
		{
			get { return _messageId; }
		}

		public int StartDays { get; set; }

		public int EndDays { get; set; }
	}
}