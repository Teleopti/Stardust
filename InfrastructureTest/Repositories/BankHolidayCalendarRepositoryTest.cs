using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.SystemSettingWeb;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class BankHolidayCalendarRepositoryTest : RepositoryTest<IBankHolidayCalendar>
	{
		private string _defaultName = "China Bank Holiday";
		private DateTime _nationalDay = new DateTime(2018, 10, 1);

		protected override IBankHolidayCalendar CreateAggregateWithCorrectBusinessUnit()
		{
			var bankHolidayCalendar = new BankHolidayCalendar(_defaultName);
			bankHolidayCalendar.AddDate(new BankHolidayDate { Date = _nationalDay, Description = "National Day" });
			return bankHolidayCalendar;
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
		public void ShouldDeleteBankHolidayDatesWhenDeletingBankHolidayCalendar()
		{
			var calendar = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(calendar);
			PersistAndRemoveFromUnitOfWork(calendar.Dates);

			var Id = calendar.Id.Value;

			var repository = new BankHolidayCalendarRepository(CurrUnitOfWork);

			repository.Delete(Id);

			var result = repository.LoadAll().Where(c=>c.Id.Value == Id);

			result.Count().Should().Be.EqualTo(0);

			var _repository = new BankHolidayDateRepository(CurrUnitOfWork);
			var _result = _repository.LoadAll().Where(d=>d.Calendar.Id== Id);

			_result.Count().Should().Be.EqualTo(0);
		}
	}
}
