using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.ViewModelFactory
{
	[TestFixture]
	public class ScheduleViewModelTestDataFactoryTest
	{
		// NCover...
		private IScheduleViewModelFactory target;

		[SetUp]
		public void Setup()
		{
			target = new ScheduleViewModelTestDataFactory();
		}

		[Test]
		public void ShouldCreateModelForWeekScheduleWithSevenDays()
		{
			var result = target.CreateWeekViewModel(DateOnly.Today);

			result.Days.Count().Should().Be.EqualTo(7);
		}
	}
}
