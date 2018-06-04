using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Messages.Denormalize.Legacy
{
	[Serializable]
	public class PersonChangedMessage : MessageWithLogOnContext, IEvent
	{
		private readonly Guid _messageId = Guid.NewGuid();
		private string _serializedPeople;

		[NonSerialized] private ICollection<Guid> _personIdCollection;

		public ICollection<Guid> PersonIdCollection
		{
			get
			{
				if (_personIdCollection != null) return _personIdCollection;
				_personIdCollection = string.IsNullOrEmpty(_serializedPeople)
					? new List<Guid>()
					: _serializedPeople.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList();
				return _personIdCollection;
			}
		}

		public string SerializedPeople
		{
			get { return _serializedPeople; }
			set { _serializedPeople = value; }
		}

		public override Guid Identity
		{
			get { return _messageId; }
		}
	}
}