using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonCollectionChangedEvent : PersonCollectionChangedEventBase
	{
	}

	public class AnalyticsPersonCollectionChangedEvent : PersonCollectionChangedEventBase
	{
	}

	public class AnalyticsPersonPeriodRangeChangedEvent : PersonCollectionChangedEventBase
	{
	}

	public abstract class PersonCollectionChangedEventBase : EventWithInfrastructureContext
	{
		private string _serializedPeople;

		[NonSerialized]
		private ICollection<Guid> _personIdCollection;

		public ICollection<Guid> PersonIdCollection
		{
			get
			{
				if (_personIdCollection != null) return _personIdCollection;
				_personIdCollection = string.IsNullOrEmpty(_serializedPeople)
					? new List<Guid>()
					: _serializedPeople.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).Distinct().ToList();
				return _personIdCollection;
			}
			set => _personIdCollection = value;
		}

		public void SetPersonIdCollection(ICollection<Guid> personIdCollection)
		{
            if (personIdCollection != null)
			{
                _serializedPeople = string.Join(",", new HashSet<Guid>(personIdCollection).Select(p => p.ToString()));
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
