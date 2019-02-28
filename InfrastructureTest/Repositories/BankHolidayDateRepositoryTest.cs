using NUnit.Framework;
using SharpTestsEx;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class BankHolidayDateRepositoryTest : RepositoryTest<IBankHolidayDate>
	{
		private IBankHolidayCalendar calendar;
		
		protected override void ConcreteSetup()
		{
			calendar = new BankHolidayCalendar { Name = "China Bank Holiday" };
			PersistAndRemoveFromUnitOfWork(calendar);
		}

		protected override IBankHolidayDate CreateAggregateWithCorrectBusinessUnit()
		{
			return new BankHolidayDate { Calendar = calendar, Date = new DateOnly(2001, 1, 1), Description = "Test" };
		}

		protected override void VerifyAggregateGraphProperties(IBankHolidayDate loadedAggregateFromDatabase)
		{
			loadedAggregateFromDatabase.Calendar.Should().Be.EqualTo(calendar);
			loadedAggregateFromDatabase.Description.Should().Be.EqualTo("Test");
			loadedAggregateFromDatabase.Date.Should().Be.EqualTo(new DateOnly(2001, 1, 1));
		}

		protected override Repository<IBankHolidayDate> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return BankHolidayDateRepository.DONT_USE_CTOR(currentUnitOfWork);
		}

		[Test]
		public void ShouldFindOnlyOneBankHolidayDateByDateAndCalendarEventItIsDeleted()
		{
			var bankHolidayDate = CreateAggregateWithCorrectBusinessUnit();
			bankHolidayDate.SetDeleted();
			PersistAndRemoveFromUnitOfWork(bankHolidayDate);

			var repository = BankHolidayDateRepository.DONT_USE_CTOR(CurrUnitOfWork);
			var result = repository.Find(bankHolidayDate.Date, calendar);

			result.Should().Not.Be.EqualTo(null);
			result.IsDeleted.Should().Be.EqualTo(true);
			result.Description.Should().Be.EqualTo("Test");
		}

		[Test]
		public void ShouldFetchByCalendarsAndPeriod()
		{
			var bankHolidayDate = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(bankHolidayDate);
			
			var repository = BankHolidayDateRepository.DONT_USE_CTOR(CurrUnitOfWork);
			var result = repository.FetchByCalendarsAndPeriod(new List<IBankHolidayCalendar> { calendar }, new DateOnlyPeriod(new DateOnly(2000, 12, 31), new DateOnly(2001, 12, 31)));

			result.Count().Should().Be.EqualTo(1);
			result.FirstOrDefault().Should().Be.EqualTo(bankHolidayDate);
		}
	}
}