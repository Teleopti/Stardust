using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels.AgentStateViewModelBuilder
{
	[DomainTest]
	[TestFixture]
	public class ForEntireBuTest : ISetup
	{
		public AgentStatesViewModelBuilder Target;
		public FakeAgentStateReadModelPersister Database;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			system.UseTestDouble(new FakeUserCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserCulture>();
		}


		[Test]
		public void ShouldGetAgentStatesForEntireBu()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var siteLondonId = Guid.NewGuid();
			var siteParisId = Guid.NewGuid();

		
					Database
						.Has(new AgentStateReadModel
						{
							PersonId = person1,
							SiteId = siteLondonId
						})
						.Has(new AgentStateReadModel
						{
							PersonId = person2,
							SiteId = siteParisId
						});

			Target.For(new AgentStateFilter { InAlarm = false })
				.States.Select(x => x.SiteId)
				.Should().Have.SameValuesAs(siteLondonId.ToString(), siteParisId.ToString());
			
		}

		[Test]
		public void ShouldGetAgentStatesInAlarmForEntireBu()
		{
			Now.Is("2017-08-04 08:30");
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var siteLondonId = Guid.NewGuid();
			var siteParisId = Guid.NewGuid();


			Database
				.Has(new AgentStateReadModel
				{
					PersonId = person1,
					SiteId = siteLondonId,
					AlarmStartTime = "2017-08-03 08:00".Utc(),
					IsRuleAlarm = true

				})
				.Has(new AgentStateReadModel
				{
					PersonId = person2,
					SiteId = siteParisId,
					AlarmStartTime = "2017-08-03 08:00".Utc(),
					IsRuleAlarm = true
				});

			Target.For(new AgentStateFilter { InAlarm = true })
				.States.Select(x => x.SiteId)
				.Should().Have.SameValuesAs(siteLondonId.ToString(), siteParisId.ToString());
		}
	}
}