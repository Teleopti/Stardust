using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.SystemSettingWeb;
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
			calendar = new BankHolidayCalendar {Name = "China Bank Holiday"};
			PersistAndRemoveFromUnitOfWork(calendar);
		}

		protected override IBankHolidayDate CreateAggregateWithCorrectBusinessUnit()
		{

			return new BankHolidayDate{Calendar = calendar, Date = new DateOnly(2001,1,1), Description = "Test"};
		}

		protected override void VerifyAggregateGraphProperties(IBankHolidayDate loadedAggregateFromDatabase)
		{
			loadedAggregateFromDatabase.Calendar.Should().Be.EqualTo(calendar);
			loadedAggregateFromDatabase.Description.Should().Be.EqualTo("Test");
			loadedAggregateFromDatabase.Date.Should().Be.EqualTo(new DateOnly(2001,1,1));
		}

		protected override Repository<IBankHolidayDate> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new BankHolidayDateRepository(currentUnitOfWork);
		}
	}
}