using System;
using System.Collections;
using System.Collections.Generic;
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

		public IList<string> Create(AbsenceCreatorInfo absenceCreatorInfo, bool isFullDayAbsence)
		{
			var businessRulesForPersonAccountUpdate = _businessRulesForPersonalAccountUpdate.FromScheduleRange(absenceCreatorInfo.ScheduleRange);
			var personAbsence = createPersonAbsence(absenceCreatorInfo);
			var ruleCheckResult = _saveSchedulePartService.Save(absenceCreatorInfo.ScheduleDay, businessRulesForPersonAccountUpdate, KeepOriginalScheduleTag.Instance);
			if (ruleCheckResult != null) return ruleCheckResult;

			if (isFullDayAbsence)
			{
				personAbsence.FullDayAbsence(absenceCreatorInfo.Person, absenceCreatorInfo.TrackedCommandInfo);
			}
			else
			{
				personAbsence.IntradayAbsence(absenceCreatorInfo.Person, absenceCreatorInfo.TrackedCommandInfo);
			}

			return null;
		}

		private static PersonAbsence createPersonAbsence(AbsenceCreatorInfo absenceCreatorInfo )
		{
			var absenceLayer = new AbsenceLayer(
				absenceCreatorInfo.Absence, 
				new DateTimePeriod(absenceCreatorInfo.AbsenceTimePeriod.StartDateTime, 
				absenceCreatorInfo.AbsenceTimePeriod.EndDateTime));

			var personAbsence = absenceCreatorInfo.ScheduleDay.CreateAndAddAbsence(absenceLayer, absenceCreatorInfo.PersonRequest) as PersonAbsence;

			return personAbsence;
		}
	}
}