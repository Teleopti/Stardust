using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.ResourcePlanner.Core;
using Teleopti.Ccc.WebTest.Areas.Global;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.ResourcePlanner.Core
{
	[TestFixture]
	public class AgentGroupStaffLoaderTest
	{
		[Test]
		public void ShouldUseFixedStaffLoaderIfNoAgentGroup()
		{
			var fixedStaffLoader = new FakeFixedStaffLoader();
			var person = PersonFactory.CreatePerson("test1");
			fixedStaffLoader.SetPeople(person);
			var target = new AgentGroupStaffLoader(fixedStaffLoader, new FakePersonRepository(new FakeStorage()));
			var result = target.Load(new DateOnlyPeriod(2017, 1, 1, 2017, 1, 28), null);

			result.AllPeople.Single().Name.ToString().Should().Be.EqualTo(person.Name.ToString());
		}

		[Test, Ignore("Not implement yet")]
		public void ShouldLoadStaffForAgentGroupWithTeamFilter()
		{
			var team = TeamFactory.CreateTeamWithId(Guid.NewGuid());
			var team2 = TeamFactory.CreateTeamWithId(Guid.NewGuid());
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(Guid.NewGuid(), new DateOnly(2017,1,1), team);
			var person2 = PersonFactory.CreatePersonWithPersonPeriodFromTeam(Guid.NewGuid(), new DateOnly(2017,1,1), team2);
			var peopleSearchProvider = new FakePeopleSearchProvider(new[] { person, person2 }, new List<IOptionalColumn>());
			peopleSearchProvider.Add(person);
			var target = new AgentGroupStaffLoader(null, new FakePersonRepository(new FakeStorage()));
			var agentGroup = new AgentGroup
			{
				Name = "agent group 1",
			};
			agentGroup.AddFilter(new TeamFilter(team));
			agentGroup.SetId(Guid.NewGuid());
			var result = target.Load(new DateOnlyPeriod(2017, 1, 1, 2017, 1, 28), agentGroup);

			result.AllPeople.Single().Name.ToString().Should().Be.EqualTo(person.Name.ToString());
		}

		//[Test, Ignore("Not implement yet")]
		//public void ShouldLoadStaffForAgentGroupWithSiteFilter()
		//{
		//	var fixedStaffLoader = new FakeFixedStaffLoader();
		//	var team = TeamFactory.CreateTeamWithId(Guid.NewGuid());
		//	var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(Guid.NewGuid(), new DateOnly(2017, 1, 1), team);
		//	fixedStaffLoader.SetPeople(person);
		//	var peopleSearchProvider = new FakePeopleSearchProvider(new[] { person }, new List<IOptionalColumn>());
		//	peopleSearchProvider.Add(person);
		//	var target = new AgentGroupStaffLoader(fixedStaffLoader, peopleSearchProvider);
		//	var agentGroup = new AgentGroup
		//	{
		//		Name = "agent group 1",
		//	};
		//	agentGroup.AddFilter(new SiteFilter(team));
		//	agentGroup.SetId(Guid.NewGuid());
		//	var result = target.Load(new DateOnlyPeriod(2017, 1, 1, 2017, 1, 28), agentGroup);

		//	result.AllPeople.Single().Name.ToString().Should().Be.EqualTo(person.Name.ToString());
		//}
	}
}