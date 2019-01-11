using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class SiteBankHolidayCalendarRepositoryTest : RepositoryTest<ISiteBankHolidayCalendar>
	{
		private IBankHolidayCalendar _bankHolidayCalendar;
		private ISite _site;

		protected override void ConcreteSetup()
		{
			_site = SiteFactory.CreateSimpleSite("site");
			PersistAndRemoveFromUnitOfWork(_site);

			_bankHolidayCalendar = new BankHolidayCalendar{Name = "ChinaBankHoliday2019"};
			PersistAndRemoveFromUnitOfWork(_bankHolidayCalendar);
		}
		protected override ISiteBankHolidayCalendar CreateAggregateWithCorrectBusinessUnit()
		{
			ISiteBankHolidayCalendar siteBankHolidayCalendar = new SiteBankHolidayCalendar
			{
				Site = _site, BankHolidayCalendarsForSite = new List<IBankHolidayCalendar>{_bankHolidayCalendar}
			};
			return siteBankHolidayCalendar;
		}

		[Test]
		public void VerifyCanLoadSiteBankHolidayCalendars()
		{
			var siteBankHolidayCalendar = new SiteBankHolidayCalendar
			{
				Site = _site, BankHolidayCalendarsForSite = new List<IBankHolidayCalendar>{_bankHolidayCalendar}
			};
			PersistAndRemoveFromUnitOfWork(siteBankHolidayCalendar);
			IEnumerable<ISiteBankHolidayCalendar> loadedSiteBankHolidayCalendars =
				new SiteBankHolidayCalendarRepository(CurrUnitOfWork).FindAllSiteBankHolidayCalendarsSortedBySite();
			Assert.AreEqual(1, loadedSiteBankHolidayCalendars.Count());
		}

		[Test]
		public void VerifyCanLoadSiteBankHolidayCalendarsBySite()
		{
			var siteBankHolidayCalendar = new SiteBankHolidayCalendar
			{
				Site = _site,
				BankHolidayCalendarsForSite = new List<IBankHolidayCalendar> { _bankHolidayCalendar }
			};
			PersistAndRemoveFromUnitOfWork(siteBankHolidayCalendar);
			ISiteBankHolidayCalendar loadedSiteBankHolidayCalendars =
				new SiteBankHolidayCalendarRepository(CurrUnitOfWork).FindSiteBankHolidayCalendar(_site);
			Assert.AreEqual("site", loadedSiteBankHolidayCalendars.Site.Description.Name);
			Assert.AreEqual("ChinaBankHoliday2019", loadedSiteBankHolidayCalendars.BankHolidayCalendarsForSite.First().Name);
		}

		[Test]
		public void VerifyCanLoadSiteBankHolidayCalendarMatchResultByCalendar()
		{
			var siteBankHolidayCalendar = new SiteBankHolidayCalendar
			{
				Site = _site,
				BankHolidayCalendarsForSite = new List<IBankHolidayCalendar> { _bankHolidayCalendar }
			};
			PersistAndRemoveFromUnitOfWork(siteBankHolidayCalendar);

			var result = new SiteBankHolidayCalendarRepository(CurrUnitOfWork).FindSiteBankHolidayCalendars(_bankHolidayCalendar.Id.Value);
			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(_site.Id.Value, result.First().Site.Id.Value);
			Assert.AreEqual(_bankHolidayCalendar.Id.Value, result.First().BankHolidayCalendarsForSite.First().Id.Value);
		}

		[Test]
		public void ShouldUpdateSiteBankHolidayCalendar()
		{
			var bankHolidayCalendar2 = new BankHolidayCalendar { Name = "ChinaBankHoliday2020" };
			PersistAndRemoveFromUnitOfWork(bankHolidayCalendar2);
			var siteBankHolidayCalendar = new SiteBankHolidayCalendar
			{
				Site = _site,
				BankHolidayCalendarsForSite = new List<IBankHolidayCalendar> { _bankHolidayCalendar, bankHolidayCalendar2 }
			};
			PersistAndRemoveFromUnitOfWork(siteBankHolidayCalendar);

			siteBankHolidayCalendar.UpdateBankHolidayCalendarsForSite(new List<IBankHolidayCalendar> { _bankHolidayCalendar });
			PersistAndRemoveFromUnitOfWork(siteBankHolidayCalendar);

			var repository = new SiteBankHolidayCalendarRepository(CurrUnitOfWork);
			var loadedSiteBankHolidayCalendars = repository.FindSiteBankHolidayCalendar(_site);
			Assert.AreEqual("site", loadedSiteBankHolidayCalendars.Site.Description.Name);
			Assert.AreEqual(1, loadedSiteBankHolidayCalendars.BankHolidayCalendarsForSite.Count);
			Assert.AreEqual("ChinaBankHoliday2019", loadedSiteBankHolidayCalendars.BankHolidayCalendarsForSite.First().Name);
		}

		protected override void VerifyAggregateGraphProperties(ISiteBankHolidayCalendar loadedAggregateFromDatabase)
		{
			ISiteBankHolidayCalendar org = CreateAggregateWithCorrectBusinessUnit();
			Assert.AreEqual(org.BankHolidayCalendarsForSite, loadedAggregateFromDatabase.BankHolidayCalendarsForSite);
		}

		protected override Repository<ISiteBankHolidayCalendar> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new SiteBankHolidayCalendarRepository(currentUnitOfWork);
		}
	}
}
