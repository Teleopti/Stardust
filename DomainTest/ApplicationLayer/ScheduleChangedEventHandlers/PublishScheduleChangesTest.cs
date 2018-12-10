using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core.Registration;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[DomainTest]
	public class PublishScheduleChangesTest
	{
		private readonly SignatureCreator signatureCreator =
			new SignatureCreator(new FakeConfigReader(new Dictionary<string, string>
			{
				{
					"CertificateModulus",
					"tcQWMgdpQeCd8+gzB3rYQAehHXF5mBGdyFMkJMEmcQmTlkpg22xLNz/kNYXZ7j2Cuhls+PBORzZkfBsNoL1vErT+N9Es4EEWOt6ntNe7wujqQqktUT/QOWEMJ8zJQM3bn7Oj9H5StBr7DWSRzgEjOc7knDcb4KCQL3ceXqmqwSonPfP1hp+bE8rZuxDISYiZVEkm417YzUHBk3ppV30Q9zvfL9IZX0q/ebCTRnLFockl7yOVucomvo8j4ssFPCAYgASoNvzWq+s5UTzYELl1I7F3hQnFwx0bIpQFmGbZ5BbNczc6rVYtCX5KDMsVaJSUcXBAnqGd20hq/ICkBR658w=="
				},
				{"CertificateExponent", "AQAB"},
				{
					"CertificateP",
					"8r2FFhgc78WZf/uKEjHPyiLL9FkcjbPsdLB/Dd6AEOVuzpVFlBJsai31gyLIUU3zY6gE/NdMZzQ7ejsjhbpC4/ptbJguTpIOGB+7dX+/DEdwZkx8rIlNG32VDIdP6kqpwPzhtGVfNiq8xHaS+SHTQf6JSWQtNKgVbilWgYyEZ9k="
				},
				{
					"CertificateQ",
					"v7Hm0iP49ReGhVvKdAsDUgcK0olmVGAKwsxsFnXUGkAWydh+T3QaChYkYBS+h5cX4UBlil5FaJKtGq4wduKDkMCN8TNHh3n2k05rh4DxxPmLvhCqkQgvMB/22E+z10VAmjKPq7BnAq/lc2rGJWa1lq3qaeSkcF6agCPQVYd6vKs="
				},
				{
					"CertificateDP",
					"7mr7dvIEKfVZiX0U5j4Kq611yfBkvUHFs+9PO94Yx3+yUDIJfyCBX+D4Te8x9bmsn2t+SqFlJ9EDwlCn2UdTP/zO0WS/xuhp84PnaccpbPQWEER8CDNrit7UMNQOyD7BcQ5w2fDfjaJ4ejdEsHJqv100luNQC3I0alkr4F6WBjE="
				},
				{
					"CertificateDQ",
					"nZ2rSlGlm/BR/Ujx9+QuQL3lmiK7btjhQDZREU6krUjQ8/n8MVwnJO/7zLyBxH7pdZ47X0AQFeG0T2G2G6o3v0dz7kTZpX0Uzx4FsA7Hu8vrqMWPWVy/X/SIRGeUWYZpjd/Q3bxXlpAGO5Ypggsnd9NcEOGci4BdzMqlvA1/T60="
				},
				{
					"CertificateInverseQ",
					"4OVuc98gImxH9yLmvMimnr9zFxw6pwRd++/A7q1Uj8BrCKhzD8rY6QosNZCKlMjWdHNYtAWrDDqxwCRPUYhmdOm+JEkxBL7yelodmongNXUS0J9kf9c+k8fjgEsB02J15QqryyuRvw0Z638BheD9Ry3NN/REO0pdnj2LHxa17V0="
				},
				{
					"CertificateD",
					"YQOGoS8pc9rSE1OUoOJlN0+bI57kOlD0uO3/NYrN3Lkyx51tMtALGTMFt7d4SNsVwgQ+EGQaM5IJcd/ylx9kgESQBvSjEhJLLiKWukQG2BH+rpOjN2Fq3qU4mqmHpQn6tbNox98Af1aDNnO+Coi652jQxbv4Kh0ot9zJHddK5wuTxIQDLAxyb50f/ReG3UekxZoEOZEtj1oEd+QB/py+hP7Xp010Wfzy82g5Ec3ELjzeNPtijxmO+WExoF5zALIUYd8ClH+ayr2Ab3rD0Dv8VM1Y08npzSs6d5OOAAnG+245koiwkgJoXvZg0EVkcdJxZsMxIGL/OWEl82VnqC6mUQ=="
				}
			}));

		[Test]
		public void ShouldPublishScheduleChangesToConfiguredSubscribers()
		{
			var utcTheTime = new DateTime(2016, 3, 10, 5, 0, 0, DateTimeKind.Utc);
			var newSchedule = new ProjectionChangedEventNew
			{
				IsDefaultScenario = true,
				PersonId = Guid.NewGuid(),
				ScheduleDays =
					new List<ProjectionChangedEventScheduleDay>
					{
						new ProjectionChangedEventScheduleDay {Date = utcTheTime.ToLocalTime()}
					}
			};
			var server = new FakeHttpServer();
			var settingsRepository = new FakeGlobalSettingDataRepository();
			var subscriptions = new ScheduleChangeSubscriptions();
			subscriptions.Add(new ScheduleChangeListener {Uri = new Uri("/", UriKind.Relative)});
			settingsRepository.PersistSettingValue(ScheduleChangeSubscriptions.Key, subscriptions);
			var publisher =
				new ScheduleChangesSubscriptionPublisher(server, new ThisIsNow(utcTheTime), settingsRepository, signatureCreator);
			var target = new ScheduleReadModelWrapperHandler(null, null, null, publisher);
			target.Handle(newSchedule);

			server.Requests.Count.Should().Be.EqualTo(1);
		}


		[Test]
		public void ShouldIgnoreOtherScenariosThanDefault()
		{
			var newSchedule = new ProjectionChangedEventNew
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
			subscriptions.Add(new ScheduleChangeListener {Uri = new Uri("/", UriKind.Relative)});
			settingsRepository.PersistSettingValue(ScheduleChangeSubscriptions.Key, subscriptions);
			var publisher = new ScheduleChangesSubscriptionPublisher(server, new ThisIsNow(new DateTime(2016, 3, 10, 5, 0, 0)),
				settingsRepository, signatureCreator);
			var target = new ScheduleReadModelWrapperHandler(null, null, null, publisher);
			target.Handle(newSchedule);

			server.Requests.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldUseUrlConfiguredForSubscriber()
		{
			var newSchedule = new ProjectionChangedEventNew
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
			subscriptions.Add(new ScheduleChangeListener
			{
				Name = "Facebook",
				RelativeDateRange = new MinMax<int>(-2, 7),
				Uri = new Uri("http://facebook")
			});
			settingsRepository.PersistSettingValue(ScheduleChangeSubscriptions.Key, subscriptions);

			var publisher = new ScheduleChangesSubscriptionPublisher(server, new ThisIsNow(new DateTime(2016, 3, 10, 5, 0, 0)),
				settingsRepository, signatureCreator);
			var target = new ScheduleReadModelWrapperHandler(null, null, null, publisher);
			target.Handle(newSchedule);

			server.Requests[0].Uri.Should().Be.EqualTo("http://facebook/");
		}

		[Test]
		public void ShouldNotNotifySubscribersOutSideInterval()
		{
			var newSchedule = new ProjectionChangedEventNew
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
			subscriptions.Add(new ScheduleChangeListener
			{
				Name = "Facebook",
				RelativeDateRange = new MinMax<int>(4, 7),
				Uri = new Uri("http://facebook")
			});
			subscriptions.Add(new ScheduleChangeListener
			{
				Name = "Salesforce",
				RelativeDateRange = new MinMax<int>(-1, 1),
				Uri = new Uri("http://salesforce")
			});
			settingsRepository.PersistSettingValue(ScheduleChangeSubscriptions.Key, subscriptions);

			var publisher = new ScheduleChangesSubscriptionPublisher(server, new ThisIsNow(new DateTime(2016, 3, 10, 5, 0, 0)),
				settingsRepository, signatureCreator);
			var target = new ScheduleReadModelWrapperHandler(null, null, null, publisher);
			target.Handle(newSchedule);

			server.Requests[0].Uri.Should().Be.EqualTo("http://salesforce/");
		}

		[Test]
		public void ShouldAppendSignatureHeader()
		{
			var newSchedule = new ProjectionChangedEventNew
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
			subscriptions.Add(new ScheduleChangeListener
			{
				Name = "Facebook",
				RelativeDateRange = new MinMax<int>(-1, 1),
				Uri = new Uri("http://facebook")
			});
			settingsRepository.PersistSettingValue(ScheduleChangeSubscriptions.Key, subscriptions);

			var publisher = new ScheduleChangesSubscriptionPublisher(server, new ThisIsNow(new DateTime(2016, 3, 10, 5, 0, 0)),
				settingsRepository, signatureCreator);
			var target = new ScheduleReadModelWrapperHandler(null, null, null, publisher);
			target.Handle(newSchedule);

			server.Requests[0].Headers["Signature"].Length.Should().Be.GreaterThan(0);
		}
	}
}