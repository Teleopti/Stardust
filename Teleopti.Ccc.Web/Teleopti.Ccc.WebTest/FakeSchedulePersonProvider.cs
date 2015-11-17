using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest
{
	public class FakeSchedulePersonProvider : ISchedulePersonProvider
	{
		private readonly IList<IPerson> _persons;

		public FakeSchedulePersonProvider(IEnumerable<IPerson> persons)
		{
			_persons = persons.ToList();
		}

		public IEnumerable<IPerson> GetPermittedPersonsForTeam(DateOnly date, Guid id, string function)
		{
			return _persons;
		}

		public IEnumerable<IPerson> GetPermittedPersonsForGroup(DateOnly date, Guid id, string function)
		{
			return _persons;
		}

		public void Add(IPerson person)
		{
			_persons.Add(person);
		}
	}
}