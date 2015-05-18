using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IPersonForScheduleFinder
	{
		IList<IAuthorizeOrganisationDetail> GetPersonFor(DateOnly shiftTradeDate, IList<Guid> teamIdList,
			string name);
	}
}
