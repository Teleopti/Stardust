using System;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IResourceCalculation
	{
		[RemoveMeWithToggle("Take data from context instead and remove paramuters here", Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680)]
		void ResourceCalculate(DateOnlyPeriod period, ResourceCalculationData resourceCalculationData, Func<IDisposable> getResourceCalculationContext = null);
	}
}
