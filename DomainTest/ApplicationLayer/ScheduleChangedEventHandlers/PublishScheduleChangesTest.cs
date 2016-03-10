using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class PublishScheduleChangesTest
	{
		[Test]
		public void ShouldPublishScheduleChangesToConfiguredSubscribers()
		{
			var newSchedule = new ProjectionChangedEvent
			{
				IsDefaultScenario = true,
				PersonId = Guid.NewGuid(),
				ScheduleDays =
					new List<ProjectionChangedEventScheduleDay>
					{
						new ProjectionChangedEventScheduleDay {Date = new DateTime(2016, 3, 10)}
					}
			};
			var server = new FakeHttpServer();
			var settingsRepository = new FakeGlobalSettingDataRepository();
			var subscriptions = new ScheduleChangeSubscriptions();
			subscriptions.Add(new ScheduleChangeListener {Uri = new Uri("/",UriKind.Relative)});
			settingsRepository.PersistSettingValue(ScheduleChangeSubscriptions.Key, subscriptions);
			var handler = new ScheduleChangesPublisher(server, settingsRepository);
			handler.Handle(newSchedule);

			server.Requests.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldUseUrlConfiguredForSubscriber()
		{
			var newSchedule = new ProjectionChangedEvent
			{
				IsDefaultScenario = true,
				PersonId = Guid.NewGuid(),
				ScheduleDays =
					new List<ProjectionChangedEventScheduleDay>
					{
						new ProjectionChangedEventScheduleDay {Date = new DateTime(2016, 3, 10)}
					}
			};
			var server = new FakeHttpServer();
			var settingsRepository = new FakeGlobalSettingDataRepository();
			var subscriptions = new ScheduleChangeSubscriptions();
			subscriptions.Add(new ScheduleChangeListener { Name = "Facebook", RelativeDateRange = new MinMax<int>(-2, 7), Uri = new Uri("http://facebook") });
			settingsRepository.PersistSettingValue(ScheduleChangeSubscriptions.Key, subscriptions);

			var handler = new ScheduleChangesPublisher(server, settingsRepository);
			handler.Handle(newSchedule);

			server.Requests[0].Uri.Should().Be.EqualTo("http://facebook/");
		}
	}
}
