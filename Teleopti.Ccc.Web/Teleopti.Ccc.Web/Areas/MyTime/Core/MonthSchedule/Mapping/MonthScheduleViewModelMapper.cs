using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
				ScheduleDays = s.Days.Select(map).ToArray(),
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
			var overtimes = mapOvertimes(s);
			return new MonthDayViewModel
			{
				Date = s.ScheduleDay.DateOnlyAsPeriod.DateOnly.Date,
				FixedDate = s.ScheduleDay.DateOnlyAsPeriod.DateOnly.ToFixedClientDateOnlyFormat(),
				Absences = mapAbsences(s),
				Overtimes = overtimes,
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

		private static AbsenceViewModel[] mapAbsences(MonthScheduleDayDomainData s)
		{
			var significantPart = s.ScheduleDay.SignificantPartForDisplay();
			var absenceCollection = s.ScheduleDay.PersonAbsenceCollection();
			if (absenceCollection.Any())
			{
				var personAbsences =
					absenceCollection.OrderBy(a => a.Layer.Payload.Priority)
						.ThenByDescending(a => s.ScheduleDay.PersonAbsenceCollection().IndexOf(a));
				return personAbsences.Select(personAbsence =>
				{
					var payload = personAbsence.Layer.Payload;
					return new AbsenceViewModel
					{
						Name = payload.Description.Name,
						ShortName = payload.Description.ShortName,
						Color = formatRgbColor(payload.DisplayColor),
						IsFullDayAbsence = significantPart == SchedulePartView.FullDayAbsence
					};
				}).ToArray();
			}
			return null;
		}

		private static OvertimeViewModel[] mapOvertimes(MonthScheduleDayDomainData s)
		{
			var personAssignment = s.ScheduleDay.PersonAssignment();
			if (personAssignment == null) return null;
			var overtimes = personAssignment.ShiftLayers.OfType<OvertimeShiftLayer>();
			if (!overtimes.Any()) return null;
			return overtimes.OrderBy(overtime => overtime.Period.StartDateTime).Select(overtime =>
			{
				var payload = overtime.Payload;
				return new OvertimeViewModel
				{
					Name = payload.Description.Name,
					ShortName = payload.Description.ShortName,
					Color = formatRgbColor(payload.DisplayColor)
				};
			}).ToArray();
		}

		private static string formatRgbColor(Color color)
		{
			return $"rgb({color.R},{color.G},{color.B})";
		}
	}
}