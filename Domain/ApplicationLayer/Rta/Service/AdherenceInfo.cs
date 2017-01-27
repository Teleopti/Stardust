using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AdherenceInfo
	{
		private readonly Context _context;
		private readonly Lazy<IEnumerable<AdherenceChange>> _adherenceChanges;
		private readonly Lazy<EventAdherence> _adherenceForStoredStateAndCurrentActivity;
		private readonly Lazy<EventAdherence> _adherenceForNewStateAndPreviousActivity;
		private readonly Lazy<EventAdherence> _adherenceForNewStateAndCurrentActivity;

		public AdherenceInfo(Context context)
		{
			_context = context;
			_adherenceChanges = new Lazy<IEnumerable<AdherenceChange>>(buildAdherenceChanges);
			_adherenceForStoredStateAndCurrentActivity = new Lazy<EventAdherence>(() => adherenceFor(_context.Stored?.StateCode, _context.Stored.PlatformTypeId(), _context.Schedule.CurrentActivity()));
			_adherenceForNewStateAndPreviousActivity = new Lazy<EventAdherence>(() => adherenceFor(_context.Input.StateCode, _context.Input.ParsedPlatformTypeId(), _context.Schedule.PreviousActivity()));
			_adherenceForNewStateAndCurrentActivity = new Lazy<EventAdherence>(() => adherenceFor(_context.State.Adherence(), _context.State.StaffingEffect()));
		}
		
		public EventAdherence AdherenceForNewStateAndCurrentActivity()
		{
			return _adherenceForNewStateAndCurrentActivity.Value;
		}

		public EventAdherence AdherenceForNewStateAndPreviousActivity()
		{
			return _adherenceForNewStateAndPreviousActivity.Value;
		}
		
		public EventAdherence AdherenceForStoredStateAndCurrentActivity()
		{
			return _adherenceForStoredStateAndCurrentActivity.Value;
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
			var rule = _context.StateMapper.RuleFor(_context.BusinessUnitId, platformTypeId, stateCode, activityId);
			if (rule == null)
				return adherenceFor(null, null);
			return adherenceFor(rule.Adherence, rule.StaffingEffect);
		}

		private static EventAdherence adherenceFor(Adherence? adherence, double? staffingEffect)
		{
			if (!adherence.HasValue)
			{
				if (!staffingEffect.HasValue)
					return EventAdherence.Neutral;
				if (staffingEffect.Value.Equals(0))
					return EventAdherence.In;
				return EventAdherence.Out;
			}

			if (adherence == Interfaces.Domain.Adherence.In)
				return EventAdherence.In;
			if (adherence == Interfaces.Domain.Adherence.Out)
				return EventAdherence.Out;

			return EventAdherence.Neutral;
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