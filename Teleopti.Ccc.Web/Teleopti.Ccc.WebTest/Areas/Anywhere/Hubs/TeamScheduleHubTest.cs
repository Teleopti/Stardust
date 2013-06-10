using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
	public class TeamScheduleHubTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldQueryReadModelsForTeam()
		{
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var teamId = Guid.NewGuid();
			var period = new DateTimePeriod(2013, 3, 4, 2013, 3, 5).ChangeEndTime(TimeSpan.FromHours(1));
			var target = new TeamScheduleHub(personScheduleDayReadModelRepository);
			var hubBuilder = new TestHubBuilder();
			hubBuilder.SetupHub(target, hubBuilder.FakeClient<IEnumerable<dynamic>>("incomingTeamSchedule", a => { }));

			target.SubscribeTeamSchedule(teamId, new DateTime(2013, 3, 4, 0, 0, 0));

			personScheduleDayReadModelRepository.AssertWasCalled(x => x.ForTeam(period, teamId));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldPushDataToCallerOnSubscribe()
		{
			var personScheduleDayReadModelRepository = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var teamId = Guid.NewGuid();
			var period = new DateTimePeriod(2013, 3, 4, 2013, 3, 5).ChangeEndTime(TimeSpan.FromHours(1));
			var target = new TeamScheduleHub(personScheduleDayReadModelRepository);
			var hubBuilder = new TestHubBuilder();
			IEnumerable<dynamic> actual = null;
			hubBuilder.SetupHub(target, hubBuilder.FakeClient<IEnumerable<dynamic>>("incomingTeamSchedule", a => { actual = a; }));
			var data = new[] {new PersonScheduleDayReadModel {Shift = "{FirstName: 'Pierre'}"}};
			personScheduleDayReadModelRepository.Stub(x => x.ForTeam(period, teamId)).Return(data);

			target.SubscribeTeamSchedule(teamId, new DateTime(2013, 3, 4, 0, 0, 0));

			Assert.That(actual.Single().FirstName, Is.EqualTo("Pierre"));
		}

	}
}