using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping
{
	public class MonthScheduleViewModelMappingProfile : Profile
	{
		private readonly IProjectionProvider _projectionProvider;

		public MonthScheduleViewModelMappingProfile(IProjectionProvider projectionProvider)
		{
			_projectionProvider = projectionProvider;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<MonthScheduleDomainData, MonthScheduleViewModel>()
				.ForMember(d => d.ScheduleDays, c => c.MapFrom(s => s.Days))
				.ForMember(d => d.FixedDate, c => c.MapFrom(s => s.CurrentDate.ToFixedClientDateOnlyFormat()))
				.ForMember(d => d.DayHeaders,
				           c =>
				           c.MapFrom(
					           s =>
					           DateHelper.GetDaysOfWeek(CultureInfo.CurrentCulture)
					                     .Select(
						                     w =>
						                     new Description(CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(w),
						                                     CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(w)))));


			CreateMap<MonthScheduleDayDomainData, MonthDayViewModel>()
				.ForMember(d => d.Date, c => c.MapFrom(s => s.ScheduleDay.DateOnlyAsPeriod.DateOnly))
				.ForMember(d => d.FixedDate,
				           c => c.MapFrom(s => s.ScheduleDay.DateOnlyAsPeriod.DateOnly.ToFixedClientDateOnlyFormat()))
				.ForMember(d => d.Absence, c => c.ResolveUsing(s =>
					{
						var significantPart = s.ScheduleDay.SignificantPartForDisplay();
						var absenceCollection = s.ScheduleDay.PersonAbsenceCollection();
						if (absenceCollection.Any())
						{
							var absence = absenceCollection.OrderBy(a => a.Layer.Payload.Priority).First();
							var name = absence.Layer.Payload.Description.Name;
							var shortName = absence.Layer.Payload.Description.ShortName;
							return new AbsenceViewModel
								{
									Name = name,
									ShortName = shortName,
									IsFullDayAbsence = (significantPart == SchedulePartView.FullDayAbsence)
								};
						}
						return null;
					}))
				.ForMember(d => d.IsDayOff, c => c.ResolveUsing(s =>
					{
						var significantPart = s.ScheduleDay.SignificantPartForDisplay();
						return (significantPart == SchedulePartView.DayOff
						        || significantPart == SchedulePartView.ContractDayOff);
					}))
				.ForMember(d => d.Shift, c => c.ResolveUsing(s =>
				{
					var personAssignment = s.ScheduleDay.PersonAssignment();
					var projection = _projectionProvider.Projection(s.ScheduleDay);

					var isNullPersonAssignment = personAssignment == null;
					var isNullShiftCategoryInfo = isNullPersonAssignment || personAssignment.ShiftCategory == null;
					var name = isNullShiftCategoryInfo ? null : personAssignment.ShiftCategory.Description.Name;
					var shortName = isNullShiftCategoryInfo ? null : personAssignment.ShiftCategory.Description.ShortName;
					var color = isNullShiftCategoryInfo ? null : formatRgbColor(personAssignment.ShiftCategory.DisplayColor);
					var contractTime = projection == null ? TimeSpan.Zero : projection.ContractTime();
					return new ShiftViewModel
					{
						Name = name,
						ShortName = shortName,
						Color = color,
						TimeSpan = isNullPersonAssignment ? string.Empty : personAssignment.Period.TimePeriod(s.ScheduleDay.TimeZone).ToShortTimeString(),
						WorkingHours = TimeHelper.GetLongHourMinuteTimeString(contractTime, CultureInfo.CurrentUICulture)
					};
					}));
		}

		private static string formatRgbColor(Color color)
		{
			return string.Format("rgb({0},{1},{2})", color.R, color.G, color.B);
		}
	}
}