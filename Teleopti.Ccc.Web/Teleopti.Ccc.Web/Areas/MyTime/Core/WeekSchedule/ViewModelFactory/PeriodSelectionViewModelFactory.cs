using System.Globalization;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class PeriodSelectionViewModelFactory : IPeriodSelectionViewModelFactory
	{
		public PeriodSelectionViewModel CreateModel(DateOnly dateOnly)
		{
			var firstDateInWeek = DateHelper.GetFirstDateInWeek(dateOnly, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
			var lastDateInWeek = firstDateInWeek.AddDays(6);
			var firstDayNextWeek = lastDateInWeek.AddDays(1);
			var lastDayPreviousWeek = firstDateInWeek.AddDays(-1);
			var week = new DateOnlyPeriod(firstDateInWeek, lastDateInWeek);
			
			return new PeriodSelectionViewModel
			       	{
			       		Date = dateOnly.ToFixedClientDateOnlyFormat(),
			       		Display = week.DateString,
			       		PeriodNavigation =
			       			new PeriodNavigationViewModel
			       				{
			       					CanPickPeriod = false,
			       					HasNextPeriod = true,
			       					HasPrevPeriod = true,
			       					NextPeriod = firstDayNextWeek.ToFixedClientDateOnlyFormat(),
			       					PrevPeriod = lastDayPreviousWeek.ToFixedClientDateOnlyFormat()
			       				},
			       		SelectableDateRange =
			       			new PeriodDateRangeViewModel
			       				{
			       					MinDate = DateOnly.MinValue.ToFixedClientDateOnlyFormat(),
									MaxDate = DateOnly.MaxValue.ToFixedClientDateOnlyFormat()
			       				},
			       		SelectedDateRange =
			       			new PeriodDateRangeViewModel
			       				{
									MinDate = firstDateInWeek.ToFixedClientDateOnlyFormat(),
									MaxDate = lastDateInWeek.ToFixedClientDateOnlyFormat(),
			       				}
			       	};
		}
	}
}