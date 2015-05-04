using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface IFindPersonInfo
	{
		PersonInfo GetById(Guid id);
	}
}