using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	public class TeamSchedulePersonsProvider : ITeamSchedulePersonsProvider
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPersonForScheduleFinder _personForScheduleFinder;
		private readonly IPersonRepository _personRepository;
        private readonly ISettingsPersisterAndProvider<NameFormatSettings> _nameFormatSettings;

        public TeamSchedulePersonsProvider(IPermissionProvider permissionProvider,
			IPersonForScheduleFinder personForScheduleFinder, IPersonRepository personRepository, ISettingsPersisterAndProvider<NameFormatSettings> nameFormatSettings)
		{
			_permissionProvider = permissionProvider;
			_personForScheduleFinder = personForScheduleFinder;
			_personRepository = personRepository;
            _nameFormatSettings = nameFormatSettings;
		}

		public IEnumerable<Guid> RetrievePersonIds(TeamScheduleViewModelData data)
		{
		    var nameFormatSetting = _nameFormatSettings.Get().ToNameFormatSetting();

			// The following function name should be modified to be more reuse-friendly ......
			var fetchedPersonList = _personForScheduleFinder.GetPersonFor(data.ScheduleDate, data.TeamIdList,
				data.SearchNameText, nameFormatSetting);

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