using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Messages.Denormalize
{
	/// <summary>
	/// Denormalize the Person finder.
	/// </summary>
	[Serializable]
	public class PersonChangedMessage : RaptorDomainMessage
	{
		private readonly Guid _messageId = Guid.NewGuid();
		private string _serializedPeople;

		[NonSerialized]
		private ICollection<Guid> _personIdCollection;

		/// <summary>
		/// Gets the message identity.
		/// </summary>
		public override Guid Identity
		{
			get { return _messageId; }
		}

		/// <summary>
		/// The collection of id for person.
		/// </summary>
		public ICollection<Guid> PersonIdCollection
		{
			get
			{
				if (_personIdCollection==null)
				{
					_personIdCollection = new Collection<Guid>();
					if (!string.IsNullOrEmpty(_serializedPeople))
					{
						var items = _serializedPeople.Split(',');
						foreach (var item in items)
						{
							_personIdCollection.Add(new Guid(item));
						}
					}
				}
				return _personIdCollection;
			}
		}

		public void SetPersonIdCollection(ICollection<Guid> personIdCollection)
		{
			var stringCollection = new string[personIdCollection.Count];
			var index = 0;
			foreach (var guid in personIdCollection)
			{
				stringCollection[index] = guid.ToString();
				index++;
			}
			_serializedPeople = string.Join(",", stringCollection);
		}
	}
}

