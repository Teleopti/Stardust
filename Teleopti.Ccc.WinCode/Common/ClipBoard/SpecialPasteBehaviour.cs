using System;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Clipboard
{
	public class SpecialPasteBehaviour
	{
		public IScheduleDay DoPaste(IScheduleDay source)
		{
			//if (source.PersistableScheduleDataCollection().Any(scheduleData => scheduleData.Id.HasValue))
			//{
			//	throw new ArgumentException("Schedule day is not a copy");
			//}

			foreach (var personAbsence in source.PersonAbsenceCollection(true))
			{
				reduceAbsenceAtStart(personAbsence, source);
				reduceAbsenceAtEnd(personAbsence, source);
			}

			return source;
		}

		private void reduceAbsenceAtStart(IPersonAbsence personAbsence, IScheduleDay source)
		{
			var scheduleDayStartDateTime = source.DateOnlyAsPeriod.Period().StartDateTime;
			var personAbsenceStartDateTime = personAbsence.Period.StartDateTime;

			if (personAbsenceStartDateTime < scheduleDayStartDateTime)
			{
				var newPeriod = personAbsence.Period.ChangeStartTime(scheduleDayStartDateTime.Subtract(personAbsenceStartDateTime));
				personAbsence.ReplaceLayer(personAbsence.Layer.Payload, newPeriod);
			}
		}


		private void reduceAbsenceAtEnd(IPersonAbsence personAbsence, IScheduleDay source)
		{
			var scheduleDayEndDateTime = source.DateOnlyAsPeriod.Period().EndDateTime;
			var personAbsenceEndDateTime = personAbsence.Period.EndDateTime;

			if (personAbsenceEndDateTime > scheduleDayEndDateTime)
			{
				var newPeriod = personAbsence.Period.ChangeEndTime(scheduleDayEndDateTime.Subtract(personAbsenceEndDateTime));
				personAbsence.ReplaceLayer(personAbsence.Layer.Payload, newPeriod);
			}
		}

	}
}