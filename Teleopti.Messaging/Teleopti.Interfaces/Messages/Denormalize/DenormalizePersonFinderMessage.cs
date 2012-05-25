using System;

namespace Teleopti.Interfaces.Messages.Denormalize
{
	/// <summary>
	/// Denormalize the Person finder.
	/// </summary>
	[Serializable]
	public class DenormalizePersonFinderMessage : RaptorDomainMessage
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
		public string Ids { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public bool IsPerson { get; set; }

	}
}

