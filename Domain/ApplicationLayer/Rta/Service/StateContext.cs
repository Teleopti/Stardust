using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateContext
	{
		private readonly Func<StoredStateInfo> _stored;
		private readonly Func<IEnumerable<ScheduleLayer>> _schedule;
		private readonly Func<IEnumerable<Mapping>> _mappings;
		private readonly Action<StateInfo> _agentStateReadModelUpdater;

		public StateContext(ExternalUserStateInputModel input, Guid personId, Guid businessUnitId, Guid teamId, Guid siteId, Func<StoredStateInfo> stored, Func<IEnumerable<ScheduleLayer>> schedule, Func<IEnumerable<Mapping>> mappings, Action<StateInfo> agentStateReadModelUpdater, INow now)
		{
			_stored = stored;
			_schedule = schedule;
			_mappings = mappings;
			_agentStateReadModelUpdater = agentStateReadModelUpdater ?? (a => { });
			Input = input ?? new ExternalUserStateInputModel();
			CurrentTime = now.UtcDateTime();
			PersonId = personId;
			BusinessUnitId = businessUnitId;
			TeamId = teamId;
			SiteId = siteId;
		}

		public ExternalUserStateInputModel Input { get; private set; }

		public DateTime CurrentTime { get; private set; }

		public Guid PersonId { get; private set; }
		public Guid BusinessUnitId { get; private set; }
		public Guid TeamId { get; private set; }
		public Guid SiteId { get; private set; }

		public StoredStateInfo Stored() { return _stored == null ? null : _stored.Invoke(); }
		public IEnumerable<ScheduleLayer> ScheduleLayers() { return _schedule.Invoke(); }
		public IEnumerable<Mapping> Mappings() { return _mappings.Invoke(); }

		public void UpdateAgentStateReadModel(StateInfo info)
		{
			_agentStateReadModelUpdater.Invoke(info);
		}
		
		// for logging
		public override string ToString()
		{
			return string.Format(
				"PersonId: {0}, BusinessUnitId: {1}, TeamId: {2}, SiteId: {3}",
				PersonId, BusinessUnitId, TeamId, SiteId);
		}
	}
}