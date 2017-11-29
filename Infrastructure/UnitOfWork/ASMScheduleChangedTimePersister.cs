﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class ASMScheduleChangedTimePersister : ITransactionHook
	{
		private static readonly Type[] includedScheduleTypes = { typeof(IPersonAbsence), typeof(IPersonAssignment), typeof(IMeeting) };
		private readonly IASMScheduleChangeTimeRepository _scheduleChangeTimeRepository;
		private readonly INow _now;
		public ASMScheduleChangedTimePersister(IASMScheduleChangeTimeRepository scheduleChangeTimeRepository, INow now)
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

		[MessageBrokerUnitOfWork]
		protected virtual void addOrUpdateTime(IEnumerable<Guid> personIds)
		{
			var personIdList = personIds.ToList();
			foreach (var personId in personIdList)
			{
				var time = _scheduleChangeTimeRepository.GetScheduleChangeTime(personId);
				if (time == null)
				{
					_scheduleChangeTimeRepository.Add(new ASMScheduleChangeTime
					{
						PersonId = personId,
						TimeStamp = _now.UtcDateTime()
					});
				}
				else
				{
					time.TimeStamp = _now.UtcDateTime();
					_scheduleChangeTimeRepository.Update(time);
				}

			}
		}

		private IEnumerable<Guid> extractPersonIdsFromScheduleChangesOnlyInDefaultScenario(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var roots = modifiedRoots.Select(r => r.Root)
				.Where(s => includedScheduleTypes.Any(t => s.GetType().GetInterfaces().Contains(t)))
				.ToList();

			var scheduleData = roots.OfType<IPersistableScheduleData>()
				.Where(s => s.Scenario.DefaultScenario && isWithinASMNotifyPeriod(s.Period.StartDateTime, s.Period.EndDateTime, s.Person))
				.Select(s => s.Person.Id.Value).ToList();

			var meetingData = roots.OfType<IMeeting>()
				.Where(m => m.Scenario.DefaultScenario)
				.SelectMany(meeting => meeting.MeetingPersons
					.Where(p => isWithinASMNotifyPeriod(meeting.StartDate.Date.Add(meeting.StartTime), meeting.EndDate.Date.Add(meeting.EndTime), p.Person))
					.Select(p => p.Person.Id.Value)).ToList();

			return scheduleData.Union(meetingData);
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