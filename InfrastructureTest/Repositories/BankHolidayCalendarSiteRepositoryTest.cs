﻿using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class BankHolidayCalendarSiteRepositoryTest : RepositoryTest<IBankHolidayCalendarSite>
	{
		private IBankHolidayCalendar _calendar;
		private ISite _site;

		protected override IBankHolidayCalendarSite CreateAggregateWithCorrectBusinessUnit()
		{
			var bankHolidayCalendaSite = new BankHolidayCalendarSite { Calendar = _calendar, Site = _site };
			return bankHolidayCalendaSite;
		}
		protected override void ConcreteSetup()
		{
			_site = SiteFactory.CreateSimpleSite("site");
			PersistAndRemoveFromUnitOfWork(_site);

			_calendar = new BankHolidayCalendar { Name = "calendar" };
			PersistAndRemoveFromUnitOfWork(_calendar);
		}

		protected override void VerifyAggregateGraphProperties(IBankHolidayCalendarSite loadedAggregateFromDatabase)
		{
			loadedAggregateFromDatabase.Calendar.Should().Be.EqualTo(_calendar);
			loadedAggregateFromDatabase.Site.Should().Be.EqualTo(_site);

		}

		protected override Repository<IBankHolidayCalendarSite> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new BankHolidayCalendarSiteRepository(currentUnitOfWork);
		}

		
	}
}
