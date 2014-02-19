using System;
using System.Globalization;
using System.Linq;
using System.Threading;
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
				.ForMember(d => d.IsWorkingDay, c => c.ResolveUsing(
					s =>
						{
							var significantPart = s.ScheduleDay.SignificantPartForDisplay();
							return (significantPart == SchedulePartView.MainShift);
						}))
				.ForMember(d => d.DisplayColor, c => c.ResolveUsing(
					s =>
						{
							var significantPart = s.ScheduleDay.SignificantPartForDisplay();
							if (significantPart == SchedulePartView.MainShift)
							{
								return s.ScheduleDay.PersonAssignment().ShiftCategory.DisplayColor.ToHtml();
							}
							return string.Empty;
						}))
				.ForMember(d => d.Absence, c => c.ResolveUsing(s =>
					{
						var absenceCollection = s.ScheduleDay.PersonAbsenceCollection();
						return absenceCollection.Any()
							       ? s.ScheduleDay.PersonAbsenceCollection().First()
							       : null;
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
						var name = personAssignment.ShiftCategory == null ? null : personAssignment.ShiftCategory.Description.Name;
						var color = personAssignment.ShiftCategory == null ? null : personAssignment.ShiftCategory.DisplayColor.ToHtml();
						var contractTime = projection == null ? TimeSpan.Zero : projection.ContractTime();
						return new ShiftViewModel
									{
										Name = name,
										Color = color,
										TimeSpan = personAssignment.Period.TimePeriod(s.ScheduleDay.TimeZone).ToShortTimeString(),
										WorkingHours = TimeHelper.GetLongHourMinuteTimeString(contractTime, CultureInfo.CurrentUICulture)
									};
					}));

			CreateMap<IPersonAbsence, AbsenceViewModel>()
				.ForMember(d => d.Name, c => c.ResolveUsing(
					s => s.Layer.Payload.Description.Name))
				.ForMember(d => d.ShortName, c => c.ResolveUsing(
					s => s.Layer.Payload.Description.ShortName));
		}
	}
}