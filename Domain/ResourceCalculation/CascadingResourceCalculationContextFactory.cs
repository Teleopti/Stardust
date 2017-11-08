using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class CascadingResourceCalculationContextFactory : ResourceCalculationContextFactory
	{
		public CascadingResourceCalculationContextFactory(CascadingPersonSkillProvider personSkillProvider, ITimeZoneGuard timeZoneGuard, AddBpoResourcesToContext addBpoResourcesToContext)
			:base(personSkillProvider, timeZoneGuard, addBpoResourcesToContext)
		{	
		}
	}
}