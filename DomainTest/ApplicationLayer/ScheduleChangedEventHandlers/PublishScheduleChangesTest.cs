using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Security;
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
			var handler = new ScheduleChangesPublisher(server, new ThisIsNow(new DateTime(2016, 3, 10, 5, 0, 0)), settingsRepository, new SignatureCreator());
			handler.Handle(newSchedule);

			server.Requests.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldIgnoreOtherScenariosThanDefault()
		{
			var newSchedule = new ProjectionChangedEvent
			{
				IsDefaultScenario = false,
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
			subscriptions.Add(new ScheduleChangeListener { Uri = new Uri("/", UriKind.Relative) });
			settingsRepository.PersistSettingValue(ScheduleChangeSubscriptions.Key, subscriptions);
			var handler = new ScheduleChangesPublisher(server, new ThisIsNow(new DateTime(2016, 3, 10, 5, 0, 0)), settingsRepository, new SignatureCreator());
			handler.Handle(newSchedule);

			server.Requests.Count.Should().Be.EqualTo(0);
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

			var handler = new ScheduleChangesPublisher(server, new ThisIsNow(new DateTime(2016, 3, 10, 5, 0, 0)), settingsRepository, new SignatureCreator());
			handler.Handle(newSchedule);

			server.Requests[0].Uri.Should().Be.EqualTo("http://facebook/");
		}

		[Test]
		public void ShouldNotNotifySubscribersOutSideInterval()
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
			subscriptions.Add(new ScheduleChangeListener { Name = "Facebook", RelativeDateRange = new MinMax<int>(4, 7), Uri = new Uri("http://facebook") });
			subscriptions.Add(new ScheduleChangeListener { Name = "Salesforce", RelativeDateRange = new MinMax<int>(-1, 1), Uri = new Uri("http://salesforce") });
			settingsRepository.PersistSettingValue(ScheduleChangeSubscriptions.Key, subscriptions);

			var handler = new ScheduleChangesPublisher(server, new ThisIsNow(new DateTime(2016,3,10,5,0,0)), settingsRepository, new SignatureCreator());
			handler.Handle(newSchedule);

			server.Requests[0].Uri.Should().Be.EqualTo("http://salesforce/");
		}

		[Test]
		public void ShouldAppendSignatureHeader()
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
			subscriptions.Add(new ScheduleChangeListener { Name = "Facebook", RelativeDateRange = new MinMax<int>(-1, 1), Uri = new Uri("http://facebook") });
			settingsRepository.PersistSettingValue(ScheduleChangeSubscriptions.Key, subscriptions);

			var handler = new ScheduleChangesPublisher(server, new ThisIsNow(new DateTime(2016, 3, 10, 5, 0, 0)), settingsRepository,new SignatureCreator());
			handler.Handle(newSchedule);

			server.Requests[0].Headers["Signature"].Should().Be.EqualTo("QLX0n3ffldPGQteTvhzyRUEt4vT4ajiJkn6IRvmXV6YxInpuK3PnT2oIQDRHEl/khUM2pp7pmLcbOJMWwypKabLUPK3p27YbzLZ58aTadDQAaks8BJtXPPQHAgxjL5o2iNaNUfR8+Qqv5WXo8LjyTCE1nnyxCOc5X9AEaPTjHr5Izhe4ZWm/G8lUvSu9Xr4OSZRBpRM8ZsyLSCf3iE5olU3ThSBSIoZA4veYc+XriOYog6Kvcik8AjoduNz/t21Pdf+JqhwvU07VgnSkIXlf3RY63mgTs1P5/gOXVhzAwD0e62HMPxuckEN2d8GKYQKcku51CYjxPFhNmYXFR0Z3Ow==");
		}
	}
}
