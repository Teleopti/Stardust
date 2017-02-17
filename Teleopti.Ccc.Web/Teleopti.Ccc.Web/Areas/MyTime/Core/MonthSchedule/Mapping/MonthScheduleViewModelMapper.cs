using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping
{
	public class MonthScheduleViewModelMapper
	{
		private readonly IProjectionProvider _projectionProvider;

		public MonthScheduleViewModelMapper(IProjectionProvider projectionProvider)
		{
			_projectionProvider = projectionProvider;
		}

		public MonthScheduleViewModel Map(MonthScheduleDomainData s)
		{
			return new MonthScheduleViewModel
			{
				ScheduleDays = s.Days.Select(map),
				CurrentDate = s.CurrentDate.Date,
				FixedDate = s.CurrentDate.ToFixedClientDateOnlyFormat(),
				DayHeaders = DateHelper.GetDaysOfWeek(CultureInfo.CurrentCulture)
							.Select(
								w =>
									new Description(CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName(w),
										CultureInfo.CurrentUICulture.DateTimeFormat.GetAbbreviatedDayName(w)))
			};
		}

		private MonthDayViewModel map(MonthScheduleDayDomainData s)
		{
			return new MonthDayViewModel
			{
				Date = s.ScheduleDay.DateOnlyAsPeriod.DateOnly.Date,
				FixedDate = s.ScheduleDay.DateOnlyAsPeriod.DateOnly.ToFixedClientDateOnlyFormat(),
				Absence = absence(s),
				HasOvertime =
					s.ScheduleDay != null && s.ScheduleDay.PersonAssignment() != null &&
					s.ScheduleDay.PersonAssignment().ShiftLayers.OfType<OvertimeShiftLayer>().Any(),
				SeatBookings = s.SeatBookingInformation,
				IsDayOff = isDayOff(s),
				Shift = shift(s)
			};
		}
	
		private ShiftViewModel shift(MonthScheduleDayDomainData s)
		{
			var personAssignment = s.ScheduleDay.PersonAssignment();
			var projection = _projectionProvider.Projection(s.ScheduleDay);

			var isNullPersonAssignment = personAssignment == null;
			var isNullShiftCategoryInfo = isNullPersonAssignment || personAssignment.ShiftCategory == null;
			var name = isNullShiftCategoryInfo ? null : personAssignment.ShiftCategory.Description.Name;
			var shortName = isNullShiftCategoryInfo ? null : personAssignment.ShiftCategory.Description.ShortName;
			var color = isNullShiftCategoryInfo ? null : formatRgbColor(personAssignment.ShiftCategory.DisplayColor);
			var contractTime = projection?.ContractTime() ?? TimeSpan.Zero;
			return new ShiftViewModel
			{
				Name = name,
				ShortName = shortName,
				Color = color,
				TimeSpan =
					isNullPersonAssignment
						? string.Empty
						: personAssignment.PeriodExcludingPersonalActivity().TimePeriod(s.ScheduleDay.TimeZone).ToShortTimeString(),
				WorkingHours = TimeHelper.GetLongHourMinuteTimeString(contractTime, CultureInfo.CurrentUICulture)
			};
		}

		private static bool isDayOff(MonthScheduleDayDomainData s)
		{
			var significantPart = s.ScheduleDay.SignificantPartForDisplay();
			return significantPart == SchedulePartView.DayOff
				   || significantPart == SchedulePartView.ContractDayOff;
		}

		private static AbsenceViewModel absence(MonthScheduleDayDomainData s)
		{
			var significantPart = s.ScheduleDay.SignificantPartForDisplay();
			var absenceCollection = s.ScheduleDay.PersonAbsenceCollection();
			if (absenceCollection.Any())
			{
				var absence =
					absenceCollection.OrderBy(a => a.Layer.Payload.Priority)
						.ThenByDescending(a => s.ScheduleDay.PersonAbsenceCollection().IndexOf(a))
						.First();
				var name = absence.Layer.Payload.Description.Name;
				var shortName = absence.Layer.Payload.Description.ShortName;
				return new AbsenceViewModel
				{
					Name = name,
					ShortName = shortName,
					IsFullDayAbsence = significantPart == SchedulePartView.FullDayAbsence
				};
			}
			return null;
		}

		private static string formatRgbColor(Color color)
		{
			return $"rgb({color.R},{color.G},{color.B})";
		}
	}
}