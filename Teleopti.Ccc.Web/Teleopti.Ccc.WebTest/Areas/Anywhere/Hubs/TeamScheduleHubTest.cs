using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
	public class TeamScheduleHubTest
	{
		[Test]
		public void ShouldQueryReadModelsForTeam()
		{
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelRepository>();
			var teamId = Guid.NewGuid();
			var period = new DateTimePeriod(2013, 3, 4, 2013, 3, 5);
			var target = new TeamScheduleHub(personScheduleDayReadModelRepository);
			var hubBuilder = new TestHubBuilder();
			hubBuilder.SetupHub(target, hubBuilder.FakeCaller<IEnumerable<dynamic>>("incomingTeamSchedule", a => { }));

			target.SubscribeTeamSchedule(teamId, new DateTime(2013, 3, 4, 0, 0, 0, DateTimeKind.Utc));

			personScheduleDayReadModelRepository.AssertWasCalled(x => x.ForTeam(period, teamId));
		}

		[Test]
		public void ShouldPushDataToCallerOnSubscribe()
		{
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelRepository>();
			var teamId = Guid.NewGuid();
			var period = new DateTimePeriod(2013, 3, 4, 2013, 3, 5);
			var target = new TeamScheduleHub(personScheduleDayReadModelRepository);
			var hubBuilder = new TestHubBuilder();
			IEnumerable<dynamic> actual = null;
			hubBuilder.SetupHub(target, hubBuilder.FakeCaller<IEnumerable<dynamic>>("incomingTeamSchedule", a => { actual = a; }));
			var data = new[] {new PersonScheduleDayReadModel {Shift = "{FirstName: 'Pierre'}"}};
			personScheduleDayReadModelRepository.Stub(x => x.ForTeam(period, teamId)).Return(data);

			target.SubscribeTeamSchedule(teamId, new DateTime(2013, 3, 4, 0, 0, 0, DateTimeKind.Utc));

			Assert.That(actual.Single().FirstName, Is.EqualTo("Pierre"));
		}

	}
}