using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
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
		[SetUp]
		public void SetUp()
		{
			_siteRepository = new FakeSiteRepository();
		}
		[Test]
		public void shouldPersistOpenHours()
		{
			var siteToBeUpdated = new Site("Site to be updated").WithId();
			siteToBeUpdated.OpenHours = new Dictionary<DayOfWeek, TimePeriod>()
			{
				{DayOfWeek.Friday, new TimePeriod(new TimeSpan(10, 0, 0), new TimeSpan(17, 0, 0))}
			};
			_siteRepository.Add(siteToBeUpdated);
			var siteViewModel = new SiteViewModel();
			siteViewModel.Id = siteToBeUpdated.Id.GetValueOrDefault();
			var startTime = new TimeSpan(1, 0, 0);
			var endTime = new TimeSpan(10, 0, 0);
			var expectedTimePeriod=new TimePeriod(startTime, endTime);
			var siteOpenHours = new SiteOpenHour()
			{
				WeekDay = DayOfWeek.Friday,
				StartTime = startTime,
				EndTime = endTime
			};
			siteViewModel.OpenHours = new List<SiteOpenHour>()
			{
				siteOpenHours
			};
			var sites =new List<SiteViewModel>()
			{
				siteViewModel
			};
			var target = new SiteOpenHoursPersister(_siteRepository);

			target.Persist(sites);

			siteToBeUpdated.OpenHours[DayOfWeek.Friday].Should().Be.EqualTo(expectedTimePeriod);
		}
	}
}
