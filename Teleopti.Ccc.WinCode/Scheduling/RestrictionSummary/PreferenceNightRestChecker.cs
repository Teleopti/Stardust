using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary
{
	public interface IPreferenceNightRestChecker
	{
		void CheckNightlyRest(Dictionary<int, IPreferenceCellData> preferenceCellDataDictionary);
	}

	public class PreferenceNightRestChecker : IPreferenceNightRestChecker
	{

		public void CheckNightlyRest(Dictionary<int, IPreferenceCellData> preferenceCellDataDictionary)
		{
			if(preferenceCellDataDictionary == null) return;

			IList<IPreferenceCellData> datas = new List<IPreferenceCellData>(preferenceCellDataDictionary.Values);
			var lastIndex = datas.Count - 1;
			foreach (var preferenceCellData in datas)
			{
				var nextIndex = datas.IndexOf(preferenceCellData) + 1;
				if (nextIndex > lastIndex) return;
                checkNightlyRest(preferenceCellData, datas[nextIndex]);
			}
		}

        private static void checkNightlyRest(IPreferenceCellData dayOne, IPreferenceCellData dayTwo)
        {
			if (dayOne == null || dayTwo == null) return;
            TimeSpan nightlyRest = dayOne.NightlyRest;
            
            if (dayOne.HasDayOff || dayTwo.HasDayOff) return;
            if (dayOne.EffectiveRestriction != null && dayOne.EffectiveRestriction.DayOffTemplate != null) return;
            if (dayTwo.EffectiveRestriction != null && dayTwo.EffectiveRestriction.DayOffTemplate != null) return;
            if (dayOne.EffectiveRestriction != null && dayOne.EffectiveRestriction.Absence != null) return;
            if (dayTwo.EffectiveRestriction != null && dayTwo.EffectiveRestriction.Absence != null) return;

            var earliestEndDayOne = dayOne.TheDate.Date;
			if (dayOne.EffectiveRestriction != null && dayOne.EffectiveRestriction.EndTimeLimitation.StartTime.HasValue)
                earliestEndDayOne = earliestEndDayOne.Add(dayOne.EffectiveRestriction.EndTimeLimitation.StartTime.Value);

            var latestStartDayTwo = dayTwo.TheDate.Date;
			if (dayTwo.EffectiveRestriction != null && dayTwo.EffectiveRestriction.StartTimeLimitation.EndTime.HasValue)
                latestStartDayTwo = latestStartDayTwo.Add(dayTwo.EffectiveRestriction.StartTimeLimitation.EndTime.Value);
            else
                latestStartDayTwo = latestStartDayTwo.AddDays(1);

            if (latestStartDayTwo - earliestEndDayOne < nightlyRest)
            {
                dayOne.ViolatesNightlyRest = true;
                dayTwo.ViolatesNightlyRest = true;
            }
        }
	}
}