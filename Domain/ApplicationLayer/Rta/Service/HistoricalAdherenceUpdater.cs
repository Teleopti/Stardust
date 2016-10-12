using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class HistoricalAdherenceUpdater : 
		IRunOnHangfire,
		IHandleEvent<PersonOutOfAdherenceEvent>
	{
		private readonly IHistoricalAdherenceReadModelPersister _persister;
		private readonly IHistoricalAdherenceReadModelReader _reader;

		public HistoricalAdherenceUpdater(IHistoricalAdherenceReadModelPersister persister, IHistoricalAdherenceReadModelReader reader)
		{
			_persister = persister;
			_reader = reader;
		}

		public void Handle(PersonOutOfAdherenceEvent @event)
		{
			var date = @event.BelongsToDate ?? new DateOnly(@event.Timestamp);
			_persister.AddOut(@event.PersonId, date, @event.Timestamp);
		}

		public void Handle(PersonInAdherenceEvent @event)
		{
			var date = @event.BelongsToDate ?? new DateOnly(@event.Timestamp);
			_persister.AddIn(@event.PersonId, date, @event.Timestamp);
		}

		public void Handle(PersonNeutralAdherenceEvent @event)
		{
			var date = @event.BelongsToDate ?? new DateOnly(@event.Timestamp);
			_persister.AddNeutral(@event.PersonId, date, @event.Timestamp);
		}
	}
}
