using System;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.Toggle
{
	public class UpdateTogglesAspect : IUpdateTogglesAspect
	{
		private readonly IToggleFiller _toggleFiller;

		public UpdateTogglesAspect(IToggleFiller toggleFiller)
		{
			_toggleFiller = toggleFiller;
		}
		
		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			if(invocation.Proxy is IRunOnStardust)
			{
				_toggleFiller.RefetchToggles();
			}
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
		}
	}
}