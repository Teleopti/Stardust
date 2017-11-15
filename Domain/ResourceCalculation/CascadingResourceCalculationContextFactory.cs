using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	[RemoveMeWithToggle("Replace old and remove 'new' in name", Toggles. ResourcePlanner_RemoveImplicitResCalcContext_46680)]
	public class CascadingResourceCalculationContextFactoryNew : CascadingResourceCalculationContextFactory
	{
		public CascadingResourceCalculationContextFactoryNew(CascadingPersonSkillProvider personSkillProvider, ITimeZoneGuard timeZoneGuard, AddBpoResourcesToContext addBpoResourcesToContext) : base(personSkillProvider, timeZoneGuard, addBpoResourcesToContext)
		{
		}

		protected override void BeforeCreatingContext()
		{
			if (ResourceCalculationContext.InContext)
			{
				throw new NestedResourceCalculationContextException();
			}
		}
	}
	
	public class CascadingResourceCalculationContextFactory : ResourceCalculationContextFactory
	{
		public CascadingResourceCalculationContextFactory(CascadingPersonSkillProvider personSkillProvider, ITimeZoneGuard timeZoneGuard, AddBpoResourcesToContext addBpoResourcesToContext)
			:base(personSkillProvider, timeZoneGuard, addBpoResourcesToContext)
		{	
		}
	}
}