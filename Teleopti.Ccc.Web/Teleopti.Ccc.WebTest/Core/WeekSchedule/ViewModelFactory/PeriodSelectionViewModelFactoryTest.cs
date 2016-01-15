using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.ViewModelFactory
{
	 [TestFixture, SetCulture("sv-SE")]
	 public class PeriodSelectionViewModelFactoryTest
	 {
		  [Test]
		  public void ShouldSetValuesFromWeekDetailsAndDate()
		  {
			var target = new PeriodSelectionViewModelFactory();

		  	var result = target.CreateModel(new DateOnly(2011, 5, 18));

		  	result.Date.Should().Be.EqualTo(new DateOnly(2011, 5, 18).ToFixedClientDateOnlyFormat());
		  	result.Display.Should().Be.EqualTo("2011-05-16 - 2011-05-22");

		  	result.PeriodNavigation.CanPickPeriod.Should().Be.False();
		  	result.PeriodNavigation.HasNextPeriod.Should().Be.True();
		  	result.PeriodNavigation.HasPrevPeriod.Should().Be.True();
		  	result.PeriodNavigation.NextPeriod.Should().Be.EqualTo(new DateOnly(2011, 5, 23).ToFixedClientDateOnlyFormat());
		  	result.PeriodNavigation.PrevPeriod.Should().Be.EqualTo(new DateOnly(2011, 5, 15).ToFixedClientDateOnlyFormat());

			result.SelectedDateRange.MinDate.Should().Be.EqualTo(new DateOnly(2011, 5, 16).ToFixedClientDateOnlyFormat());
			result.SelectedDateRange.MaxDate.Should().Be.EqualTo(new DateOnly(2011, 5, 22).ToFixedClientDateOnlyFormat());

		  	result.SelectableDateRange.MinDate.Should().Be.EqualTo(DateOnly.MinValue.ToFixedClientDateOnlyFormat());
		  	result.SelectableDateRange.MaxDate.Should().Be.EqualTo(DateOnly.MaxValue.ToFixedClientDateOnlyFormat());

		  }
	 }
}
