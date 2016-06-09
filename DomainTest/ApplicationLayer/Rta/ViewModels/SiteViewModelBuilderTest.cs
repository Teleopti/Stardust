using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	[TestFixture]
	public class SiteViewModelBuilderTest : ISetup
	{
		public SiteViewModelBuilder Target;
		public FakeSiteRepository Sites;
		public FakeUserCulture Culture;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserCulture>();
		}

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

		[Test]
		public void ShouldSortSwedishName()
		{
			Sites.Has(new Site("Ä"));
			Sites.Has(new Site("A"));
			Sites.Has(new Site("Å"));

			Culture.IsSwedish();
			var result = Target.Build();

			result.Select(x => x.Name)
				.Should().Have.SameSequenceAs(new[] {"A", "Å", "Ä"});
		}
	}
}