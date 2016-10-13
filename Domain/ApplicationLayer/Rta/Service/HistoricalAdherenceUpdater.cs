using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	[EnabledBy(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
	public class HistoricalAdherenceUpdater : 
		IRunOnHangfire,
		IHandleEvent<PersonOutOfAdherenceEvent>,
		IHandleEvent<PersonInAdherenceEvent>,
		IHandleEvent<PersonNeutralAdherenceEvent>
	{
		private readonly IHistoricalAdherenceReadModelPersister _persister;

		public HistoricalAdherenceUpdater(IHistoricalAdherenceReadModelPersister persister)
		{
			_persister = persister;
		}

		public void Handle(PersonOutOfAdherenceEvent @event)
		{
			_persister.AddOut(@event.PersonId, @event.Timestamp);
		}

		public void Handle(PersonInAdherenceEvent @event)
		{
			_persister.AddIn(@event.PersonId, @event.Timestamp);
		}

		public void Handle(PersonNeutralAdherenceEvent @event)
		{
			_persister.AddNeutral(@event.PersonId, @event.Timestamp);
		}
	}
}
