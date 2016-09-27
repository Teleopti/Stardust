using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IPeopleForShiftTradeFinder
	{
		IList<IAuthorizeOrganisationDetail> GetPeople(IPerson personFrom, DateOnly shiftTradeDate
			, IList<Guid> teamIdList, string name, NameFormatSetting nameFormat = NameFormatSetting.FirstNameThenLastName);
	}
}
