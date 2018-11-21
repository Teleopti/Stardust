using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Audit
{
	[EnabledBy(Toggles.Wfm_AuditTrail_GenericAuditTrail_74938)]
	public class PurgeAuditHandler : IHandleEvent<TenantDayTickEvent>, IRunOnHangfire
	{
		private readonly PurgeAuditRunner _purgeAuditRunner;

		public PurgeAuditHandler(PurgeAuditRunner purgeAuditRunner)
		{
			_purgeAuditRunner = purgeAuditRunner;
		}
		
		[UnitOfWork]
		public virtual void Handle(TenantDayTickEvent @event)
		{
			_purgeAuditRunner.Run();
		}
	}
}
