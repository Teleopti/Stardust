using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class TenantAccessAuditContext : IHandleContextAction<GenericPersistApiCallActionObj>,
		IHandleContextAction<AppLogonChangeActionObj>, IHandleContextAction<IdentityChangeActionObj>
	{
		private readonly ITenantAuditPersister _tenantAuditPersister;
		private readonly ICurrentHttpContext _currentHttpContext;

		public TenantAccessAuditContext(ITenantAuditPersister tenantAuditPersister, ICurrentHttpContext currentHttpContext)
		{
			_tenantAuditPersister = tenantAuditPersister;
			_currentHttpContext = currentHttpContext;
		}

		public void Handle(GenericPersistApiCallActionObj command)
		{
			auditPersist(command.PersonInfo, PersistActionIntent.GenericPersistApiCall);
		}

		public void Handle(AppLogonChangeActionObj command)
		{
			auditPersist(command.PersonInfo, PersistActionIntent.AppLogonChange);
		}

		public void Handle(IdentityChangeActionObj command)
		{
			auditPersist(command.PersonInfo, PersistActionIntent.IdentityChange);
		}

		private void auditPersist(PersonInfo personInfo, PersistActionIntent intent)
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
	}
}