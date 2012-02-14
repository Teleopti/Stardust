using System.Globalization;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class PeriodSelectionViewModelFactory : IPeriodSelectionViewModelFactory
	{
		public PeriodSelectionViewModel CreateModel(DateOnly dateOnly)
		{
			var firstDateInWeek = DateHelper.GetFirstDateInWeek(dateOnly, CultureInfo.CurrentCulture);
			var lastDateInWeek = DateHelper.GetLastDateInWeek(dateOnly, CultureInfo.CurrentCulture);
			var firstDayNextWeek = lastDateInWeek.AddDays(1);
			var lastDayPreviousWeek = firstDateInWeek.AddDays(-1);
			var week = new DateOnlyPeriod(new DateOnly(firstDateInWeek), new DateOnly(lastDateInWeek));
			
			return new PeriodSelectionViewModel
			       	{
			       		Date = dateOnly.ToFixedClientDateOnlyFormat(),
			       		Display = week.DateString,
			       		Navigation =
			       			new PeriodNavigationViewModel
			       				{
			       					CanPickPeriod = false,
			       					HasNextPeriod = true,
			       					HasPrevPeriod = true,
			       					FirstDateNextPeriod = new DateOnly(firstDayNextWeek).ToFixedClientDateOnlyFormat(),
			       					LastDatePreviousPeriod = new DateOnly(lastDayPreviousWeek).ToFixedClientDateOnlyFormat()
			       				},
			       		SelectableDateRange =
			       			new PeriodDateRangeViewModel
			       				{
			       					MinDate = new DateOnly(CultureInfo.CurrentCulture.Calendar.MinSupportedDateTime).ToFixedClientDateOnlyFormat(),
									MaxDate = new DateOnly(CultureInfo.CurrentCulture.Calendar.MaxSupportedDateTime).ToFixedClientDateOnlyFormat()
			       				},
			       		SelectedDateRange =
			       			new PeriodDateRangeViewModel
			       				{
									MinDate = new DateOnly(firstDateInWeek).ToFixedClientDateOnlyFormat(),
									MaxDate = new DateOnly(lastDateInWeek).ToFixedClientDateOnlyFormat(),
			       				}
			       	};
		}
	}
}