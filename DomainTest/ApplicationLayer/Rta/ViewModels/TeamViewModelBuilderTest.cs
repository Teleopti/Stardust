using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	[TestFixture]
	public class TeamViewModelBuilderTest
	{
		public TeamViewModelBuilder Target;
		public FakeDatabase Database;

		[Test]
		public void ShouldBuild()
		{
			var site = Guid.NewGuid();
			Database
				.WithSite(site)
				.WithTeam("Green");

			var result = Target.Build(site);

			result.Single().Name.Should().Be("Green");
		}

		[Test, Ignore]
		public void ShouldSortByName()
		{
			var site = Guid.NewGuid();
			Database
				.WithSite(site)
				.WithTeam("C")
				.WithTeam("B")
				.WithTeam("A");

			var result = Target.Build(site);

			result.Select(x => x.Name)
				.Should().Have.SameSequenceAs(new[] { "A", "B", "C" });
		}
	}
}