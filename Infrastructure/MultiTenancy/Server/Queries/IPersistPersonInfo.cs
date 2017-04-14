using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface IPersistPersonInfo
	{
		void Persist(PersonInfo personInfo);
		void RollBackPersonInfo(Guid personInfoId, string tenantName);
	}
}