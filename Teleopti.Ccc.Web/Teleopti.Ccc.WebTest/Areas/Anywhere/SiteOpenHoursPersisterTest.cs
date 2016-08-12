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
		[SetUp]
		public void SetUp()
		{
			_siteRepository = new FakeSiteRepository();
			_siteOpenHourRepository = new FakeSiteOpenHourRepository();
		}
		[Test]
		public void shouldPersistOpenHours()
		{
			var siteToBeUpdated = new Site("Site to be updated").WithId();
			var siteOpenHour = new SiteOpenHour()
			{
				TimePeriod = new TimePeriod(new TimeSpan(10, 0, 0), new TimeSpan(17, 0, 0)),
				IsClosed = true,
				WeekDay = DayOfWeek.Friday
			};
			siteToBeUpdated.AddOpenHour(siteOpenHour);
			_siteOpenHourRepository.Add(siteOpenHour);
			_siteRepository.Add(siteToBeUpdated);
			var siteViewModel = new SiteViewModel();
			siteViewModel.Id = siteToBeUpdated.Id.GetValueOrDefault();
			var startTime = new TimeSpan(1, 0, 0);
			var endTime = new TimeSpan(10, 0, 0);
			var expectedSiteOpenHour = new SiteOpenHour()
			{
				Parent = siteToBeUpdated,
				WeekDay = DayOfWeek.Friday,
				TimePeriod = new TimePeriod(startTime, endTime),
				IsClosed = false
			};
			var siteOpenHours = new SiteOpenHourViewModel()
			{
				WeekDay = DayOfWeek.Friday,
				StartTime = startTime,
				EndTime = endTime,
				IsClosed = false
			};
			siteViewModel.OpenHours = new List<SiteOpenHourViewModel>()
			{
				siteOpenHours
			};
			var sites = new List<SiteViewModel>()
			{
				siteViewModel
			};
			var target = new SiteOpenHoursPersister(_siteRepository, _siteOpenHourRepository);

			target.Persist(sites);
			
			Assert.IsTrue(siteToBeUpdated.OpenHourCollection.Any(openHour=>openHour.WeekDay== expectedSiteOpenHour.WeekDay&& openHour.TimePeriod== expectedSiteOpenHour.TimePeriod&& openHour.IsClosed== expectedSiteOpenHour.IsClosed));
			Assert.IsTrue(siteToBeUpdated.OpenHourCollection.Count()==1);
		}
	}
}
