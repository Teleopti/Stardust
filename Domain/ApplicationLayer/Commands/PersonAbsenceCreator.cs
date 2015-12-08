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

		public PersonAbsence Create(IAbsence absence, IScheduleRange scheduleRange, IScheduleDay scheduleDay, DateTimePeriod absenceTimePeriod, IPerson person, TrackedCommandInfo command, bool isFullDayAbsence)
		{
			var businessRulesForPersonAccountUpdate = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);
			var personAbsence = createPersonAbsence(scheduleDay, absence, absenceTimePeriod);
			_saveSchedulePartService.Save(scheduleDay, businessRulesForPersonAccountUpdate, KeepOriginalScheduleTag.Instance);

			if (isFullDayAbsence)
			{
				personAbsence.FullDayAbsence (person, command);
			}
			else
			{
				personAbsence.IntradayAbsence (person, command);
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