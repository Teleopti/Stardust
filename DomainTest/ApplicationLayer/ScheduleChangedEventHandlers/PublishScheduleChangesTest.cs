using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Messaging.Client.Http;

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
			subscriptions.Add(new ScheduleChangeSubscription {Uri = new Uri("/",UriKind.Relative)});
			settingsRepository.PersistSettingValue("ScheduleChangeSubscriptions", subscriptions);
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
			subscriptions.Add(new ScheduleChangeSubscription { Name = "Facebook", RelativeDateRange = new MinMax<int>(-2, 7), Uri = new Uri("http://facebook") });
			settingsRepository.PersistSettingValue("ScheduleChangeSubscriptions", subscriptions);

			var handler = new ScheduleChangesPublisher(server, settingsRepository);
			handler.Handle(newSchedule);

			server.Requests[0].Uri.Should().Be.EqualTo("http://facebook/");
		}
	}

	[Serializable]
	public class ScheduleChangeSubscriptions : SettingValue
	{
		private readonly IList<ScheduleChangeSubscription> subscriptions = new List<ScheduleChangeSubscription>();

		public void Add(ScheduleChangeSubscription subscription)
		{
			subscriptions.Add(subscription);
		}

		public void Remove(ScheduleChangeSubscription subscription)
		{
			subscriptions.Remove(subscription);
		}

		public ScheduleChangeSubscription[] Subscriptions()
		{
			return subscriptions.ToArray();
		}
	}

	public class ScheduleChangeSubscription
	{
		public string Name { get; set; }
		public Uri Uri { get; set; }
		public MinMax<int> RelativeDateRange {get; set;}
	}

	public class ScheduleChangesPublisher : IHandleEvent<ProjectionChangedEvent>, IRunOnServiceBus
	{
		private readonly IHttpServer _server;
		private readonly ISettingDataRepository _settingsRepository;

		public ScheduleChangesPublisher(IHttpServer server, ISettingDataRepository settingsRepository)
		{
			_server = server;
			_settingsRepository = settingsRepository;
		}

		public void Handle(ProjectionChangedEvent @event)
		{
			var settings = _settingsRepository.FindValueByKey("ScheduleChangeSubscriptions", new ScheduleChangeSubscriptions());
			foreach (var scheduleChangeSubscription in settings.Subscriptions())
			{
				_server.PostOrThrow(scheduleChangeSubscription.Uri.ToString(), @event);
			}
		}
	}
}
