using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.AgentInfo
{
	[DomainTest]
	public class PlanningGroupStaffLoaderTest
	{
		public FakePersonRepository PersonRepository;
		public IPlanningGroupStaffLoader Target;

		[Test]
		public void ShouldLoadStaffForPlanningGroup()
		{
			var team = TeamFactory.CreateTeamWithId(Guid.NewGuid());
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(Guid.NewGuid(), new DateOnly(2017, 1, 1), team);
			var planningGroup = new PlanningGroup()
				.WithId()
				.AddFilter(new TeamFilter(team));
			PersonRepository.Add(person);

			var result = Target.Load(new DateOnlyPeriod(2017, 1, 1, 2017, 1, 28), planningGroup);

			result.AllPeople.Single().Name.ToString().Should().Be.EqualTo(person.Name.ToString());
		}
	}
}