using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	public class AbsenceMerger
	{
		private readonly IList<IScheduleDay> pasteList;

		public AbsenceMerger(IList<IScheduleDay> scheduleDays)
		{
			pasteList = scheduleDays;
		}

		public void MergeWithDayBefore()
		{
			for (var i = pasteList.Count - 1; i > 0; i--)
			{
				var scheduleDay = pasteList[i];
				var scheduleDayBefore = pasteList[i - 1];

				if (scheduleDay == null || scheduleDayBefore == null) continue;
				var isFullDay = ((scheduleDay.SignificantPart() == SchedulePartView.FullDayAbsence || scheduleDay.SignificantPart() == SchedulePartView.ContractDayOff));
				var merged = false;

				if (isFullDay)
				{
					if (scheduleDay.Person.Equals(scheduleDayBefore.Person))
					{
						foreach (var data in scheduleDay.PersistableScheduleDataCollection())
						{
							var personAbsence = data as IPersonAbsence;
							if (personAbsence == null) continue;
							if(personAbsence.Id != null) continue;

							foreach (var dataDayBefore in scheduleDayBefore.PersistableScheduleDataCollection())
							{
								var personAbsenceDayBefore = dataDayBefore as IPersonAbsence;
								if (personAbsenceDayBefore == null) continue;
								if(personAbsenceDayBefore.Id != null) continue;

								var personAbsenceMerged = personAbsenceDayBefore.Merge(personAbsence);
								if (personAbsenceMerged != null)
								{
									merged = true;
									scheduleDayBefore.Remove(dataDayBefore);
									scheduleDayBefore.Add(personAbsenceMerged);
								}
							}
						}

						if (merged)
						{
							pasteList.RemoveAt(i);
							RemoveDoubles(scheduleDayBefore);
						}
					}
				}

				foreach (var dataDayBefore in scheduleDayBefore.PersistableScheduleDataCollection())
				{
					var personAbsenceDayBefore = dataDayBefore as IPersonAbsence;
					if (merged || personAbsenceDayBefore == null || !dataDayBefore.Period.Intersect(scheduleDay.Period)) continue;
					if(!scheduleDay.PersistableScheduleDataCollection().Contains(personAbsenceDayBefore))
						scheduleDay.Add(personAbsenceDayBefore);
				}
			}
		}

		public void RemoveDoubles(IScheduleDay scheduleDay)
		{
			var tempList = scheduleDay.PersistableScheduleDataCollection().ToList();
			for (var i = tempList.Count - 1; i > 0; i--)
			{
				var personAbsence = tempList[i] as IPersonAbsence;
				if (personAbsence == null) continue;
				foreach (var data in tempList)
				{
					var tempPersonAbsence = data as IPersonAbsence;
					if (tempPersonAbsence == null) continue;
					if (tempPersonAbsence == personAbsence) continue;

					if (tempPersonAbsence.Period.Contains(personAbsence.Period) && tempPersonAbsence.Layer.Payload == personAbsence.Layer.Payload)
						scheduleDay.Remove(personAbsence);
					break;
				}
			}
		}

		public void MergeOnDayStart()
		{
			foreach (var scheduleDay in pasteList)
			{
				var absenceList = scheduleDay.PersistableScheduleDataCollection().OfType<IPersonAbsence>().ToList();
				bool merge = false;

				for (var i = absenceList.Count - 1; i > 0; i--)
				{
					var personAbsence1 = absenceList[i];
					var personAbsence2 = absenceList[i - 1];

					if (!personAbsence1.Period.Contains(scheduleDay.Period.StartDateTime) && !personAbsence2.Period.Contains(scheduleDay.Period.StartDateTime)) continue;
					var mergedAbsence = personAbsence1.Merge(personAbsence2);
					if (mergedAbsence != null)
					{
						absenceList.RemoveAt(i);
						absenceList.Insert(i, mergedAbsence);
						absenceList.RemoveAt(i - 1);
						merge = true;
					}
				}

				if (!merge) return;

				scheduleDay.Clear<IPersonAbsence>();
				foreach (var personAbsence in absenceList)
				{
					scheduleDay.Add(personAbsence);
				}
			}	
		}
	}
}
