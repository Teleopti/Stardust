using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.Optimization.Filter
{
	[DomainTest]
	public class FindFilterTest
	{
		public FindFilter Target;
		public FakeContractRepository ContractRepository;
		public FakeTeamRepository TeamRepository;
		public FakeSiteRepository SiteRepository;
		public FakeSkillRepository SkillRepository;

		[Test]
		public void ShouldGiveEmptyResult()
		{
			Target.Search(RandomName.Make(), 10)
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldFindContract()
		{
			var searchString = RandomName.Make();
			var expectedGuid = Guid.NewGuid();
			const string expectedType = "contract";
			var contract = new Contract(RandomName.Make() + searchString + RandomName.Make()).WithId(expectedGuid);
			ContractRepository.Add(contract);

			var result = Target.Search(searchString, 10).Single();

			result.Name.Should().Contain(searchString);
			result.Id.Should().Be.EqualTo(expectedGuid);
			result.FilterType.Should().Be.EqualTo(expectedType);
		}

		[Test]
		public void ShouldFindTeam()
		{
			var searchString = RandomName.Make();
			var expectedGuid = Guid.NewGuid();
			const string expectedType = "team";
			var team = new Team().WithId(expectedGuid).WithDescription(new Description(RandomName.Make() + searchString + RandomName.Make()));
			team.Site = new Site(RandomName.Make());
			TeamRepository.Add(team);

			var result = Target.Search(searchString, 10).Single();

			result.Name.Should().Contain(searchString);
			result.Id.Should().Be.EqualTo(expectedGuid);
			result.FilterType.Should().Be.EqualTo(expectedType);
		}

		[Test]
		public void ShouldFindSite()
		{
			var searchString = RandomName.Make();
			var expectedGuid = Guid.NewGuid();
			const string expectedType = "site";
			var site = new Site(RandomName.Make() + searchString + RandomName.Make()).WithId(expectedGuid);
			SiteRepository.Add(site);

			var result = Target.Search(searchString, 10).Single();

			result.Name.Should().Contain(searchString);
			result.Id.Should().Be.EqualTo(expectedGuid);
			result.FilterType.Should().Be.EqualTo(expectedType);
		}

		[Test]
		public void ShouldOnlyFetchMaxHits()
		{
			const int maxHits = 3;
			var name = RandomName.Make();
			SiteRepository.Add(new Site(name + RandomName.Make()).WithId());
			SiteRepository.Add(new Site(name + RandomName.Make()).WithId());
			TeamRepository.Add(new Team().WithId().WithDescription(new Description(name + RandomName.Make())));
			TeamRepository.Add(new Team().WithId().WithDescription(new Description(name + RandomName.Make())));
			ContractRepository.Add(new Contract(name + RandomName.Make()).WithId());
			ContractRepository.Add(new Contract(name + RandomName.Make()).WithId());

			Target.Search(name, maxHits).Count()
				.Should().Be.EqualTo(maxHits);
		}
		
		[Test]
		public void ShouldReturnAllWhenSearchEmpty()
		{
			var name = RandomName.Make();
			SiteRepository.Add(new Site(name + RandomName.Make()).WithId());
			SiteRepository.Add(new Site(name + RandomName.Make()).WithId());
			ContractRepository.Add(new Contract(name + RandomName.Make()).WithId());
			ContractRepository.Add(new Contract(name + RandomName.Make()).WithId());

			Target.Search(name, 10).Count()
				.Should().Be.EqualTo(4);
		}

		[Test]
		public void ShouldGiveEmptyResultForPlanningGroup()
		{
			Target.SearchForPlanningGroup(RandomName.Make(), 10)
				.Should().Be.Empty();
		}
		
		[Test]
		public void ShouldReturnAllWhenSearchEmptyForPlanningGroup()
		{
			var name = RandomName.Make();
			SiteRepository.Add(new Site(name + RandomName.Make()).WithId());
			SiteRepository.Add(new Site(name + RandomName.Make()).WithId());
			ContractRepository.Add(new Contract(name + RandomName.Make()).WithId());
			ContractRepository.Add(new Contract(name + RandomName.Make()).WithId());
			
			Target.SearchForPlanningGroup("", 10).Count()
				.Should().Be.EqualTo(4);
		}

		[Test]
		public void ShouldFindContractForPlanningGroup()
		{
			var searchString = RandomName.Make();
			var expectedGuid = Guid.NewGuid();
			const string expectedType = "contract";
			var contract = new Contract(RandomName.Make() + searchString + RandomName.Make()).WithId(expectedGuid);
			ContractRepository.Add(contract);

			var result = Target.SearchForPlanningGroup(searchString, 10).Single();

			result.Name.Should().Contain(searchString);
			result.Id.Should().Be.EqualTo(expectedGuid);
			result.FilterType.Should().Be.EqualTo(expectedType);
		}

		[Test]
		public void ShouldFindTeamForPlanningGroup()
		{
			var searchString = RandomName.Make();
			var expectedGuid = Guid.NewGuid();
			const string expectedType = "team";
			var team = new Team().WithId(expectedGuid).WithDescription(new Description(RandomName.Make() + searchString + RandomName.Make()));
			team.Site = new Site(RandomName.Make());
			TeamRepository.Add(team);

			var result = Target.SearchForPlanningGroup(searchString, 10).Single();

			result.Name.Should().Contain(searchString);
			result.Id.Should().Be.EqualTo(expectedGuid);
			result.FilterType.Should().Be.EqualTo(expectedType);
		}

		[Test]
		public void ShouldFindSiteForPlanningGroup()
		{
			var searchString = RandomName.Make();
			var expectedGuid = Guid.NewGuid();
			const string expectedType = "site";
			var site = new Site(RandomName.Make() + searchString + RandomName.Make()).WithId(expectedGuid);
			SiteRepository.Add(site);

			var result = Target.SearchForPlanningGroup(searchString, 10).Single();

			result.Name.Should().Contain(searchString);
			result.Id.Should().Be.EqualTo(expectedGuid);
			result.FilterType.Should().Be.EqualTo(expectedType);
		}

		[Test]
		public void ShouldFindSkillForPlanningGroup()
		{
			var searchString = RandomName.Make();
			var expectedGuid = Guid.NewGuid();
			const string expectedType = "skill";
			var skill = new Skill(RandomName.Make()+ searchString + RandomName.Make()).WithId(expectedGuid);
			SkillRepository.Add(skill);

			var result = Target.SearchForPlanningGroup(searchString, 10).Single();

			result.Name.Should().Contain(searchString);
			result.Id.Should().Be.EqualTo(expectedGuid);
			result.FilterType.Should().Be.EqualTo(expectedType);
		}

		[Test]
		public void ShouldOnlyFetchMaxHitsForPlanningGroup()
		{
			const int maxHits = 3;
			var name = RandomName.Make();
			var site1 = new Site(name + RandomName.Make()).WithId();
			var site2 = new Site(name + RandomName.Make()).WithId();
			var team1 = new Team().WithId().WithDescription(new Description(name + RandomName.Make()));
			var team2 = new Team().WithId().WithDescription(new Description(name + RandomName.Make()));
			team1.Site = site1;
			team2.Site = site2;
			
			SiteRepository.Add(site1);
			SiteRepository.Add(site2);
			TeamRepository.Add(team1);
			TeamRepository.Add(team2);
			ContractRepository.Add(new Contract(name + RandomName.Make()).WithId());
			ContractRepository.Add(new Contract(name + RandomName.Make()).WithId());

			Target.SearchForPlanningGroup(name, maxHits).Count()
				.Should().Be.EqualTo(maxHits);
		}
	}
}