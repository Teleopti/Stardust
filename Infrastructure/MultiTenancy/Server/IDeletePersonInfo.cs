using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface IDeletePersonInfo
	{
		void Delete(Guid personId);
	}
}