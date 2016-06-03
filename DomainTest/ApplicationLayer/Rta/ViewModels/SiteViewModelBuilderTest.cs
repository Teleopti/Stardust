using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	[TestFixture]
	public class SiteViewModelBuilderTest
	{
		public SiteViewModelBuilder Target;
		public FakeSiteRepository Sites;

		[Test]
		public void ShouldBuild()
		{
			Sites.Has(new Site("Paris"));

			var result = Target.Build();

			result.Single().Name.Should().Be("Paris");
		}

		[Test]
		public void ShouldSortByName()
		{
			Sites.Has(new Site("C"));
			Sites.Has(new Site("B"));
			Sites.Has(new Site("A"));

			var result = Target.Build();

			result.Select(x => x.Name)
				.Should().Have.SameSequenceAs(new[] {"A", "B", "C"});
		}
	}
}