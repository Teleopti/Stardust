using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.ApplicationLayer.ViewModels.AgentStateViewModelBuilder
{
	[DomainTest]
	[TestFixture]
	public class AgentStateValuesTest : ISetup
	{
		public AgentStatesViewModelBuilder Target;
		public FakeAgentStateReadModelPersister Database;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		public FakeUserCulture Culture;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeAgentStateReadModelPersister>().For<IAgentStateReadModelReader>();
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			system.UseTestDouble(new FakeUserCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserCulture>();
		}

		[Test]
		public void ShouldMapAgentStatesToViewModel()
		{
			var personId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Database.Has(
				new AgentStateReadModel
				{
					PersonId = personId,
					SiteId = siteId,
					TeamId = teamId,
					StateName = "state",
					StateStartTime = "2015-10-22 08:00".Utc(),
					Activity = "phone",
					NextActivity = "lunch",
					NextActivityStartTime = "2015-10-22 09:00".Utc(),
					RuleName = "in adherence",
					RuleStartTime = "2015-10-22 08:00".Utc(),
					RuleColor = 0
				}
				);
			Now.Is("2015-10-22 08:30".Utc());
			
			var states = Target.Build(new AgentStateFilter { TeamIds = new[] { teamId }}).States;

			states.Single().PersonId.Should().Be(personId);
			states.Single().State.Should().Be("state");
			states.Single().Activity.Should().Be("phone");
			states.Single().NextActivity.Should().Be("lunch");
			states.Single().NextActivityStartTime.Should().Be("09:00");
			states.Single().Rule.Should().Be("in adherence");
			states.Single().Color.Should().Be("#000000");
			states.Single().TimeInState.Should().Be(30 * 60);
		}
		
		[Test]
		public void ShouldHaveWhiteAsDefaultColor()
		{
			var teamId = Guid.NewGuid();
			Database.Has(
				new AgentStateReadModel
				{
					TeamId = teamId
				}
				);

			var states = Target.Build(new AgentStateFilter { TeamIds = new[] { teamId } }).States;

			states.Single().Color.Should().Be("#FFFFFF");
		}

		[Test]
		public void ShouldGetActivityTimeInLoggedOnUserTimeZone()
		{
			var teamId = Guid.NewGuid();
			Database.Has(
				new AgentStateReadModel
				{
					TeamId = teamId,
					NextActivityStartTime = "2015-11-23 09:00".Utc()
				}
				);
			Now.Is("2015-11-23 08:30".Utc());
			TimeZone.IsSweden();
			Culture.IsSwedish();

			var states = Target.Build(new AgentStateFilter { TeamIds = new[] { teamId } }).States;

			states.Single().NextActivityStartTime.Should().Be("10:00");
		}

		[Test]
		public void ShouldGetActivityTimeForTomorrow()
		{
			var teamId = Guid.NewGuid();
			Database.Has(
				new AgentStateReadModel
				{
					TeamId = teamId,
					NextActivityStartTime = "2015-11-24 09:00".Utc()
				}
				);
			Now.Is("2015-11-23 17:30".Utc());
			TimeZone.IsSweden();
			Culture.IsSwedish();

			var states = Target.Build(new AgentStateFilter { TeamIds = new[] { teamId } }).States;

			states.Single().NextActivityStartTime.Should().Be("2015-11-24 10:00");
		}

		[Test]
		public void ShouldGetNullActivityTime()
		{
			var teamId = Guid.NewGuid();
			Database.Has(
				new AgentStateReadModel
				{
					TeamId = teamId,
					NextActivityStartTime = null
				}
				);
			Now.Is("2015-11-23 08:30".Utc());

			var states = Target.Build(new AgentStateFilter { TeamIds = new[] { teamId } }).States;

			states.Single().NextActivityStartTime.Should().Be(null);
		}
		
		[Test]
		public void ShouldBeNullWhenThereIsNoAlarmStartTime()
		{
			var teamId = Guid.NewGuid();
			Database.Has(
				new AgentStateReadModel
				{
					TeamId = teamId
				}
				);

			var states = Target.Build(new AgentStateFilter { TeamIds = new[] { teamId } }).States;

			states.Single().TimeInAlarm.Should().Be(null);
		}

		[Test]
		public void ShouldHaveCalculatedTimeInAlarm()
		{
			var teamId = Guid.NewGuid();
			Database.Has(
				new AgentStateReadModel
				{
					TeamId = teamId,
					AlarmStartTime = "2015-12-22 08:00".Utc()
				}
			);
			Now.Is("2015-12-22 08:01".Utc());

			var states = Target.Build(new AgentStateFilter { TeamIds = new[] { teamId } }).States;

			states.Single().TimeInAlarm.Should().Be(60);
		}

		[Test]
		public void ShouldBeNullWhenAlarmHasNotStartedYet()
		{
			var teamId = Guid.NewGuid();
			Database.Has(
				new AgentStateReadModel
				{
					TeamId = teamId,
					AlarmStartTime = "2015-12-22 09:00".Utc()
				}
				);
			Now.Is("2015-12-22 08:30".Utc());

			var states = Target.Build(new AgentStateFilter { TeamIds = new[] { teamId } }).States;

			states.Single().TimeInAlarm.Should().Be(null);
		}

		[Test]
		public void ShouldBeAlarmColorWhenAlarmHasStarted()
		{
			var teamId = Guid.NewGuid();
			Database.Has(
				new AgentStateReadModel
				{
					TeamId = teamId,
					AlarmStartTime = "2015-12-22 08:00".Utc(),
					RuleColor = Color.Orange.ToArgb(),
					AlarmColor = Color.Red.ToArgb()
				}
				);
			Now.Is("2015-12-22 08:30".Utc());

			var states = Target.Build(new AgentStateFilter { TeamIds = new[] { teamId } }).States;

			states.Single().Color.Should().Be(ColorTranslator.ToHtml(Color.FromArgb(Color.Red.ToArgb())));
		}
	}
}