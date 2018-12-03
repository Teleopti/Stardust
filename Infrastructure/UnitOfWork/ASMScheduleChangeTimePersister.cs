using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;

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

		private IEnumerable<Guid> extractPersonIdsFromScheduleChangesOnlyInDefaultScenario(IEnumerable<IRootChangeInfo> rootChangeInfos)
		{
			var infos = rootChangeInfos
				.Where(s => includedScheduleTypes.Any(t => s.Root.GetType().GetInterfaces().Contains(t)))
				.ToList();

			var scheduleData = infos
				.Select(r => r.Root)
				.OfType<IPersistableScheduleData>()
				.Where(s => s.Scenario.DefaultScenario && isWithinASMNotifyPeriod(s.Period, s.Person))
				.Select(s => s.Person.Id.Value);

			var meetingData = getPersonDataForMeeting(infos);

			return scheduleData.Union(meetingData);
		}

		private IEnumerable<Guid> getPersonDataForMeeting(IList<IRootChangeInfo> rootChangeInfos)
		{
			var result = new List<Guid>();
			foreach (var info in rootChangeInfos)
			{
				var meeting = info.Root as IMeeting;
				if (meeting == null || !meeting.Scenario.DefaultScenario)
					continue;


				var changes = (info.Root as IProvideCustomChangeInfo).CustomChanges(info.Status);
				foreach (var change in changes)
				{
					var person = (change.Root as IMainReference)?.MainRoot as IPerson;
					var period = (change.Root as IPeriodized)?.Period;
					if (person == null
						|| period == null
						|| !isWithinASMNotifyPeriod(period.Value, person))
						continue;

					result.Add(person.Id.GetValueOrDefault());
				}
			}
			return result.Distinct();
		}

		private bool isWithinASMNotifyPeriod(DateTimePeriod period, IPerson person)
		{
			var nowInUtc = _now.UtcDateTime();
			var personTimezone = person.PermissionInformation.DefaultTimeZone();
			var nowInPersonTimezone = TimeZoneHelper.ConvertFromUtc(nowInUtc, personTimezone);
			var nowStartDate = nowInPersonTimezone.AddHours(-1);
			var nowEndDate = nowStartDate.AddDays(1);
		
			var dateOnlyPeriod = period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());

			var changedInfoStartDateInPersonTimezone = dateOnlyPeriod.StartDate.AddDays(-1).Date;
			var changedInfoEndDateInPersonTimezone = dateOnlyPeriod.EndDate.AddDays(1).Date;
			return nowStartDate < changedInfoEndDateInPersonTimezone && nowEndDate > changedInfoStartDateInPersonTimezone;
		}
	}
}