using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public class TenantUnitOfWorkAttribute : AspectAttribute
	{
		public TenantUnitOfWorkAttribute() : base(typeof(ITenantUnitOfWorkAspect))
		{
		}
	}
}