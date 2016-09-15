using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AdherenceInfo
	{
		private readonly Context _context;
		private readonly MappingsState _mappingsState;

		public AdherenceInfo(Context context, MappingsState mappingsState)
		{
			_context = context;
			_mappingsState = mappingsState;
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




		public bool AdherenceChangedFromActivityChange()
		{
			EventAdherence? from = null;
			if (_context.Stored != null)
				from = AdherenceForStoredState();
			var to = AdherenceForStoredStateAndCurrentActivity();
			return from != to;
		}

		public bool AdherenceChangedFromStateChange()
		{
			EventAdherence? from = null;
			if (_context.Stored != null)
				from = AdherenceForStoredStateAndCurrentActivity();
			var to = AdherenceForNewStateAndCurrentActivity();
			return from != to;
		}
	}
}