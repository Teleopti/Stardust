using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class ExtendedPreferencePredicate : IExtendedPreferencePredicate
	{
		public bool IsExtended(IPreferenceDay preferenceDay) { return false; }
	}
}