using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface IPersistPersonInfo
	{
		string Persist(PersonInfo personInfo, bool throwOnError = true);
		void RollBackPersonInfo(Guid personInfoId, string tenantName);
		string PersistIdentity(PersonInfo personInfo, bool throwOnError = true);
		string PersistApplicationLogonName(PersonInfo personInfo, bool throwOnError = true);
	}
}