using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[DomainTest]
	public class FetchAgentGroupModelTest
	{
		public IFetchAgentGroupModel Target;
		public FakeAgentGroupRepository AgentGroupRepository;

		[Test]
		public void ShouldIncludePersistedAgentGroupWhenLoadingAll()
		{
			var presentInDb = new AgentGroup(RandomName.Make()).WithId();
			AgentGroupRepository.Add(presentInDb);

			var loaded = Target.FetchAll().Single();

			loaded.Id.Should().Be.EqualTo(presentInDb.Id);
			loaded.Name.Should().Be.EqualTo(presentInDb.Name);

		}

		[Test]
		public void ShouldIncludeContractFilterWhenFetching()
		{
			var filterName = RandomName.Make();
			var contract = new Contract(filterName).WithId();
			var contractFilter = new ContractFilter(contract);
			var agentGroup = new AgentGroup().WithId();
			agentGroup.AddFilter(contractFilter);
			AgentGroupRepository.Add(agentGroup);

			var loaded = Target.FetchAll().Single(x => x.Id.Equals(agentGroup.Id.Value));

			var filter = loaded.Filters.Single();
			filter.FilterType.Should().Be.EqualTo(FilterModel.ContractFilterType);
			filter.Id.Should().Be.EqualTo(contract.Id.Value);
			filter.Name.Should().Be.EqualTo(filterName);
		}

		[Test]
		public void ShouldIncludeTeamFilterWhenFetching()
		{
			var filterName = RandomName.Make();
			var team = new Team().WithId().WithDescription(new Description(filterName));
			var teamFilter = new TeamFilter(team);
			var agentGroup = new AgentGroup().WithId();
			agentGroup.AddFilter(teamFilter);
			AgentGroupRepository.Add(agentGroup);

			var loaded = Target.FetchAll().Single(x => x.Id.Equals(agentGroup.Id.Value));

			var filter = loaded.Filters.Single();
			filter.FilterType.Should().Be.EqualTo(FilterModel.TeamFilterType);
			filter.Id.Should().Be.EqualTo(team.Id.Value);
			filter.Name.Should().Be.EqualTo(filterName);
		}

		[Test]
		public void ShouldIncludeSiteFilterWhenFetching()
		{
			var filterName = RandomName.Make();
			var site = new Site(filterName).WithId();
			var siteFilter = new SiteFilter(site);
			var agentGroup = new AgentGroup().WithId();
			agentGroup.AddFilter(siteFilter);
			AgentGroupRepository.Add(agentGroup);

			var loaded = Target.FetchAll().Single(x => x.Id.Equals(agentGroup.Id.Value));

			var filter = loaded.Filters.Single();
			filter.FilterType.Should().Be.EqualTo(FilterModel.SiteFilterType);
			filter.Id.Should().Be.EqualTo(site.Id.Value);
			filter.Name.Should().Be.EqualTo(filterName);
		}

		[Test]
		public void ShouldIncludeSkillFilterWhenFetching()
		{
			var filterName = RandomName.Make();
			var skill = new Skill(filterName).WithId();
			var skillFilter = new SkillFilter(skill);
			var agentGroup = new AgentGroup().WithId();
			agentGroup.AddFilter(skillFilter);
			AgentGroupRepository.Add(agentGroup);

			var loaded = Target.FetchAll().Single(x => x.Id.Equals(agentGroup.Id.Value));

			var filter = loaded.Filters.Single();
			filter.FilterType.Should().Be.EqualTo(FilterModel.SkillFilterType);
			filter.Id.Should().Be.EqualTo(skill.Id.Value);
			filter.Name.Should().Be.EqualTo(filterName);
		}


		[Test]
		public void ShouldFetchAgentGroup()
		{
			var curr = new AgentGroup(RandomName.Make()).WithId();
			AgentGroupRepository.Add(curr);

			var agentGroupModel = Target.Fetch(curr.Id.Value);
			agentGroupModel.Id.Should().Be.EqualTo(curr.Id);
			agentGroupModel.Name.Should().Be.EqualTo(curr.Name);
		}

		[Test]
		public void ShouldThrowIfFetchNonExisting()
		{
			Assert.Throws<ArgumentException>(() => Target.Fetch(Guid.NewGuid()));
		}
	}
}