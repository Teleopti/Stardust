using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface IPersistPersonInfo
	{
		string Persist(PersonInfo personInfo, PersistActionIntent actionIntent = PersistActionIntent.NotSet, bool throwOnError = true);
		void RollBackPersonInfo(Guid personInfoId, string tenantName);
	}
}