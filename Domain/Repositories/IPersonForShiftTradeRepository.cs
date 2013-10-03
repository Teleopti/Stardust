using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IPersonForShiftTradeRepository
	{
		IList<IAuthorizeOrganisationDetail> GetPersonForShiftTrade(DateOnly shiftTradeDate, Guid? teamId);
	}
}
