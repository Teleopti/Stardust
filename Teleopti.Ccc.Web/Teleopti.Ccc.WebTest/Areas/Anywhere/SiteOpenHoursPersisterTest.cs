using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere
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
			var sites = createSiteViewModels(new Dictionary<DayOfWeek, TimePeriod>
			{
				{weekDay, timePeriod}
			});

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
			var weekDayTimePeriods = new Dictionary<DayOfWeek, TimePeriod>
			{
				{DayOfWeek.Monday, new TimePeriod(new TimeSpan(1, 0, 0), new TimeSpan(10, 0, 0))}
			};
			var sites = createSiteViewModels(weekDayTimePeriods);

			var target = new SiteOpenHoursPersister(_siteRepository, _siteOpenHourRepository);
			var count = target.Persist(sites);

			Assert.IsTrue(count > 0);
			Assert.IsTrue(
				_site.OpenHourCollection.Any(
					openHour =>
						openHour.WeekDay == DayOfWeek.Monday && openHour.TimePeriod == weekDayTimePeriods[DayOfWeek.Monday] &&
						openHour.IsClosed == false));
			Assert.IsTrue(
				_site.OpenHourCollection.Any(
					openHour =>
						openHour.WeekDay == DayOfWeek.Tuesday && openHour.IsClosed));
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

		private List<SiteViewModel> createSiteViewModels(Dictionary<DayOfWeek, TimePeriod> weekDayTimePeriods)
		{
			var siteViewModel = new SiteViewModel {Id = _site.Id.GetValueOrDefault()};
			var openHourList = new List<SiteOpenHourViewModel>();
			foreach (var weekDayTimePeriod in weekDayTimePeriods)
			{
				openHourList.Add(new SiteOpenHourViewModel()
				{
					WeekDay = weekDayTimePeriod.Key,
					StartTime = weekDayTimePeriod.Value.StartTime,
					EndTime = weekDayTimePeriod.Value.EndTime,
					IsClosed = false
				});
			}
			siteViewModel.OpenHours = openHourList;
			return new List<SiteViewModel>()
			{
				siteViewModel
			};
		}
	}
}
