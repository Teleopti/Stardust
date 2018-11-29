using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface ITenantPersonLogonQuerier
	{
		IEnumerable<IPersonInfoModel> FindApplicationLogonUsers(IEnumerable<string> logonNames);
	}
}
