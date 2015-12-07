using System;
using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.Messaging;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest
{
	public class FakeSchedulePersonProvider : ISchedulePersonProvider
	{
		private readonly IList<IPerson> _persons;
		private readonly IList<IPerson> _personsWithMyTeamSchedulesPermission;
		private readonly IList<IPerson> _personsWithViewConfidentialPermission;

		public FakeSchedulePersonProvider(IEnumerable<IPerson> persons)
		{
			_persons = persons.ToList();
			_personsWithMyTeamSchedulesPermission = new List<IPerson>();
			_personsWithMyTeamSchedulesPermission.AddRange(persons);
			_personsWithViewConfidentialPermission = new List<IPerson>();
		}

		public IEnumerable<IPerson> GetPermittedPersonsForTeam(DateOnly date, Guid id, string function)
		{
			return _persons;
		}

		public IEnumerable<IPerson> GetPermittedPersonsForGroup(DateOnly date, Guid id, string function)
		{
			if (function == DefinedRaptorApplicationFunctionPaths.MyTeamSchedules)
			{
				return _personsWithMyTeamSchedulesPermission;
			}
			if (function == DefinedRaptorApplicationFunctionPaths.ViewConfidential)
			{
				return _personsWithViewConfidentialPermission;
			}
			return _persons;
		}

		public IEnumerable<IPerson> GetPermittedPeople(GroupScheduleInput input, string function)
		{
			throw new NotImplementedException();
		}

		public void AddPersonWithMyTeamSchedulesPermission(IPerson person)
		{
			_personsWithMyTeamSchedulesPermission.Add(person);
		}
		public void AddPersonWitViewConfidentialPermission(IPerson person)
		{
			_personsWithViewConfidentialPermission.Add(person);
		}
	}
}