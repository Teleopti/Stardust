using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class SchedulePersonProvider : ISchedulePersonProvider
	{
		private readonly IPersonRepository _personRepository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly ITeamRepository _teamRepository;
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

		public SchedulePersonProvider(IPersonRepository personRepository, IPermissionProvider permissionProvider, ITeamRepository teamRepository, IGroupingReadOnlyRepository groupingReadOnlyRepository)
		{
			_personRepository = personRepository;
			_permissionProvider = permissionProvider;
			_teamRepository = teamRepository;
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
		}

		public IEnumerable<IPerson> GetPermittedPersonsForTeam(DateOnly date, Guid id, string function)
		{
			var team = _teamRepository.Load(id);
			var period = new DateOnlyPeriod(date, date);
			var persons = _personRepository.FindPeopleBelongTeam(team, period) ?? new IPerson[] { };
			return (from p in persons
					where _permissionProvider.HasPersonPermission(function, date, p)
					select p).ToArray();
		}

		public IEnumerable<IPerson> GetPermittedPersonsForGroup(DateOnly date, Guid id, string function)
		{
			var details = _groupingReadOnlyRepository.DetailsForGroup(id, date);
			var availableDetails = details.Where(
				p => _permissionProvider.HasOrganisationDetailPermission(
					function, date, p));
			return _personRepository.FindPeople(availableDetails.Select(d => d.PersonId));
		}
	}
}