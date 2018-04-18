using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Aspects
{
	public class TenantAuditAspect : IAspect
	{
		private readonly ITenantAuditPersister _tenantAuditPersister;
		private readonly ICurrentTenantUser _currentTenantUser;

		public TenantAuditAspect(ITenantAuditPersister tenantAuditPersister, ICurrentTenantUser currentTenantUser)
		{
			_tenantAuditPersister = tenantAuditPersister;
			_currentTenantUser = currentTenantUser;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			var personInfo = invocation.Arguments.SingleOrDefault(x => x is PersonInfo) as PersonInfo;
			var intent = invocation.Arguments.SingleOrDefault(x => x is PersistActionIntent);
			if (personInfo != null && intent != null)
			{
				AuditPersist(personInfo, (PersistActionIntent)intent);
			}
		}

		private void AuditPersist(PersonInfo personInfo, PersistActionIntent intent)
		{
			var tenantUser = _currentTenantUser.CurrentUser();
			var data = Newtonsoft.Json.JsonConvert.SerializeObject(new
			{
				Identity = personInfo.Identity ?? string.Empty,
				AppLogonName = personInfo.ApplicationLogonInfo.LogonName,
				AppLogonPwd = string.IsNullOrEmpty(personInfo.ApplicationLogonInfo.LogonPassword) ? string.Empty : "<pwd-hash-set>"
			});

			var ta = new TenantAudit(tenantUser.Id, personInfo.Id, intent.ToString(), data);
			_tenantAuditPersister.Persist(ta);
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
		}
	}
}