using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class SchedulePersonProvider : ISchedulePersonProvider
	{
		private readonly IPersonRepository _personRepository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly ITeamRepository _teamRepository;

		public SchedulePersonProvider(IPersonRepository personRepository, IPermissionProvider permissionProvider, ITeamRepository teamRepository)
		{
			_personRepository = personRepository;
			_permissionProvider = permissionProvider;
			_teamRepository = teamRepository;
		}

		public IEnumerable<IPerson> GetPermittedPersonsForTeam(DateOnly date, Guid id)
		{
			var team = _teamRepository.Load(id);
			var period = new DateOnlyPeriod(date, date);
			var persons = _personRepository.FindPeopleBelongTeam(team, period) ?? new IPerson[] { };
			return (from p in persons
					where _permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, date, p)
					select p).ToArray();
		}
	}
}