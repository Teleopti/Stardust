using System;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class IntradayOptimizationContext
	{
		private readonly VirtualSkillContext _virtualSkillContext;
		private readonly ResourceCalculationContextFactory _resourceCalculationContext;

		public IntradayOptimizationContext(VirtualSkillContext virtualSkillContext, ResourceCalculationContextFactory resourceCalculationContext)
		{
			_virtualSkillContext = virtualSkillContext;
			_resourceCalculationContext = resourceCalculationContext;
		}

		public IDisposable Create(DateOnlyPeriod period)
		{
			var virtualSkillContext = _virtualSkillContext.Create(period);
			IDisposable resourceContext = null;
			if (!ResourceCalculationContext.InContext)
			{
				resourceContext = _resourceCalculationContext.Create();
			}
			return new GenericDisposable(() =>
			{
				if (resourceContext != null)
				{
					resourceContext.Dispose();
				}
				virtualSkillContext.Dispose();
			});
		}
	}
}