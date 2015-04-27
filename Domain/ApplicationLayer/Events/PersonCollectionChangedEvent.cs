using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonCollectionChangedEvent : EventWithLogOnAndInitiator
	{
		private string _serializedPeople;

		[NonSerialized]
		private ICollection<Guid> _personIdCollection;

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
            if (personIdCollection != null)
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
        
        public string SerializedPeople 
        { 
            get
            {
                return _serializedPeople;
            } 
            set
            {
                _serializedPeople = value;
            } 
        }
	}
}

