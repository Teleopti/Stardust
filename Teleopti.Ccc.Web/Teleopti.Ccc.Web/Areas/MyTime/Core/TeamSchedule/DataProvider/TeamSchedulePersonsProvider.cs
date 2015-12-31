using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	public class TeamSchedulePersonsProvider : ITeamSchedulePersonsProvider
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPersonForScheduleFinder _personForScheduleFinder;
		private IPersonRepository _personRepository;

		public TeamSchedulePersonsProvider(IPermissionProvider permissionProvider,
			IPersonForScheduleFinder personForScheduleFinder, IPersonRepository personRepository)
		{
			_permissionProvider = permissionProvider;
			_personForScheduleFinder = personForScheduleFinder;
			_personRepository = personRepository;
		}

		public IEnumerable<Guid> RetrievePersonIds(TeamScheduleViewModelData data)
		{
			// The following function name should be modified to be more reuse-friendly ......
			var fetchedPersonList = _personForScheduleFinder.GetPersonFor(data.ScheduleDate, data.TeamIdList,
				data.SearchNameText);

			var permittedPersonList = fetchedPersonList.Where(id =>
				_permissionProvider.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, data.ScheduleDate, id)
				).Select(item => item.PersonId).ToList();

			return permittedPersonList;
		}

		public IEnumerable<IPerson> RetrievePeople(TeamScheduleViewModelData data)
		{
			return _personRepository.FindPeopleSimplify(RetrievePersonIds(data));
		}
	}
}