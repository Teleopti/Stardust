using System;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class IntradayOptimizationContext
	{
		private readonly VirtualSkillContext _virtualSkillContext;
		private readonly IResourceCalculationContextFactory _resourceCalculationContext;

		public IntradayOptimizationContext(VirtualSkillContext virtualSkillContext, IResourceCalculationContextFactory resourceCalculationContext)
		{
			_virtualSkillContext = virtualSkillContext;
			_resourceCalculationContext = resourceCalculationContext;
		}

		public IDisposable Create(DateOnlyPeriod period)
		{
			var virtualSkillContext = _virtualSkillContext.Create(period);
			var resourceContext = _resourceCalculationContext.Create();
			ResourceCalculationContext.Fetch().PrimarySkillMode = true;
			return new GenericDisposable(() =>
			{
				resourceContext.Dispose();
				virtualSkillContext.Dispose();
			});
		}
	}
}