using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.DayOffPlanning.Scheduling
{
	public class ShiftLengthDecider
	{
		private readonly IDesiredShiftLengthCalculator _desiredShiftLengthCalculator;

		public ShiftLengthDecider(IDesiredShiftLengthCalculator desiredShiftLengthCalculator)
		{
			_desiredShiftLengthCalculator = desiredShiftLengthCalculator;
		}

		[RemoveMeWithToggle("remove null check on openhoursskillresult", Toggles.ResourcePlanner_ConsiderOpenHoursWhenDecidingPossibleWorkTimes_76118)]
		public IList<ShiftProjectionCache> FilterList(IList<ShiftProjectionCache> shiftList, IWorkShiftMinMaxCalculator workShiftMinMaxCalculator, IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions, OpenHoursSkillResult openHoursSkillResult, DateOnly date)
		{
			if (shiftList == null) return null;
			if (!shiftList.Any()) return shiftList;
			bool usingTeamBlockAndSameShift = schedulingOptions.UseBlock && schedulingOptions.BlockSameShift;
			if (schedulingOptions.WorkShiftLengthHintOption != WorkShiftLengthHintOption.AverageWorkTime &&
			    !usingTeamBlockAndSameShift)
				return shiftList;

			if (!schedulingOptions.UseAverageShiftLengths && !usingTeamBlockAndSameShift)
				return shiftList;

			//PERFORMANCE TEST NEW
			ILookup<TimeSpan, ShiftProjectionCache> contractTimes;
			if (openHoursSkillResult != null)
			{
				var currentTime = openHoursSkillResult.ForCurrentDate(date);
				contractTimes = shiftList.Where(s => s.WorkShiftProjectionPeriod().ElapsedTime() <= currentTime).ToLookup(s => s.WorkShiftProjectionContractTime());
			}
			else
			{
				contractTimes = shiftList.ToLookup(s => s.WorkShiftProjectionContractTime());
			}

			var resultingTimes = contractTimes.Select(x => x.Key).ToArray();

			//hämta önskad skiftlängd
			var shiftLength = _desiredShiftLengthCalculator.FindAverageLength(workShiftMinMaxCalculator, matrix, schedulingOptions, openHoursSkillResult);

			//välj närmaste från listan
			IList<TimeSpan> resultingList = new List<TimeSpan>(resultingTimes);
			while (resultingList.Count > 0)
			{
				double minDiffTime = double.MaxValue;
				int selectedIndex = 0;
				for (int i = 0; i < resultingList.Count; i++)
				{
					TimeSpan time = resultingList[i];
					if (Math.Abs(shiftLength.TotalSeconds - time.TotalSeconds) < minDiffTime)
					{
						minDiffTime = Math.Abs(shiftLength.TotalSeconds - time.TotalSeconds);
						selectedIndex = i;
					}
				}
				//filtrera på den
				var selectedTime = resultingList[selectedIndex];
				var resultList = contractTimes[selectedTime].ToList();

				//om ingen träff kör på näst närmaste o.s.v
				if (resultList.Count > 0)
					return resultList;

				resultingList.RemoveAt(selectedIndex);
			}

			return new List<ShiftProjectionCache>();
		}
	}
}