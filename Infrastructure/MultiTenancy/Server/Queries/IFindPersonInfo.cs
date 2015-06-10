using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface IFindPersonInfo
	{
		PersonInfo GetById(Guid id);
	}
}