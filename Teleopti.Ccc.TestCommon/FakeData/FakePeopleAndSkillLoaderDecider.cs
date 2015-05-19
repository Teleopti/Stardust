using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakePeopleAndSkillLoaderDecider : IPeopleAndSkillLoaderDecider
	{
		public ILoaderDeciderResult Execute(IScenario scenario, DateTimePeriod period, IEnumerable<IPerson> people)
		{
			return new LoaderDeciderResult(period, new Guid[] { people.First().Id.Value }, new Guid[] { Guid.NewGuid() }, new Guid[] { Guid.NewGuid() });
		}
	}
}