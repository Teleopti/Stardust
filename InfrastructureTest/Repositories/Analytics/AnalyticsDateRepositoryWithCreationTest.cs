using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsDatabaseTest]
	[Toggle(Toggles.ETL_EventbasedDate_39562)]
	public class AnalyticsDateRepositoryWithCreationTest : ISetup
	{
		public IAnalyticsDateRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		private FakeEventPublisher _fakeEventPublisher;


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			_fakeEventPublisher = new FakeEventPublisher();
			system.UseTestDouble(_fakeEventPublisher).For<IEventPublisher>();
		}

		[SetUp]
		public void Setup()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(new EternityAndNotDefinedDate());
			analyticsDataFactory.Persist();
		}

		[Test]
		public void ShouldCreateMissingDatesWhenLoading()
		{
			var targetDate = new DateTime(2000, 01, 05);
			var date = WithAnalyticsUnitOfWork.Get(() => Target.Date(targetDate));
			date.DateDate.Date.Should().Be.EqualTo(targetDate.Date);
			date.DateId.Should().Be.GreaterThan(0);

			_fakeEventPublisher.PublishedEvents.Should().Not.Be.Empty();
			_fakeEventPublisher.PublishedEvents.OfType<AnalyticsDatesChangedEvent>().SingleOrDefault().Should().Not.Be.Null();
		}

		[LogTime]
		[Test]
		public void ShouldCreateManyMissingDatesWhenLoading()
		{
			var targetDate = new DateTime(2010, 01, 05);
			var date = WithAnalyticsUnitOfWork.Get(() => Target.Date(targetDate));
			date.DateDate.Date.Should().Be.EqualTo(targetDate.Date);

			_fakeEventPublisher.PublishedEvents.Should().Not.Be.Empty();
			_fakeEventPublisher.PublishedEvents.OfType<AnalyticsDatesChangedEvent>().SingleOrDefault().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldOnlyPublishOnceEvenIfMultipleCreationsHappenInSameUoW()
		{
			var targetDate = new DateTime(2000, 01, 05);
			IAnalyticsDate date = null;
			WithAnalyticsUnitOfWork.Do(() =>
			{
				date = Target.Date(targetDate-TimeSpan.FromDays(1));
				date = Target.Date(targetDate);
			});
			date.DateDate.Date.Should().Be.EqualTo(targetDate.Date);
			date.DateId.Should().Be.GreaterThan(0);

			_fakeEventPublisher.PublishedEvents.Should().Not.Be.Empty();
			_fakeEventPublisher.PublishedEvents.OfType<AnalyticsDatesChangedEvent>().SingleOrDefault().Should().Not.Be.Null();
		}

		[Test]
		public async void ShouldHandleMultipleRequestsAtTheSameTime()
		{
			var tasks = new List<Task>();

			for (var i = 0; i < 10; i++)
			{
				tasks.Add(new Task(() =>
				{
					var targetDate = new DateTime(2000, 01, 05);
					IAnalyticsDate date = null;
					WithAnalyticsUnitOfWork.Do(() =>
					{
						date = Target.Date(targetDate);
					});
					date.DateDate.Date.Should().Be.EqualTo(targetDate.Date);
					date.DateId.Should().Be.GreaterThan(0);
				}));
			}
			foreach(var task in tasks)
				task.Start();

			await Task.WhenAll(tasks.ToArray());

			_fakeEventPublisher.PublishedEvents.Should().Not.Be.Empty();
			_fakeEventPublisher.PublishedEvents.OfType<AnalyticsDatesChangedEvent>().SingleOrDefault().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldLoadEternityAsMaxDate()
		{
			var targetDate = new DateTime(2000, 01, 05);
			WithAnalyticsUnitOfWork.Get(() => Target.Date(targetDate));
			_fakeEventPublisher.Clear();

			var date = WithAnalyticsUnitOfWork.Get(() => Target.MaxDate());
			date.DateDate.Date.Should().Be.EqualTo(AnalyticsDate.Eternity.DateDate);
			date.DateId.Should().Be.EqualTo(AnalyticsDate.Eternity.DateId);

			_fakeEventPublisher.PublishedEvents.Should().Be.Empty();
		}

		[Test]
		public void ShouldLoadMinDate()
		{
			var targetDate = new DateTime(2000, 01, 05);
			WithAnalyticsUnitOfWork.Get(() => Target.Date(targetDate));
			_fakeEventPublisher.Clear();

			var date = WithAnalyticsUnitOfWork.Get(() => Target.MinDate());
			date.DateDate.Should().Be.EqualTo(new DateTime(1999, 12, 31));
			date.DateId.Should().Be.GreaterThan(0);

			_fakeEventPublisher.PublishedEvents.Should().Be.Empty();
		}
	}
}