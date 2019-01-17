using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Aspects
{
	public class TenantAuditAspect : IAspect
	{
		private readonly ITenantAuditPersister _tenantAuditPersister;
		private readonly ICurrentHttpContext _currentHttpContext;

		public TenantAuditAspect(ITenantAuditPersister tenantAuditPersister, ICurrentHttpContext currentHttpContext)
		{
			_tenantAuditPersister = tenantAuditPersister;
			_currentHttpContext = currentHttpContext;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			object intent = null;
			var tenantAttr = invocation.Method.GetCustomAttributes(typeof(TenantAuditAttribute), false).FirstOrDefault();

			if (tenantAttr != null)
			{
				intent = ((TenantAuditAttribute)tenantAttr).ActionIntent;
			}

			var action = invocation.Arguments.OfType<TenantAuditAction>().SingleOrDefault();
			var personInfo = action.PersonInfo;
			if (personInfo != null && intent != null)
			{
				AuditPersist(personInfo, (PersistActionIntent)intent);
			}
		}

		private void AuditPersist(PersonInfo personInfo, PersistActionIntent intent)
		{
			var tenantUser = _currentHttpContext.Current().Items[WebTenantAuthenticationConfiguration.PersonInfoKey] as PersonInfo;

			var data = Newtonsoft.Json.JsonConvert.SerializeObject(new
			{
				Identity = personInfo.Identity ?? string.Empty,
				AppLogonName = personInfo.ApplicationLogonInfo.LogonName ?? string.Empty,
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