using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Clipboard
{
	public class SpecialPasteBehaviour
	{
		public IScheduleDay DoPaste(IScheduleDay source)
		{
			removeAbsenceWhichEndEarlierThanDayStart(source);
			removeAbsenceWhichStartLaterThanDayEnd(source);
			reduceAbsenceToDayLength(source);

			return source;
		}

		private void reduceAbsenceToDayLength(IScheduleDay source)
		{
			foreach (var personAbsence in source.PersonAbsenceCollection(true))
			{
				reduceAbsenceAtStart(personAbsence, source);
				reduceAbsenceAtEnd(personAbsence, source);
			}
		}

		private void removeAbsenceWhichStartLaterThanDayEnd(IScheduleDay source)
		{
			IList<IPersonAbsence> personAbsencesToRemove = new List<IPersonAbsence>();

			foreach (var personAbsence in source.PersonAbsenceCollection(true))
			{
				var scheduleDayLocalEndDateTime = source.DateOnlyAsPeriod.Period().EndDateTimeLocal(source.TimeZone);
				var personAbsenceLocalStartDateTime = personAbsence.Period.StartDateTimeLocal(source.TimeZone);
				if(personAbsenceLocalStartDateTime > scheduleDayLocalEndDateTime)
 					personAbsencesToRemove.Add(personAbsence);
			}

			foreach (var personAbsence in personAbsencesToRemove)
			{
				source.Remove(personAbsence);
			}
		}


		private void removeAbsenceWhichEndEarlierThanDayStart(IScheduleDay source)
		{
			IList<IPersonAbsence> personAbsencesToRemove = new List<IPersonAbsence>();

			foreach (var personAbsence in source.PersonAbsenceCollection(true))
			{
				var scheduleDayLocalStartDateTime = source.DateOnlyAsPeriod.Period().StartDateTimeLocal(source.TimeZone);
				var personAbsenceLocalEndDateTime = personAbsence.Period.EndDateTimeLocal(source.TimeZone);
				if (personAbsenceLocalEndDateTime < scheduleDayLocalStartDateTime)
					personAbsencesToRemove.Add(personAbsence);
			}

			foreach (var personAbsence in personAbsencesToRemove)
			{
				source.Remove(personAbsence);
			}
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