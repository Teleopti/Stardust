using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ArchiveScheduleEvent : EventWithInfrastructureContext
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public Guid FromScenario { get; set; }
		public Guid ToScenario { get; set; }
		public Guid TrackingId { get; set; }

		//public Guid PersonId { get; set; }
		private string _serializedPeople;

		[NonSerialized]
		private ICollection<Guid> _personIdCollection;

		public ICollection<Guid> PersonIdCollection
		{
			get
			{
				if (_personIdCollection != null) return _personIdCollection;
				_personIdCollection = string.IsNullOrEmpty(_serializedPeople)
					? new HashSet<Guid>()
					: new HashSet<Guid>(_serializedPeople.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse));
				return _personIdCollection;
			}
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