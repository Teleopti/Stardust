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
		private DateTime _springFestival = new DateTime(2019, 2, 3);
		private DateTime _newYear = new DateTime(2019,1,1);

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
		public void ShouldLoadAllBankHolidayCalendars()
		{
			var calendar = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(calendar);

			var anotherCalendar = new BankHolidayCalendar("SwdenCalendar");
			anotherCalendar.AddDate(new BankHolidayDate { Date = new DateTime(2020,3,8), Description = "Women Day" });
			PersistAndRemoveFromUnitOfWork(anotherCalendar);

			PersistAndRemoveFromUnitOfWork(calendar.Dates);
			PersistAndRemoveFromUnitOfWork(anotherCalendar.Dates);


			var repository = new BankHolidayCalendarRepository(CurrUnitOfWork);
			var result = repository.LoadAll().OrderBy(c=>c.Name);

			result.Count().Should().Be.EqualTo(2);
			result.First().Name.Should().Be.EqualTo(_defaultName);
			result.Last().Name.Should().Be.EqualTo("SwdenCalendar");
		}

		[Test]
		public void ShouldPersistBankHolidayCalendar()
		{
			var calendar = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(calendar);
			PersistAndRemoveFromUnitOfWork(calendar.Dates);

			var repository = new BankHolidayCalendarRepository(CurrUnitOfWork);
			var result = repository.Load(calendar.Id.Value);

			result.Name.Should().Be.EqualTo(calendar.Name);
			result.Dates.Count.Should().Be.EqualTo(1);
			result.Dates.First().Date.Should().Be.EqualTo(_nationalDay);
			result.Dates.First().Description.Should().Be.EqualTo("National Day");
		}

		[Test]
		public void ShouldPersistBankHolidayCalendarWithUpdatingCalendarName()
		{
			var calendar = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(calendar);

			var newCalendarName = "New Calendar Name";
			calendar.Name = newCalendarName;
			PersistAndRemoveFromUnitOfWork(calendar);

			var repository = new BankHolidayCalendarRepository(CurrUnitOfWork);
			var result = repository.Load(calendar.Id.Value);

			result.Name.Should().Be.EqualTo(newCalendarName);
		}

		[Test]
		public void ShouldPersistBankHolidayCalendarWithAddingCalendarDates()
		{
			var calendar = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(calendar);
			PersistAndRemoveFromUnitOfWork(calendar.Dates);

			var date = new BankHolidayDate { Date = _springFestival, Description = "Spring Festival" };
			calendar.AddDate(date);
			
			PersistAndRemoveFromUnitOfWork(date);

			var repository = new BankHolidayCalendarRepository(CurrUnitOfWork);
			var result = repository.Load(calendar.Id.Value);
			
			result.Dates.Count.Should().Be.EqualTo(2);
			result.Dates.First().Date.Should().Be.EqualTo(_nationalDay);
			result.Dates.First().Description.Should().Be.EqualTo("National Day");
			result.Dates.Last().Date.Should().Be.EqualTo(_springFestival);
			result.Dates.Last().Description.Should().Be.EqualTo("Spring Festival");
		}

		[Test]
		public void ShouldPersistBankHolidayCalendarWithDeletingCalendarDates()
		{
			var calendar = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(calendar);
			PersistAndRemoveFromUnitOfWork(calendar.Dates);

			var date = calendar.Dates.First();
			calendar.DeleteDate(date.Id.Value);
			PersistAndRemoveFromUnitOfWork(date);

			var repository = new BankHolidayCalendarRepository(CurrUnitOfWork);
			var result = repository.Load(calendar.Id.Value);
			
			result.Dates.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldPersistBankHolidayCalendarWithUpdatingCalendarDates()
		{
			var calendar = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(calendar);
			PersistAndRemoveFromUnitOfWork(calendar.Dates);

			var date = calendar.Dates.First();
			date.Date = _newYear;
			date.Description = "New Year";
		
			calendar.UpdateDate(date);
			PersistAndRemoveFromUnitOfWork(date);

			var repository = new BankHolidayCalendarRepository(CurrUnitOfWork);
			var result = repository.Load(calendar.Id.Value);

			result.Dates.Count.Should().Be.EqualTo(1);
			result.Dates.First().Date.Should().Be.EqualTo(_newYear);
			result.Dates.First().Description.Should().Be.EqualTo("New Year");
		}

		[Test]
		public void ShouldPersistBankHolidayCalendarWithAddingUpdatingDeletingCalendarDates()
		{
			var calendar = CreateAggregateWithCorrectBusinessUnit();
			calendar.AddDate(new BankHolidayDate { Date = new DateTime(2080,1,1), Description = "Deleting Day" });
			PersistAndRemoveFromUnitOfWork(calendar);
			PersistAndRemoveFromUnitOfWork(calendar.Dates);

			var dateUpdated = calendar.Dates.First();
			dateUpdated.Date = _newYear;
			dateUpdated.Description = "New Year";
			calendar.UpdateDate(dateUpdated);
			PersistAndRemoveFromUnitOfWork(dateUpdated);

			var dateAdded = new BankHolidayDate { Date = _springFestival, Description = "Spring Festival" };
			calendar.AddDate(dateAdded);
			PersistAndRemoveFromUnitOfWork(dateAdded);

			var dateDeleted = calendar.Dates.ToList().Find(d => d.Description == "Deleting Day");
			calendar.DeleteDate(dateDeleted.Id.Value);
			PersistAndRemoveFromUnitOfWork(dateDeleted);

			var repository = new BankHolidayCalendarRepository(CurrUnitOfWork);
			var result = repository.Load(calendar.Id.Value);

			result.Dates.Count.Should().Be.EqualTo(2);
			result.Dates.First().Date.Should().Be.EqualTo(_newYear);
			result.Dates.First().Description.Should().Be.EqualTo("New Year");
			result.Dates.Last().Date.Should().Be.EqualTo(_springFestival);
			result.Dates.Last().Description.Should().Be.EqualTo("Spring Festival");
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
