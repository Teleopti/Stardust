using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.DomainTest.SiteOpenHours
{
	[TestFixture]
	public class SiteOpenHoursPersisterTest
	{
		private ISiteRepository _siteRepository;
		private ISiteOpenHourRepository _siteOpenHourRepository;
		private ISite _site;

		[SetUp]
		public void SetUp()
		{
			_siteRepository = new FakeSiteRepository();
			_siteOpenHourRepository = new FakeSiteOpenHourRepository();

			prepareData();
		}

		[Test]
		public void ShouldPersistOpenHours()
		{
			var weekDay = DayOfWeek.Friday;
			var timePeriod = new TimePeriod(new TimeSpan(1, 0, 0), new TimeSpan(10, 0, 0));
			var sites = createSiteViewModels(createSiteOpenHourViewModel(weekDay, timePeriod));

			var target = new SiteOpenHoursPersister(_siteRepository, _siteOpenHourRepository);
			var count = target.Persist(sites);

			Assert.IsTrue(count > 0);
			Assert.IsTrue(
				_site.OpenHourCollection.Any(
					openHour =>
						openHour.WeekDay == weekDay && openHour.TimePeriod == timePeriod &&
						openHour.IsClosed == false));
		}

		[Test]
		public void ShouldPersistLackWeekDaysAsClosed()
		{
			var weekDayTimePeriod = new TimePeriod(new TimeSpan(1, 0, 0), new TimeSpan(10, 0, 0));
			var sites =
				createSiteViewModels(createSiteOpenHourViewModel(DayOfWeek.Monday, weekDayTimePeriod));

			var target = new SiteOpenHoursPersister(_siteRepository, _siteOpenHourRepository);
			var count = target.Persist(sites);

			Assert.IsTrue(count > 0);
			Assert.IsTrue(
				_site.OpenHourCollection.Any(
					openHour =>
						openHour.WeekDay == DayOfWeek.Monday && openHour.TimePeriod == weekDayTimePeriod &&
						openHour.IsClosed == false));
			Assert.IsTrue(
				_site.OpenHourCollection.Any(
					openHour =>
						openHour.WeekDay == DayOfWeek.Tuesday && openHour.IsClosed));
		}

		[Test]
		public void ShouldPersistWeekDayAsOpened()
		{
			var openedPeriod = new TimePeriod(8, 0, 17, 0);
			var closedPeriod = new TimePeriod(5, 0, 16, 0);
			var sites = createSiteViewModels(
				createSiteOpenHourViewModel(DayOfWeek.Monday, openedPeriod),
				createSiteOpenHourViewModel(DayOfWeek.Monday, closedPeriod, true)
				);

			var target = new SiteOpenHoursPersister(_siteRepository, _siteOpenHourRepository);
			var count = target.Persist(sites);
			Assert.IsTrue(count > 0);

			var siteOpenHourForMonday = _site.OpenHourCollection.FirstOrDefault(s => s.WeekDay == DayOfWeek.Monday);
			Assert.NotNull(siteOpenHourForMonday);
			Assert.IsTrue(!siteOpenHourForMonday.IsClosed);
			Assert.AreEqual(siteOpenHourForMonday.TimePeriod, openedPeriod);
		}

		[Test]
		public void ShouldPersistClosedForSite()
		{
			var period = new TimePeriod(8, 0, 17, 0);
			var sites = createSiteViewModels(
				createSiteOpenHourViewModel(DayOfWeek.Monday, period, true),
				createSiteOpenHourViewModel(DayOfWeek.Tuesday, period, true),
				createSiteOpenHourViewModel(DayOfWeek.Wednesday, period, true),
				createSiteOpenHourViewModel(DayOfWeek.Thursday, period, true),
				createSiteOpenHourViewModel(DayOfWeek.Friday, period, true),
				createSiteOpenHourViewModel(DayOfWeek.Saturday, period, true),
				createSiteOpenHourViewModel(DayOfWeek.Sunday, period, true)
				);

			var target = new SiteOpenHoursPersister(_siteRepository, _siteOpenHourRepository);
			var count = target.Persist(sites);
			Assert.IsTrue(count > 0);

			Assert.IsTrue(_site.OpenHourCollection.All(o => o.IsClosed));

			var siteOpenHourForMonday = _site.OpenHourCollection.FirstOrDefault(s => s.WeekDay == DayOfWeek.Monday);
			Assert.NotNull(siteOpenHourForMonday);
			Assert.AreEqual(siteOpenHourForMonday.TimePeriod, period);
		}

		[Test]
		public void ShouldPersistWithNextDayPeriod()
		{
			var weekDay = DayOfWeek.Friday;
			var timePeriod = new TimePeriod(new TimeSpan(20, 0, 0), new TimeSpan(34, 0, 0));
			var sites = createSiteViewModels(createSiteOpenHourViewModel(weekDay, timePeriod));

			var target = new SiteOpenHoursPersister(_siteRepository, _siteOpenHourRepository);
			var count = target.Persist(sites);

			Assert.IsTrue(count > 0);
			Assert.IsTrue(
				_site.OpenHourCollection.Any(
					openHour =>
						openHour.WeekDay == weekDay && openHour.TimePeriod == timePeriod &&
						openHour.IsClosed == false));
		}

		private void prepareData()
		{
			_site = new Site("Site to be updated").WithId();
			var siteOpenHour = new SiteOpenHour()
			{
				TimePeriod = new TimePeriod(new TimeSpan(10, 0, 0), new TimeSpan(17, 0, 0)),
				IsClosed = true,
				WeekDay = DayOfWeek.Friday
			};
			_site.AddOpenHour(siteOpenHour);
			_siteOpenHourRepository.Add(siteOpenHour);
			_siteRepository.Add(_site);
		}

		private List<SiteViewModel> createSiteViewModels(params SiteOpenHourViewModel[] siteOpenHourViewModels)
		{
			var siteViewModel = new SiteViewModel {Id = _site.Id.GetValueOrDefault()};
			siteViewModel.OpenHours = siteOpenHourViewModels;
			return new List<SiteViewModel>()
			{
				siteViewModel
			};
		}

		private SiteOpenHourViewModel createSiteOpenHourViewModel(DayOfWeek dayOfWeek, TimePeriod weekDayTimePeriod, bool isClosed = false)
		{
			return new SiteOpenHourViewModel()
			{
				WeekDay = dayOfWeek,
				StartTime = weekDayTimePeriod.StartTime,
				EndTime = weekDayTimePeriod.EndTime,
				IsClosed = isClosed
			};
		}
	}
}
