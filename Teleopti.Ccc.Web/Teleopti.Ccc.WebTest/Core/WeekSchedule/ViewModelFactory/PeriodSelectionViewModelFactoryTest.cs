using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.ViewModelFactory
{
	 [TestFixture]
	 public class PeriodSelectionViewModelFactoryTest
	 {
		  [Test]
		  public void ShouldSetValuesFromWeekDetailsAndDate()
		  {
			var target = new PeriodSelectionViewModelFactory();

		  	var result = target.CreateModel(new DateOnly(2011, 5, 18));

		  	result.Date.Should().Be.EqualTo(new DateOnly(2011, 5, 18).ToFixedClientDateOnlyFormat());
		  	result.Display.Should().Be.EqualTo("2011-05-16 - 2011-05-22");

		  	result.Navigation.CanPickPeriod.Should().Be.False();
		  	result.Navigation.HasNextPeriod.Should().Be.True();
		  	result.Navigation.HasPrevPeriod.Should().Be.True();
		  	result.Navigation.FirstDateNextPeriod.Should().Be.EqualTo(new DateOnly(2011, 5, 23).ToFixedClientDateOnlyFormat());
		  	result.Navigation.LastDatePreviousPeriod.Should().Be.EqualTo(new DateOnly(2011, 5, 15).ToFixedClientDateOnlyFormat());

			result.SelectedDateRange.MinDate.Should().Be.EqualTo(new DateOnly(2011, 5, 16).ToFixedClientDateOnlyFormat());
			result.SelectedDateRange.MaxDate.Should().Be.EqualTo(new DateOnly(2011, 5, 22).ToFixedClientDateOnlyFormat());

		  	result.SelectableDateRange.MinDate.Should().Be.EqualTo(
				new DateOnly(CultureInfo.CurrentCulture.Calendar.MinSupportedDateTime).ToFixedClientDateOnlyFormat());
		  	result.SelectableDateRange.MaxDate.Should().Be.EqualTo(
				new DateOnly(CultureInfo.CurrentCulture.Calendar.MaxSupportedDateTime).ToFixedClientDateOnlyFormat());

		  }
	 }
}
