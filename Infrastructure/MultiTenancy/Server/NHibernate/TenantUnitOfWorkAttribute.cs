using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate
{
	public class TenantUnitOfWorkAttribute : ResolvedAspectAttribute
	{
		public TenantUnitOfWorkAttribute() : base(typeof(TenantUnitOfWorkAspect))
		{
		}
	}
}