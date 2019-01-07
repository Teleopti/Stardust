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
	}
}
