using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping
{
	public class PreferenceAndScheduleDayViewModelMapper
	{
		private readonly IProjectionProvider _projectionProvider;
		private readonly IUserTimeZone _userTimeZone;
		private readonly PreferenceDayViewModelMapper _preferenceDayModelMapper;

		public PreferenceAndScheduleDayViewModelMapper(IProjectionProvider projectionProvider, IUserTimeZone userTimeZone, PreferenceDayViewModelMapper preferenceDayModelMapper)
		{
			_projectionProvider = projectionProvider;
			_userTimeZone = userTimeZone;
			_preferenceDayModelMapper = preferenceDayModelMapper;
		}

		public PreferenceAndScheduleDayViewModel Map(IScheduleDay s)
		{
			var personRestrictionCollection = s.PersonRestrictionCollection();
			var significantPartForDisplay = s.SignificantPartForDisplay();
			return new PreferenceAndScheduleDayViewModel
			{
				Date = s.DateOnlyAsPeriod.DateOnly.ToFixedClientDateOnlyFormat(),
				Preference =
					personRestrictionCollection == null
						? null
						: _preferenceDayModelMapper.Map(personRestrictionCollection.OfType<IPreferenceDay>().SingleOrDefault()),
				DayOff = s.HasDayOff() ? map(s.PersonAssignment().DayOff()) : null,
				Absence = (significantPartForDisplay == SchedulePartView.FullDayAbsence ||
						   significantPartForDisplay == SchedulePartView.ContractDayOff) &&
						  s.PersonAbsenceCollection() != null
					? mapAbsence(s)
					: null,
				PersonAssignment = significantPartForDisplay == SchedulePartView.MainShift ? mapAssignment(s) : null,
				Feedback = !s.IsScheduled(),
				StyleClassName = styleClassName(s),
				BorderColor = borderColor(s),
				Meetings = meetings(s),
				PersonalShifts = personalShifts(s)
			};
		}

		private PersonAssignmentDayViewModel mapAssignment(IScheduleDay s)
		{
			if (s == null) return null;
			var personAssignment = s.PersonAssignment();

			var visualLayerCollection = _projectionProvider.Projection(s);
			if (visualLayerCollection == null) return null;

			var contractTime = visualLayerCollection.ContractTime();
			return new PersonAssignmentDayViewModel
			{
				ShiftCategory = personAssignment.ShiftCategory?.Description.Name,
				ContractTime =
					TimeHelper.GetLongHourMinuteTimeString(contractTime,
						CultureInfo.CurrentUICulture),
				TimeSpan = personAssignment.PeriodExcludingPersonalActivity().TimePeriod(s.TimeZone).ToShortTimeString(),
				ContractTimeMinutes = (int) contractTime.TotalMinutes
			};
		}

		private AbsenceDayViewModel mapAbsence(IScheduleDay s)
		{
			var personAbsence = s?.PersonAbsenceCollection().FirstOrDefault()?.Layer;
			if (personAbsence == null) return null;

			var visualLayerCollection = _projectionProvider.Projection(s);
			if (visualLayerCollection == null) return null;

			var contractTime = visualLayerCollection.ContractTime();
			return new AbsenceDayViewModel
			{
				Absence = personAbsence.Payload.Description.Name,
				AbsenceContractTime =
					TimeHelper.GetLongHourMinuteTimeString(contractTime,
						CultureInfo.CurrentUICulture),
				AbsenceContractTimeMinutes = (int) contractTime.TotalMinutes
			};
		}

		private DayOffDayViewModel map(IDayOff s)
		{
			return new DayOffDayViewModel {DayOff = s.Description.Name};
		}

		private IList<PersonalShiftViewModel> personalShifts(IScheduleDay s)
		{
			var assignment = s?.PersonAssignment();
			if (assignment != null)
			{
				return (from layer in assignment.PersonalActivities()
					select new PersonalShiftViewModel
					{
						Subject =
							layer.Payload.ConfidentialDescription(assignment.Person).Name,
						TimeSpan =
							ScheduleDayStringVisualizer.ToLocalStartEndTimeString(layer.Period, _userTimeZone.TimeZone(),
								CultureInfo.CurrentCulture)
					}).ToList();
			}
			return null;
		}

		private IList<MeetingViewModel> meetings(IScheduleDay s)
		{
			var meetings = s?.PersonMeetingCollection();
			if (meetings?.Count > 0)
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
		}

		private static string borderColor(IScheduleDay s)
		{
			if (s == null) return null;
			var significantPartForDisplay = s.SignificantPartForDisplay();
			if (significantPartForDisplay == SchedulePartView.ContractDayOff)
			{
				var personAbsenceCollection = s.PersonAbsenceCollection();
				if (personAbsenceCollection.Any())
					return personAbsenceCollection.First().Layer.Payload.DisplayColor.ToHtml();
			}
			if (significantPartForDisplay == SchedulePartView.FullDayAbsence)
				return s.PersonAbsenceCollection().First().Layer.Payload.DisplayColor.ToHtml();
			if (significantPartForDisplay == SchedulePartView.MainShift)
				return s.PersonAssignment().ShiftCategory?.DisplayColor.ToHtml();
			if (significantPartForDisplay == SchedulePartView.DayOff)
				return null;
			return null;
		}

		private static string styleClassName(IScheduleDay s)
		{
			if (s == null) return null;
			var significantPartForDisplay = s.SignificantPartForDisplay();
			if (significantPartForDisplay == SchedulePartView.ContractDayOff)
				return StyleClasses.Striped;
			if (significantPartForDisplay == SchedulePartView.DayOff)
				return StyleClasses.DayOff + " " + StyleClasses.Striped;
			return null;
		}
	}
}