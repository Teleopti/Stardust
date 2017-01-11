﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Rta
{
	[TestFixture]
	[IoCTest]
	public class GetSiteAdherenceTest : ISetup
	{
		public AgentsInAlarmForSiteViewModelBuilder Target;
		public FakeSiteRepository Sites;
		public FakeSiteInAlarmReader AgentState;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			system.UseTestDouble<FakeSiteInAlarmReader>().For<ISiteInAlarmReader>();
		}

		[Test]
		public void ShouldReturnOutOfAdherenceCount()
		{
			var site = new Site("s").WithId();
			Sites.Has(site);
			AgentState.Has(new AgentStateReadModel
			{
				SiteId = site.Id.Value,
				IsRuleAlarm = true,
				AlarmStartTime = DateTime.MinValue
			});

			var result = Target.Build();

			result.Single().Id.Should().Be(site.Id);
			result.Single().OutOfAdherence.Should().Be(1);
		}

		[Test]
		public void ShouldReturnSitesWithoutAdherence()
		{
			var site = new Site("s").WithId();
			Sites.Has(site);
			
			var result = Target.Build();

			result.Single().Id.Should().Be(site.Id);
			result.Single().OutOfAdherence.Should().Be(0);
		}
	}

}
