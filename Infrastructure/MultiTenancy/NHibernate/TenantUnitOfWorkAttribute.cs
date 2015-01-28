using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate
{
	public class TenantUnitOfWorkAttribute : ResolvedAspectAttribute
	{
		public TenantUnitOfWorkAttribute() : base(typeof(TenantUnitOfWorkAspect))
		{
		}
	}
}