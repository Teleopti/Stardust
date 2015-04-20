using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	public class TeamSchedulePersonsProvider : ITeamSchedulePersonsProvider
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPersonForShiftTradeRepository _personForShiftTradeRepository;

		public TeamSchedulePersonsProvider(IPermissionProvider permissionProvider,
			IPersonForShiftTradeRepository personForShiftTradeRepository)
		{
			_permissionProvider = permissionProvider;
			_personForShiftTradeRepository = personForShiftTradeRepository;
		}

		public IEnumerable<Guid> RetrievePersons(TeamScheduleViewModelData data)
		{
			// The following function name should be modified to be more reuse-friendly ......
			var fetchedPersonList = _personForShiftTradeRepository.GetPersonForShiftTrade(data.ScheduleDate, data.TeamIdList,
				data.SearchNameText);

			var permittedPersonList = fetchedPersonList.Where(id =>
				_permissionProvider.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ScheduleDate, id)
				).Select(item => item.PersonId).ToList();

			return permittedPersonList;
		}
	}
}