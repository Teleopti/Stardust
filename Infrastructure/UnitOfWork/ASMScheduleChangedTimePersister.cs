using System;
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
		private static readonly Type[] includedTypes = new[] { typeof(IPersonAbsence), typeof(IPersonAssignment) };
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

			var scheduleDataInDefaultScenario = extractScheduleChangesOnlyInDefaultScenario(modifiedRoots);
			var persistableScheduleDatas = scheduleDataInDefaultScenario as IPersistableScheduleData[] ?? scheduleDataInDefaultScenario.ToArray();
			if (!persistableScheduleDatas.Any()) return;

			addOrUpdateTime(persistableScheduleDatas);
		}

		[MessageBrokerUnitOfWork]
		protected virtual void addOrUpdateTime(IEnumerable<IPersistableScheduleData> persistableScheduleDatas)
		{

			foreach (var scheduleData in persistableScheduleDatas)
			{
				var personId = scheduleData.Person.Id.Value;
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

		private IEnumerable<IPersistableScheduleData> extractScheduleChangesOnlyInDefaultScenario(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			return modifiedRoots.Select(r => r.Root)
				.OfType<IPersistableScheduleData>()
				.Where(s =>
				s.Scenario.DefaultScenario
				&&
				isWithinASMNotifyPeriod(s)
				&& includedTypes.Any(t => s.GetType().GetInterfaces().Contains(t)))
				.ToList();
		}

		private bool isWithinASMNotifyPeriod(IPersistableScheduleData scheduleData)
		{
			var nowInUtc = _now.UtcDateTime();
			var personTimezone = scheduleData.Person.PermissionInformation.DefaultTimeZone();
			var nowInPersonTimezone = TimeZoneHelper.ConvertFromUtc(nowInUtc, personTimezone);
			var expectedPeriodInUtc = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(nowInPersonTimezone.Date.AddDays(-1), personTimezone),
				TimeZoneHelper.ConvertToUtc(nowInPersonTimezone.Date.AddDays(1), personTimezone));
			return scheduleData.Period.StartDateTime <= expectedPeriodInUtc.EndDateTime && scheduleData.Period.EndDateTime >= expectedPeriodInUtc.StartDateTime;
		}
	}
}