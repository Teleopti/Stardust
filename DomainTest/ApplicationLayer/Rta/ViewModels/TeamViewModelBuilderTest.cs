﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
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
	public class TeamViewModelBuilderTest : ISetup
	{
		public TeamViewModelBuilder Target;
		public FakeDatabase Database;
		public FakeUserUiCulture UiCulture;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserUiCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserUiCulture>();
		}

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

		[Test]
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

		[Test]
		public void ShouldSortSwedishName()
		{
			var site = Guid.NewGuid();
			Database
				.WithSite(site)
				.WithTeam("Ä")
				.WithTeam("A")
				.WithTeam("Ĺ");
			UiCulture.IsSwedish();

			var result = Target.Build(site);

			result.Select(x => x.Name)
				.Should().Have.SameSequenceAs(new[] { "A", "Ĺ", "Ä" });
		}
	}
}