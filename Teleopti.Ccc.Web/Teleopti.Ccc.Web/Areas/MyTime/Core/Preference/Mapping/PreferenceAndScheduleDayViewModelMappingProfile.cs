using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceAndScheduleDayViewModelMappingProfile : Profile
	{
		private readonly IProjectionProvider _projectionProvider;
		private readonly IUserTimeZone _userTimeZone;

		public PreferenceAndScheduleDayViewModelMappingProfile(IProjectionProvider projectionProvider, IUserTimeZone userTimeZone)
		{
			_projectionProvider = projectionProvider;
			_userTimeZone = userTimeZone;
		}

		protected override void Configure()
		{
			CreateMap<IScheduleDay, PreferenceAndScheduleDayViewModel>()
				.ForMember(d => d.Date, o => o.MapFrom(s => s.DateOnlyAsPeriod.DateOnly.ToFixedClientDateOnlyFormat()))
				.ForMember(s => s.Preference,
				           o =>
				           o.MapFrom(
					           s =>
					           s.PersonRestrictionCollection() == null
						           ? null
						           : s.PersonRestrictionCollection().OfType<IPreferenceDay>().SingleOrDefault()))
				.ForMember(s => s.DayOff,
				           o =>
				           o.MapFrom(s => s.PersonDayOffCollection() == null ? null : s.PersonDayOffCollection().SingleOrDefault()))
				.ForMember(s => s.Absence, o => o.MapFrom(s => (s.SignificantPartForDisplay() == SchedulePartView.FullDayAbsence ||
				                                                s.SignificantPartForDisplay() == SchedulePartView.ContractDayOff) &&
				                                               s.PersonAbsenceCollection() != null
					                                               ? s.PersonAbsenceCollection().First()
					                                               : null))
				.ForMember(s => s.PersonAssignment,
				           o => o.MapFrom(s => s.SignificantPartForDisplay() == SchedulePartView.MainShift ? s : null))

				.ForMember(d => d.Feedback, o => o.MapFrom(s => s == null || !s.IsScheduled()))
				.ForMember(d => d.StyleClassName, o => o.ResolveUsing(s =>
					{
						if (s != null)
						{
							if (s.SignificantPartForDisplay() == SchedulePartView.ContractDayOff)
								return StyleClasses.Striped;
							if (s.SignificantPartForDisplay() == SchedulePartView.DayOff)
								return StyleClasses.DayOff + " " + StyleClasses.Striped;
						}
						return null;
					}))
				.ForMember(d => d.BorderColor, o => o.ResolveUsing(s =>
					{
						if (s != null)
						{
							if ((s.SignificantPartForDisplay() == SchedulePartView.ContractDayOff))
							{
								if (s.PersonAbsenceCollection() != null)
									return s.PersonAbsenceCollection().First().Layer.Payload.DisplayColor.ToHtml();

							}
							if (s.SignificantPartForDisplay() == SchedulePartView.FullDayAbsence)
								return s.PersonAbsenceCollection().First().Layer.Payload.DisplayColor.ToHtml();
							if (s.SignificantPartForDisplay() == SchedulePartView.MainShift)
								return s.PersonAssignment().ShiftCategory.DisplayColor.ToHtml();
							if (s.SignificantPartForDisplay() == SchedulePartView.DayOff)
								return null;
						}
						return null;
					}))
				.ForMember(d => d.Meetings, o => o.ResolveUsing(s =>
					{
						var meetings = s.PersonMeetingCollection();
						if (meetings.Count > 0)
						{
							return meetings.Select(personMeeting => new MeetingViewModel
								{
									Subject = personMeeting.BelongsToMeeting.GetSubject(new NoFormatting()),
									TimeSpan = ScheduleDayStringVisualizer.ToLocalStartEndTimeString(personMeeting.Period,
									                                     _userTimeZone.TimeZone(), CultureInfo.CurrentCulture),
									IsOptional = personMeeting.Optional
								}).ToList();
						}
						return null;
					}))
				.ForMember(d => d.PersonalShifts, o => o.ResolveUsing(s =>
					{
						var assignments = s.PersonAssignmentCollection();
						if (assignments.Count > 0)
						{
							return (from personAssignment in assignments
							        from layer in personAssignment.PersonalLayers()
							        select new PersonalShiftViewModel
								        {
									        Subject =
										        layer.Payload.ConfidentialDescription(personAssignment.Person, s.DateOnlyAsPeriod.DateOnly).Name,
									        TimeSpan =
												ScheduleDayStringVisualizer.ToLocalStartEndTimeString(layer.Period, _userTimeZone.TimeZone(), CultureInfo.CurrentCulture)
								        }).ToList();
						}
						return null;
					}))
				;

			CreateMap<IPersonDayOff, DayOffDayViewModel>()
				.ForMember(d => d.DayOff, o => o.MapFrom(s => s.DayOff.Description.Name))
				;

			CreateMap<IPersonAbsence, AbsenceDayViewModel>()
				.ForMember(s => s.Absence, o => o.MapFrom(s => s.Layer.Payload.Description.Name))
				;

			CreateMap<IScheduleDay, PersonAssignmentDayViewModel>()
				.ForMember(d => d.ShiftCategory, o => o.MapFrom(s => s.PersonAssignment().ShiftCategory.Description.Name))
				.ForMember(d => d.ContractTime, o => o.MapFrom(s => TimeHelper.GetLongHourMinuteTimeString(_projectionProvider.Projection(s).ContractTime(), CultureInfo.CurrentUICulture)))
				.ForMember(d => d.TimeSpan, o => o.MapFrom(s => s.PersonAssignment().Period.TimePeriod(s.TimeZone).ToShortTimeString()))
				.ForMember(d => d.ContractTimeMinutes, o => o.MapFrom(s => _projectionProvider.Projection(s).ContractTime().TotalMinutes));
		}
	}
}