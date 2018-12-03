using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.DomainTest.SiteOpenHours
{
	[TestFixture]
	public class SiteWithOpenHourProviderTest
	{
		private const string londonSiteName = "London";
		private const string parisSiteName = "Paris";

		private ISiteRepository _siteRepository;
		private FakePermissions _fakeAuthorization;

		private Guid londonSiteId;
		private ISite siteLondon;
		private List<ISiteOpenHour> londonOpenHours;

		private Guid parisSiteId;
		private ISite siteParis;
		private List<ISiteOpenHour> parisOpenHours;

		private SiteWithOpenHourProvider target;

		[SetUp]
		public void SetUp()
		{
			_siteRepository = new FakeSiteRepository();

			var mutableNow = new MutableNow();
			mutableNow.Is("2017-06-12 10:00");

			_fakeAuthorization = new FakePermissions();

			var person = PersonFactory.CreatePersonWithId();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var loggedOnUser = new FakeLoggedOnUser(person);

			target = new SiteWithOpenHourProvider(mutableNow, _siteRepository, _fakeAuthorization, loggedOnUser);

			prepareData();
		}

		[Test]
		public void ShouldGetSitesWithOpenHours()
		{
			_fakeAuthorization.HasPermissionToSite(DefinedRaptorApplicationFunctionPaths.WebRequests, londonSiteId);
			_fakeAuthorization.HasPermissionToSite(DefinedRaptorApplicationFunctionPaths.WebRequests, parisSiteId);

			var result = target.GetSitesWithOpenHour().ToArray();
			Assert.AreEqual(result.Length, 2);

			var londonSite = result[0];
			londonSite.Id.Should().Be(siteLondon.Id);
			londonSite.Name.Should().Be(londonSiteName);

			var openHours = londonSite.OpenHours.ToArray();
			openHours.Length.Should().Be(londonOpenHours.Count);
			isSame(openHours[0], londonOpenHours[0]);
			isSame(openHours[1], londonOpenHours[1]);

			var parisSite = result[1];
			parisSite.Id.Should().Be(siteParis.Id);
			parisSite.Name.Should().Be(parisSiteName);

			openHours = parisSite.OpenHours.ToArray();
			openHours.Length.Should().Be(parisOpenHours.Count);
			isSame(openHours[0], parisOpenHours[0]);
			isSame(openHours[1], parisOpenHours[1]);
		}

		[Test]
		public void ShouldGetSitesWithPermissionOnly()
		{
			_fakeAuthorization.HasPermissionToSite(DefinedRaptorApplicationFunctionPaths.WebRequests, londonSiteId);

			var result = target.GetSitesWithOpenHour().ToArray();
			Assert.AreEqual(result.Length, 1);

			var londonSite = result[0];
			londonSite.Id.Should().Be(siteLondon.Id);
			londonSite.Name.Should().Be(londonSiteName);

			var openHours = londonSite.OpenHours.ToArray();
			openHours.Length.Should().Be(londonOpenHours.Count);
			isSame(openHours[0], londonOpenHours[0]);
			isSame(openHours[1], londonOpenHours[1]);
		}

		[Test]
		public void ShouldNotGetSitesWithoutWebRequestsPermission()
		{
			_fakeAuthorization.HasPermissionToSite(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb, londonSiteId);

			var result = target.GetSitesWithOpenHour().ToArray();
			Assert.AreEqual(result.Length, 0);
		}

		private void prepareData()
		{
			londonSiteId = Guid.NewGuid();
			siteLondon = SiteFactory.CreateSiteWithId(londonSiteId, londonSiteName);

			londonOpenHours = new List<ISiteOpenHour>();

			var siteOpenHour = createSiteOpenHour(siteLondon, DayOfWeek.Friday, TimeSpan.FromHours(10), TimeSpan.FromHours(17));
			londonOpenHours.Add(siteOpenHour);

			siteOpenHour = createSiteOpenHour(siteLondon, DayOfWeek.Tuesday, TimeSpan.FromHours(10), TimeSpan.FromHours(17));
			londonOpenHours.Add(siteOpenHour);

			_siteRepository.Add(siteLondon);

			parisSiteId = Guid.NewGuid();
			siteParis = SiteFactory.CreateSiteWithId(parisSiteId, parisSiteName);

			parisOpenHours = new List<ISiteOpenHour>();

			siteOpenHour = createSiteOpenHour(siteParis, DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(12));
			parisOpenHours.Add(siteOpenHour);

			siteOpenHour = createSiteOpenHour(siteParis, DayOfWeek.Tuesday, TimeSpan.FromHours(13), TimeSpan.FromHours(18));
			parisOpenHours.Add(siteOpenHour);

			_siteRepository.Add(siteParis);
		}

		private ISiteOpenHour createSiteOpenHour(ISite site, DayOfWeek dayOfWeek, TimeSpan start, TimeSpan end,
			bool isClosed = false)
		{
			var siteOpenHour = new SiteOpenHour
			{
				WeekDay = dayOfWeek,
				TimePeriod = new TimePeriod(start, end),
				IsClosed = isClosed
			};
			siteOpenHour.SetId(Guid.NewGuid());
			site.AddOpenHour(siteOpenHour);

			return siteOpenHour;
		}

		private void isSame(SiteOpenHourViewModel openHourVm, ISiteOpenHour openHour)
		{
			Assert.AreEqual(openHour.WeekDay, openHourVm.WeekDay);
			Assert.AreEqual(openHour.TimePeriod.StartTime, openHourVm.StartTime);
			Assert.AreEqual(openHour.TimePeriod.EndTime, openHourVm.EndTime);
			Assert.AreEqual(openHour.IsClosed, openHourVm.IsClosed);
		}
	}
}