using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class BankHolidayCalendarRepositoryTest : RepositoryTest<IBankHolidayCalendar>
	{
		private string _defaultName = "China Bank Holiday";

		protected override IBankHolidayCalendar CreateAggregateWithCorrectBusinessUnit()
		{
			var calendar = new BankHolidayCalendar();
			calendar.Name = _defaultName;
			return calendar;
		}

		protected override void VerifyAggregateGraphProperties(IBankHolidayCalendar loadedAggregateFromDatabase)
		{
			loadedAggregateFromDatabase.Name.Should().Be.EqualTo(_defaultName);
		}

		protected override Repository<IBankHolidayCalendar> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new BankHolidayCalendarRepository(currentUnitOfWork);
		}

		[Test]
		public void ShouldGetBankHolidayCalendarsByIds()
		{
			var calendar1 = new BankHolidayCalendar{Name = "calendar1"};
			var calendar2 = new BankHolidayCalendar{Name = "calendar2"};
			PersistAndRemoveFromUnitOfWork(calendar1);
			PersistAndRemoveFromUnitOfWork(calendar2);
			var ids = new List<Guid> {calendar1.Id.GetValueOrDefault(), calendar2.Id.GetValueOrDefault()};

			var result = new BankHolidayCalendarRepository(CurrUnitOfWork).FindBankHolidayCalendars(ids).ToList().OrderBy(calendar=> calendar.Name);
			Assert.AreEqual(result.First().Name, "calendar1");
			Assert.AreEqual(result.Last().Name, "calendar2");
		}

		[Test]
		public void ShouldGetEmptyListWhenFindByEmptyIdList()
		{
			var calendar1 = new BankHolidayCalendar { Name = "calendar1" };
			var calendar2 = new BankHolidayCalendar { Name = "calendar2" };
			PersistAndRemoveFromUnitOfWork(calendar1);
			PersistAndRemoveFromUnitOfWork(calendar2);
			var ids = new List<Guid>();

			var result = new BankHolidayCalendarRepository(CurrUnitOfWork).FindBankHolidayCalendars(ids);
			Assert.AreEqual(result.Count, 0);
		}

		[Test]
		public void ShouldGetEmptyListWhenInputNull()
		{
			var calendar1 = new BankHolidayCalendar { Name = "calendar1" };
			var calendar2 = new BankHolidayCalendar { Name = "calendar2" };
			PersistAndRemoveFromUnitOfWork(calendar1);
			PersistAndRemoveFromUnitOfWork(calendar2);

			var result = new BankHolidayCalendarRepository(CurrUnitOfWork).FindBankHolidayCalendars(null);
			Assert.AreEqual(result.Count, 0);
		}
	}
}
