using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
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

			if (adherence == InterfaceLegacy.Domain.Adherence.In)
				return EventAdherence.In;
			if (adherence == InterfaceLegacy.Domain.Adherence.Out)
				return EventAdherence.Out;
			if (adherence == InterfaceLegacy.Domain.Adherence.Neutral)
				return EventAdherence.Neutral;

			return EventAdherence.Neutral;
		}

		public EventAdherence Adherence() => _adherence.Value;
		
	}

}