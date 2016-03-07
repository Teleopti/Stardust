using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Infrastructure.Analytics
{
	public class AnalyticsUnitOfWorkAttribute : AspectAttribute
	{
		public AnalyticsUnitOfWorkAttribute() : base(typeof(IAnalyticsUnitOfWorkAspect))
		{
		}
	}
}