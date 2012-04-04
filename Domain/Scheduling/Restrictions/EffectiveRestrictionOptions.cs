using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class EffectiveRestrictionOptions : IEffectiveRestrictionOptions
	{
		public bool UseAvailability { get; set; }
		public bool UsePreference { get; set; }

		public EffectiveRestrictionOptions(bool usePreference, bool useAvailability)
		{
			UseAvailability = useAvailability;
			UsePreference = usePreference;
		}
	}
}