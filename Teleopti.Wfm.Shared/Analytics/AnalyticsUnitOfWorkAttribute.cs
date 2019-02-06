using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Aop
{
	public class AnalyticsUnitOfWorkAttribute : AspectAttribute
	{
		public AnalyticsUnitOfWorkAttribute() : base(typeof(IAnalyticsUnitOfWorkAspect))
		{
		}
	}
}