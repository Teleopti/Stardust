using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	[TestFixture]
	public class SiteViewModelBuilderTest : ISetup
	{
		public SiteViewModelBuilder Target;
		public FakeDatabase Database;
		public FakeNumberOfAgentsInSiteReader AgentsInSite;
		public FakeUserUiCulture UiCulture;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserUiCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserUiCulture>();
		}

		[Test]
		public void ShouldBuild()
		{
			Database
				.WithSite(null, "Paris");
			
			var result = Target.Build();

			result.Single().Name.Should().Be("Paris");
		}

		[Test]
		public void ShouldSortByName()
		{
			Database
				.WithSite(null, "C")
				.WithSite(null, "B")
				.WithSite(null, "A");

			var result = Target.Build();

			result.Select(x => x.Name)
				.Should().Have.SameSequenceAs(new[] {"A", "B", "C"});
		}

		[Test]
		public void ShouldSortSwedishName()
		{
			Database
				.WithSite(null, "Ä")
				.WithSite(null, "A")
				.WithSite(null, "Å");

			UiCulture.IsSwedish();
			var result = Target.Build();

			result.Select(x => x.Name)
				.Should().Have.SameSequenceAs(new[] {"A", "Å", "Ä"});
		}
		
		[Test]
		public void ShouldGetNumberOfAgents()
		{
			var siteId = Guid.NewGuid();
			Database
				.WithSite(siteId, "Paris");
			AgentsInSite.Has(siteId, 20);

			var result = Target.Build().Single();

			result.Name.Should().Be("Paris");
			result.Id.Should().Be(siteId);
			result.NumberOfAgents.Should().Be(20);
		}
		
		[Test]
		public void ShouldGetNumberOfAgentsForSkill()
		{
			var siteId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			Database
				.WithSite(siteId, "Paris");
			AgentsInSite.Has(siteId, skillId, 20);

			var result = Target.ForSkills(new [] {skillId}).Single();

			result.Name.Should().Be("Paris");
			result.Id.Should().Be(siteId);
			result.NumberOfAgents.Should().Be(20);
		}

		[Test]
		public void ShouldNotReturnEmptySite()
		{
			var siteId = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			Database
				.WithSite(siteId)
				.WithSite();
			AgentsInSite.Has(siteId, skillId, 20);

			Target.ForSkills(new[] {skillId}).Single()
				.Id.Should().Be(siteId);
		}


	}
}