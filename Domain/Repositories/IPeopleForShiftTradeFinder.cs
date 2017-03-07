using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IPeopleForShiftTradeFinder
	{
		IList<IPersonAuthorizationInfo> GetPeople(IPerson personFrom, DateOnly shiftTradeDate
			, IList<Guid> teamIdList, string name, NameFormatSetting nameFormat = NameFormatSetting.FirstNameThenLastName);
	}
}
