using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class ASMScheduleChangeTimePersister : ITransactionHook
	{
		private static readonly Type[] includedScheduleTypes = { typeof(IPersonAbsence), typeof(IPersonAssignment), typeof(IMeeting) };
		private readonly IASMScheduleChangeTimeRepository _scheduleChangeTimeRepository;
		private readonly INow _now;

		public ASMScheduleChangeTimePersister(IASMScheduleChangeTimeRepository scheduleChangeTimeRepository,
			INow now)
		{
			_scheduleChangeTimeRepository = scheduleChangeTimeRepository;
			_now = now;
		}
		public void AfterCompletion(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			if (modifiedRoots == null || !modifiedRoots.Any()) return;

			var personDataInDefaultScenario = extractPersonIdsFromScheduleChangesOnlyInDefaultScenario(modifiedRoots);

			if (!personDataInDefaultScenario.Any()) return;

			addOrUpdateTime(personDataInDefaultScenario);
		}

		private void addOrUpdateTime(IEnumerable<Guid> personIds)
		{
			var personIdList = personIds.ToList();
			_scheduleChangeTimeRepository.Save(personIdList.Select(personId => new ASMScheduleChangeTime
			{
				PersonId = personId,
				TimeStamp = _now.UtcDateTime()
			})
			.ToArray());
		}

		private IEnumerable<Guid> extractPersonIdsFromScheduleChangesOnlyInDefaultScenario(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var roots = modifiedRoots.Select(r => r.Root)
				.Where(s => includedScheduleTypes.Any(t => s.GetType().GetInterfaces().Contains(t)))
				.ToList();

			var scheduleData = roots
				.OfType<IPersistableScheduleData>()
				.Where(s => s.Scenario.DefaultScenario && isWithinASMNotifyPeriod(s.Period.StartDateTime, s.Period.EndDateTime, s.Person))
				.Select(s => s.Person.Id.Value).ToList();

			var meetingData = roots
				.OfType<IMeeting>()
				.SelectMany(meeting => isMeetingWithinASMNotifyPeriod(meeting))
				.ToList();

			return scheduleData.Union(meetingData);
		}

		private IEnumerable<Guid> isMeetingWithinASMNotifyPeriod(IMeeting meeting)
		{
			if (!meeting.Scenario.DefaultScenario)
				return Enumerable.Empty<Guid>();

			var meetingDaysInUtc = meeting.MeetingRecurrenceOption.GetMeetingDays(meeting.StartDate, meeting.EndDate)
				.Select(md => new
				{
					StartDateTime = TimeZoneHelper.ConvertToUtc(md.Date.Add(meeting.StartTime), meeting.TimeZone),
					EndDateTime = TimeZoneHelper.ConvertToUtc(md.Date.Add(meeting.EndTime), meeting.TimeZone)
				});

			return meeting.MeetingPersons
				.Where(person =>
				meetingDaysInUtc.Any(md => isWithinASMNotifyPeriod(md.StartDateTime, md.EndDateTime, person.Person)))
				.Select(person => person.Person.Id.GetValueOrDefault());
		}

		private bool isWithinASMNotifyPeriod(DateTime startDateTime, DateTime endDateTime, IPerson person)
		{
			var nowInUtc = _now.UtcDateTime();
			var personTimezone = person.PermissionInformation.DefaultTimeZone();
			var nowInPersonTimezone = TimeZoneHelper.ConvertFromUtc(nowInUtc, personTimezone);
			var nowStartDate = nowInPersonTimezone.Date.AddDays(-1);
			var nowEndDate = nowInPersonTimezone.Date.AddDays(1);
			var userStartDate = TimeZoneHelper.ConvertFromUtc(startDateTime, personTimezone).Date;
			var userEndDate = TimeZoneHelper.ConvertFromUtc(endDateTime, personTimezone).Date;
			return nowStartDate <= userEndDate && nowEndDate >= userStartDate;
		}
	}
}