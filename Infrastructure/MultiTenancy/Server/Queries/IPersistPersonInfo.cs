using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface IPersistPersonInfo
	{
		//string Persist(PersonInfo personInfo, bool throwOnError = true);
		string Persist(GenericPersistApiCallActionObj genericPersistAuditAction);
		void RollBackPersonInfo(Guid personInfoId, string tenantName);
		string PersistIdentity(IdentityChangeActionObj identityChangeActionObj);
		string PersistApplicationLogonName(AppLogonChangeActionObj appLogonChangeActionObj);
	}
}