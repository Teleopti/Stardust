using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization.Filters;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.Filter
{
	[DomainTest]
	public class FindFilterTest
	{
		public FindFilter Target;
		public FakeContractRepository ContractRepository;
		public FakeTeamRepository TeamRepository;
		public FakeSiteRepository SiteRepository;

		[Test]
		public void ShouldGiveEmptyResult()
		{
			Target.Search(RandomName.Make())
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldFindContract()
		{
			var expectedContractName = RandomName.Make();
			var expectedGuid = Guid.NewGuid();
			const string expectedType = "contract";
			var contract = new Contract(expectedContractName + RandomName.Make()).WithId(expectedGuid);
			ContractRepository.Add(contract);

			var result = Target.Search(expectedContractName).Single();

			result.Name.Should().StartWith(expectedContractName);
			result.Id.Should().Be.EqualTo(expectedGuid);
			result.FilterType.Should().Be.EqualTo(expectedType);
		}

		[Test]
		public void ShouldFindTeam()
		{
			var expectedTeamName = RandomName.Make();
			var expectedGuid = Guid.NewGuid();
			const string expectedType = "team";
			var team = new Team {Description = new Description(expectedTeamName)}.WithId(expectedGuid);
			TeamRepository.Add(team);

			var result = Target.Search(expectedTeamName).Single();

			result.Name.Should().StartWith(expectedTeamName);
			result.Id.Should().Be.EqualTo(expectedGuid);
			result.FilterType.Should().Be.EqualTo(expectedType);
		}

		[Test]
		public void ShouldFindSite()
		{
			var expectedSiteName = RandomName.Make();
			var expectedGuid = Guid.NewGuid();
			const string expectedType = "site";
			var site = new Site(expectedSiteName).WithId(expectedGuid);
			SiteRepository.Add(site);

			var result = Target.Search(expectedSiteName).Single();

			result.Name.Should().StartWith(expectedSiteName);
			result.Id.Should().Be.EqualTo(expectedGuid);
			result.FilterType.Should().Be.EqualTo(expectedType);
		}

		[Test]
		public void EmptySearchStringShouldGiveNoResult()
		{
			var site = new Site(RandomName.Make());
			site.SetId(Guid.NewGuid());
			SiteRepository.Add(site);

			Target.Search(string.Empty)
				.Should().Be.Empty();
		}
	}
}