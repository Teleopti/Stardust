using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface IDeletePersonInfo
	{
		void Delete(Guid personId);
	}
}