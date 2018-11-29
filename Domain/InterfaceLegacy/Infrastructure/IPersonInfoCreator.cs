using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface IPersonInfoCreator
	{
		Guid CreateAndPersistPersonInfo(IPersonInfoModel personInfo);
		void RollbackPersistedTenantUsers(Guid personId);
	}
}