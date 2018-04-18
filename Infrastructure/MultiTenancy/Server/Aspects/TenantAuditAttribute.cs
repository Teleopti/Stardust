using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Aspects
{
	public class TenantAuditAttribute : AspectAttribute
	{
		public TenantAuditAttribute() : base(typeof(TenantAuditAspect))
		{
		}
	}
}