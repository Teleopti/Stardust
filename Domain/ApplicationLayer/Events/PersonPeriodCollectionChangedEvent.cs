using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonPeriodCollectionChangedEvent : EventWithInfrastructureContext
	{
		private string _serializedPersonPeriod;

		[NonSerialized]
		private ICollection<Guid> _personIdCollection;

		public ICollection<Guid> PersonIdCollection
		{
			get
			{
				if (_personIdCollection == null)
				{
					_personIdCollection = new List<Guid>();
					if (!string.IsNullOrEmpty(_serializedPersonPeriod))
					{
						var items = _serializedPersonPeriod.Split(',');
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
			if (personIdCollection != null)
			{
				var stringCollection = new string[personIdCollection.Count];
				var index = 0;
				foreach (var guid in personIdCollection)
				{
					stringCollection[index] = guid.ToString();
					index++;
				}
				_serializedPersonPeriod = string.Join(",", stringCollection);
			}

		}

		public string SerializedPersonPeriod
		{
			get
			{
				return _serializedPersonPeriod;
			}
			set
			{
				_serializedPersonPeriod = value;
			}
		}
	}
}