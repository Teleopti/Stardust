using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class SchedulePersonProvider : ISchedulePersonProvider
	{
		private readonly IPersonRepository _personRepository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

		public SchedulePersonProvider(IPersonRepository personRepository, IPermissionProvider permissionProvider, IGroupingReadOnlyRepository groupingReadOnlyRepository)
		{
			_personRepository = personRepository;
			_permissionProvider = permissionProvider;
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
		}

		public IEnumerable<IPerson> GetPermittedPersonsForGroup(DateOnly date, Guid id, string function)
		{
			var details = _groupingReadOnlyRepository.DetailsForGroup(id, date);
			var availableDetails = details.Where(
				p => _permissionProvider.HasOrganisationDetailPermission(
					function, date, p));
			return _personRepository.FindPeople(availableDetails.Select(d => d.PersonId));
		}

		public IEnumerable<IPerson> GetPermittedPeople(GroupScheduleInput input, string function)
		{
			var people = _personRepository.FindPeople(input.PersonIds);
			return (from person in people
				where _permissionProvider.HasPersonPermission(function, new DateOnly(input.ScheduleDate), person)
				select person).ToArray();
		}
	}
}