using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class PersonAbsenceCreator : IPersonAbsenceCreator
	{
		private readonly ISaveSchedulePartService _saveSchedulePartService;
		private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;

		public PersonAbsenceCreator(ISaveSchedulePartService saveSchedulePartService, IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate)
		{
			_saveSchedulePartService = saveSchedulePartService;
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
		}

		public object Create(AbsenceCreatorInfo info, bool isFullDayAbsence, bool isNeedRuleCheckResult = false)
		{
			var businessRulesForPersonAccountUpdate = _businessRulesForPersonalAccountUpdate.FromScheduleRange(info.ScheduleRange);
			var personAbsence = createPersonAbsence(info.ScheduleDay, info.Absence, info.AbsenceTimePeriod);
			var ruleCheckResult = _saveSchedulePartService.Save(info.ScheduleDay, businessRulesForPersonAccountUpdate, KeepOriginalScheduleTag.Instance, isNeedRuleCheckResult);
			if (isNeedRuleCheckResult) return ruleCheckResult;

			if (isFullDayAbsence)
			{
				personAbsence.FullDayAbsence(info.Person, info.TrackedCommandInfo);
			}
			else
			{
				personAbsence.IntradayAbsence(info.Person, info.TrackedCommandInfo);
			}

			return personAbsence;
		}

		private static PersonAbsence createPersonAbsence(IScheduleDay scheduleDay, IAbsence absence, DateTimePeriod absenceTimePeriod)
		{
			var absenceLayer = new AbsenceLayer(absence, new DateTimePeriod(absenceTimePeriod.StartDateTime, absenceTimePeriod.EndDateTime));
			var personAbsence = scheduleDay.CreateAndAddAbsence(absenceLayer) as PersonAbsence;
			return personAbsence;
		}
	}
}