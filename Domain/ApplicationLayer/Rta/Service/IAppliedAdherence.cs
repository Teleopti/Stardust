
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAppliedAdherence
	{
		Adherence ForAlarm(IRtaRule _rtaRule);
		EventAdherence ForEvent(Adherence? adherence, double? staffingEffect);
	}

	public class ByStaffingEffect : IAppliedAdherence
	{
		public Adherence ForAlarm(IRtaRule _rtaRule)
		{
			if (_rtaRule == null)
				return Adherence.In;
			return forStaffingEffect(_rtaRule.StaffingEffect);
		}

		public EventAdherence ForEvent(Adherence? adherence, double? staffingEffect)
		{
			return forStaffingEffect(staffingEffect) == Adherence.Out 
				? EventAdherence.Out
				: EventAdherence.In;
		}

		private static Adherence forStaffingEffect(double? staffingEffect)
		{
			if (!staffingEffect.HasValue)
				return Adherence.In;
			return staffingEffect.Value.Equals(0)
				? Adherence.In
				: Adherence.Out;
		}
	}

	public class BySetting : IAppliedAdherence
	{
		public Adherence ForAlarm(IRtaRule _rtaRule)
		{
			if (_rtaRule == null)
				return Adherence.Neutral;
			return _rtaRule.Adherence.HasValue
				? _rtaRule.Adherence.Value
				: forStaffingEffect(_rtaRule.StaffingEffect);
		}

		public EventAdherence ForEvent(Adherence? adherence, double? staffingEffect)
		{
			if (!adherence.HasValue)
				adherence = forStaffingEffect(staffingEffect);

			if (adherence == Adherence.In)
				return EventAdherence.In;
			if (adherence == Adherence.Out)
				return EventAdherence.Out;

			return EventAdherence.Neutral;
		}

		private static Adherence forStaffingEffect(double? staffingEffect)
		{
			if (!staffingEffect.HasValue)
				return Adherence.Neutral;
			return staffingEffect.Value.Equals(0)
				? Adherence.In
				: Adherence.Out;
		}
	}
}