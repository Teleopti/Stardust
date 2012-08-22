using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class ExtendedPreferencePredicate : IExtendedPreferencePredicate
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool IsExtended(IPreferenceDay preferenceDay)
		{
			return preferenceDay.Restriction.ActivityRestrictionCollection.Any() || 
				   preferenceDay.Restriction.EndTimeLimitation.HasValue() ||
			       preferenceDay.Restriction.StartTimeLimitation.HasValue() ||
			       preferenceDay.Restriction.WorkTimeLimitation.HasValue();
		}
	}
}