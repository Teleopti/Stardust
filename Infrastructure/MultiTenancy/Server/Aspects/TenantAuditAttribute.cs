using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Aspects
{
	public class TenantAuditAttribute : AspectAttribute
	{
		private readonly PersistActionIntent _actionIntent;
		public TenantAuditAttribute(PersistActionIntent actionIntent) : base(typeof(TenantAuditAspect))
		{
			_actionIntent = actionIntent;
		}

		public PersistActionIntent ActionIntent => _actionIntent;
	}
}