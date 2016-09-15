using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AdherenceInfo
	{
		private readonly Context _context;
		private readonly MappingsState _mappingsState;
		private readonly Lazy<IEnumerable<AdherenceChange>> _adherenceChanges;

		public AdherenceInfo(Context context, MappingsState mappingsState)
		{
			_context = context;
			_mappingsState = mappingsState;
			_adherenceChanges = new Lazy<IEnumerable<AdherenceChange>>(buildAdherenceChanges);
		}
		
		public EventAdherence AdherenceForNewStateAndCurrentActivity()
		{
			return _context.AppliedAdherence.ForEvent(_context.State.Adherence(), _context.State.StaffingEffect());
		}

		public EventAdherence AdherenceForStoredState()
		{
			return adherenceFor(_context.Stored?.StateCode, _context.Stored.PlatformTypeId(), _context.Stored?.ActivityId);
		}

		public EventAdherence AdherenceForNewStateAndPreviousActivity()
		{
			return adherenceFor(_context.Input.StateCode, _context.Input.ParsedPlatformTypeId(), _context.Schedule.PreviousActivity());
		}

		public EventAdherence AdherenceForNoStateAndCurrentActivity()
		{
			return adherenceFor(null, Guid.Empty, _context.Schedule.CurrentActivity());
		}

		public EventAdherence AdherenceForStoredStateAndCurrentActivity()
		{
			return adherenceFor(_context.Stored?.StateCode, _context.Stored.PlatformTypeId(), _context.Schedule.CurrentActivity());
		}

		private EventAdherence adherenceFor(string stateCode, Guid platformTypeId, ScheduledActivity activity)
		{
			var activityId = (Guid?)null;
			if (activity != null)
				activityId = activity.PayloadId;
			return adherenceFor(stateCode, platformTypeId, activityId);
		}

		private EventAdherence adherenceFor(string stateCode, Guid platformTypeId, Guid? activityId)
		{
			var rule = _context.StateMapper.RuleFor(_mappingsState, _context.BusinessUnitId, platformTypeId, stateCode, activityId);
			if (rule == null)
				return _context.AppliedAdherence.ForEvent(null, null);
			return _context.AppliedAdherence.ForEvent(rule.Adherence, rule.StaffingEffect);
		}

		public EventAdherence? Adherence => _adherenceChanges.Value.LastOrDefault()?.Adherence ?? _context.Stored.Adherence;

		public IEnumerable<AdherenceChange> AdherenceChanges()
		{
			return _adherenceChanges.Value;
		}

		private IEnumerable<AdherenceChange> buildAdherenceChanges()
		{
			var adherence = _context.Stored?.Adherence;

			var adherenceChanges = new List<AdherenceChange>();
			if (_context.Schedule.ActivityChanged())
			{
				if (adherence != AdherenceForStoredStateAndCurrentActivity())
				{
					adherence = AdherenceForStoredStateAndCurrentActivity();
					DateTime timeOfAdherenceChange;
					var activityStartTime = _context.Schedule.ActivityStartTime();
					if (activityStartTime != null)
						timeOfAdherenceChange = activityStartTime.Value;
					else if (_context.Schedule.PreviousActivity() != null)
						timeOfAdherenceChange = _context.Schedule.PreviousActivity().EndDateTime;
					else
						timeOfAdherenceChange = _context.CurrentTime;

					adherenceChanges.Add(new AdherenceChange
					{
						Adherence = adherence.Value,
						Time = timeOfAdherenceChange
					});
				}
			}

			if (_context.State.StateChanged() || _context.Stored == null)
			{
				if (adherence != AdherenceForNewStateAndCurrentActivity())
				{
					adherence = AdherenceForNewStateAndCurrentActivity();
					adherenceChanges.Add(new AdherenceChange
					{
						Adherence = adherence.Value,
						Time = _context.CurrentTime
					});
				}
			}
			
			return adherenceChanges
				.GroupBy(change => change.Time, (_, changes) => changes.Last())
				.GroupBy(change => change.Adherence, (_, changes) => changes.First())
				.ToArray();
		}

		public class AdherenceChange
		{
			public DateTime Time;
			public EventAdherence Adherence;
		}
		
	}

}