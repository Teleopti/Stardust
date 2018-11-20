using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

		public FakeSchedulePersonProvider(IEnumerable<IPerson> persons)
		{
			_persons = persons.ToList();
			_personsWithMyTeamSchedulesPermission = persons.ToList();
		}

		public IEnumerable<IPerson> GetPermittedPersonsForGroup(DateOnly date, Guid id, string function)
		{
			if (function == DefinedRaptorApplicationFunctionPaths.MyTeamSchedules)
			{
				return _personsWithMyTeamSchedulesPermission;
			}
			if (function == DefinedRaptorApplicationFunctionPaths.ViewConfidential)
			{
				return new IPerson[0];
			}
			return _persons;
		}

		public IEnumerable<IPerson> GetPermittedPeople(GroupScheduleInput input, string function)
		{
			throw new NotImplementedException();
		}
	}
}