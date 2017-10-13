using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core
{
	[TestFixture]
	public class MonthScheduleDomainDataProviderTest
	{
		private FakeScheduleProvider scheduleProvider;
		private IMonthScheduleDomainDataProvider target;


		[SetUp]
		public void Setup()
		{
			scheduleProvider = new FakeScheduleProvider();
			target = new MonthScheduleDomainDataProvider(scheduleProvider,
				new FakeToggleManager(), MockRepository.GenerateMock<ISeatOccupancyProvider>());
		}

		[Test]
		public void ShouldMapDate()
		{
			var today = DateOnly.Today;

			var result = target.Get(today, true);

			result.CurrentDate.Should().Be.EqualTo(today);
		}


		[Test]
		[SetCulture("sv-SE")]
		public void ShouldMapDays()
		{
			var date = new DateOnly(2014, 1, 11);
			scheduleProvider.AddScheduleDay(ScheduleDayFactory.Create(new DateOnly(2013, 12, 30)));

			var result = target.Get(date, true);

			result.Days.Count().Should().Be(1);
			result.Days.ElementAt(0).ScheduleDay.DateOnlyAsPeriod.DateOnly.Should().Be(new DateOnly(2013, 12, 30));
		}
	}
}