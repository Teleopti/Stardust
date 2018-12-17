using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.SystemSettingWeb.BankHolidayCalendar;
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
			var bankHolidayCalendar = new BankHolidayCalendar
			{
				Name = _defaultName,
				Dates = new List<DateTime> { DateTime.Today}
			};

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
		public void ShouldPersistBankHolidayCalendar()
		{
			var bankHolidayCalendar = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(bankHolidayCalendar);

			var repository = new BankHolidayCalendarRepository(CurrUnitOfWork);
			var loadedBankHolidayCalendar = repository.Get(bankHolidayCalendar.Id.GetValueOrDefault());

			loadedBankHolidayCalendar.Name.Should().Be.EqualTo(bankHolidayCalendar.Name);
			loadedBankHolidayCalendar.Dates.First().Should().Be.EqualTo(DateTime.Today);
		}
	}
}
