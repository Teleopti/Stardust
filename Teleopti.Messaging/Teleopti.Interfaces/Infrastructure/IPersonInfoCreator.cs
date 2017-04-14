using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IPersonInfoCreator
	{
		Guid CreateAndPersistPersonInfo(IPersonInfoModel personInfo);
		void RollbackPersistedTenantUsers(Guid personId);
	}
}