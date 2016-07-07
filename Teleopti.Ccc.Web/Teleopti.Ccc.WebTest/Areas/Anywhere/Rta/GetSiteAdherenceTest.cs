﻿using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Rta
{
	[TestFixture]
	[IoCTest]
	public class GetSiteAdherenceTest : ISetup
	{
		public IGetSiteAdherence Target;
		public FakeSiteRepository Sites;
		public FakeSiteOutOfAdherenceReadModelPersister OutOfAdherence;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new WebAppModule(configuration));
			system.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			system.UseTestDouble<FakeSiteOutOfAdherenceReadModelPersister>().For<ISiteOutOfAdherenceReadModelReader>();
		}

		[Test]
		public void ShouldReturnOutOfAdherenceCount()
		{
			var site = new Site("s").WithId();
			Sites.Has(site);
			OutOfAdherence.Has(new SiteOutOfAdherenceReadModel
			{
				SiteId = site.Id.Value,
				Count = 1
			});
			
			var result = Target.OutOfAdherence();

			result.Single().Id.Should().Be(site.Id);
			result.Single().OutOfAdherence.Should().Be(1);
		}

		[Test]
		public void ShouldReturnSitesWithoutAdherence()
		{
			var site = new Site("s").WithId();
			Sites.Has(site);
			
			var result = Target.OutOfAdherence();

			result.Single().Id.Should().Be(site.Id);
			result.Single().OutOfAdherence.Should().Be(0);
		}
	}
}
