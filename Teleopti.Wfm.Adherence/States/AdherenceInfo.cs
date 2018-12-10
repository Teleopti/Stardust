using System;
using Teleopti.Wfm.Adherence.States.Events;

namespace Teleopti.Wfm.Adherence.States
{
	public class AdherenceInfo
	{
		private readonly Context _context;
		private readonly Lazy<EventAdherence> _adherence;

		public AdherenceInfo(Context context)
		{
			_context = context;
			_adherence = new Lazy<EventAdherence>(() => adherenceFor(_context.State.StateGroupId(), _context.Schedule.CurrentActivity()));
		}
		
		private EventAdherence adherenceFor(Guid? stateGroupId, ScheduledActivity activity)
		{
			var activityId = (Guid?)null;
			if (activity != null)
				activityId = activity.PayloadId;
			return adherenceFor(stateGroupId, activityId);
		}

		private EventAdherence adherenceFor(Guid? stateGroupId, Guid? activityId)
		{
			var rule = _context.StateMapper.RuleFor(_context.BusinessUnitId, stateGroupId, activityId);
			return adherenceFor(rule?.Adherence, rule?.StaffingEffect);
		}

		private static EventAdherence adherenceFor(Configuration.Adherence? adherence, double? staffingEffect)
		{
			if (!adherence.HasValue)
			{
				if (!staffingEffect.HasValue)
					return EventAdherence.Neutral;
				if (staffingEffect.Value.Equals(0))
					return EventAdherence.In;
				return EventAdherence.Out;
			}

			if (adherence == Configuration.Adherence.In)
				return EventAdherence.In;
			if (adherence == Configuration.Adherence.Out)
				return EventAdherence.Out;
			if (adherence == Configuration.Adherence.Neutral)
				return EventAdherence.Neutral;

			return EventAdherence.Neutral;
		}

		public EventAdherence Adherence() => _adherence.Value;
		
	}

}