using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface ITenantPersonLogonQuerier
	{
		IEnumerable<IPersonInfoModel> FindApplicationLogonUsers(IEnumerable<string> logonNames);
	}
}
