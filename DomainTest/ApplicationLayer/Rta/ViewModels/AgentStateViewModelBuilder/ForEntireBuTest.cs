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
		public void ShouldGetMamimum50AgentStatesForEntireBu()
		{
			Enumerable.Range(0, 51)
				.ForEach(i =>
				{
					Database
						.Has(new AgentStateReadModel
						{
							PersonId = Guid.NewGuid(),
							SiteId = Guid.NewGuid(),
						});
				});

			var agentState = Target.For(new AgentStateFilter {InAlarm = false}).States.ToArray();

			agentState.Length.Should().Be(50);
		}

		[Test]
		public void ShouldGetMamimum50AgentStatesInAlarmForEntireBu()
		{
			Now.Is("2017-08-02 08:30");
			Enumerable.Range(0, 51)
				.ForEach(i =>
				{
					Database
						.Has(new AgentStateReadModel
						{
							PersonId = Guid.NewGuid(),
							SiteId = Guid.NewGuid(),
							AlarmStartTime = "2017-08-02 08:00".Utc(),
							IsRuleAlarm = true
						});
				});

			var agentState = Target.For(new AgentStateFilter {InAlarm = true}).States.ToArray();

			agentState.Length.Should().Be(50);
		}
	}
}