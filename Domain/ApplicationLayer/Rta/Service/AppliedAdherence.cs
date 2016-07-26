
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AppliedAdherence
	{
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