using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate
{
	public class TenantUnitOfWorkAttribute : AspectAttribute
	{
		public TenantUnitOfWorkAttribute() : base(typeof(ITenantUnitOfWorkAspect))
		{
		}
	}
}