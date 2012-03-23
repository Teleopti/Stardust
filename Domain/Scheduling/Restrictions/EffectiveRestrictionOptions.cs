using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class EffectiveRestrictionOptions : IEffectiveRestrictionOptions
	{
		public bool UsePreference { get; set; }

		public EffectiveRestrictionOptions(bool usePreference)
		{
			UsePreference = usePreference;
		}
	}
}