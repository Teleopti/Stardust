using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	public class SendAdherenceMessagesTest
	{
		[Test]
		public void ShouldSendMessageForTeam()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var sender = new FakeMessageSender();
			var teamId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", Guid.NewGuid(), null, teamId, null)
				.Done();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(state.Timestamp), sender);

			target.SaveExternalUserState(state);

			sender.LastNotification.DomainId.Should().Be(teamId.ToString());
		}

		[Test]
		public void ShouldNotSendMessageIfAdherenceHasNotChanged()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var sender = new FakeMessageSender();
			var teamId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId, null, teamId, null)
				.WithSchedule(personId, phone, state.Timestamp.AddHours(-1), state.Timestamp.AddHours(1))
				.WithAlarm("statecode", phone, 1)
				.Done();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(state.Timestamp), sender);

			target.SaveExternalUserState(state);
			target.SaveExternalUserState(state);

			sender.AllNotifications.Where(x => x.DomainType == typeof(TeamAdherenceMessage).Name).Should().Have.Count.EqualTo(1);
			sender.AllNotifications.Where(x => x.DomainType == typeof(SiteAdherenceMessage).Name).Should().Have.Count.EqualTo(1);
			sender.AllNotifications.Where(x => x.DomainType == typeof(AgentsAdherenceMessage).Name).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldSetBusinessIdOnTeamMessage()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var sender = new FakeMessageSender();
			var businessUnidId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId, businessUnidId, Guid.NewGuid(), null)
				.Done();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(state.Timestamp), sender);

			target.SaveExternalUserState(state);

			sender.LastTeamNotification.BusinessUnitId.Should().Be.EqualTo(businessUnidId.ToString());
		}

		[Test]
		public void ShouldSetDomainTypeOnTeamMessage()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var sender = new FakeMessageSender();
			var businessUnidId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId, businessUnidId, Guid.NewGuid(), null)
				.Done();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(state.Timestamp), sender);

			target.SaveExternalUserState(state);

			sender.AllNotifications.Where(x => x.DomainType == typeof(TeamAdherenceMessage).Name).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldSetBusinessIdOnSiteMessage()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var sender = new FakeMessageSender();
			var businessUnidId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId, businessUnidId, null, Guid.NewGuid())
				.Done();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(state.Timestamp), sender);

			target.SaveExternalUserState(state);

			sender.LastSiteNotification.BusinessUnitId.Should().Be.EqualTo(businessUnidId.ToString());
		}

		[Test]
		public void ShouldSetDomainTypeOnSiteMessage()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var sender = new FakeMessageSender();
			var businessUnidId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId, businessUnidId, null, Guid.NewGuid())
				.Done();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(state.Timestamp), sender);

			target.SaveExternalUserState(state);

			sender.AllNotifications.Where(x => x.DomainType == typeof(SiteAdherenceMessage).Name).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldNotSendMessageIfPersonIdIsNotAvailable()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var sender = new FakeMessageSender();
			var database = new FakeRtaDatabase()
				.WithSource(state.SourceId)
				.Done();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(state.Timestamp), sender);

			target.SaveExternalUserState(state);

			sender.AllNotifications.Should().Be.Empty();
		}
	}
}