using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning.Scheduling
{
	public class ShiftLengthDecider : IShiftLengthDecider
	{
		private readonly IDesiredShiftLengthCalculator _desiredShiftLengthCalculator;

		public ShiftLengthDecider(IDesiredShiftLengthCalculator desiredShiftLengthCalculator)
		{
			_desiredShiftLengthCalculator = desiredShiftLengthCalculator;
		}

		public IList<ShiftProjectionCache> FilterList(IList<ShiftProjectionCache> shiftList, IWorkShiftMinMaxCalculator workShiftMinMaxCalculator, IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions, IDictionary<DateOnly, TimeSpan> maxWorkTimeDictionary)
		{
			if (shiftList == null) return null;
			if (!shiftList.Any()) return shiftList;
			bool usingTeamBlockAndSameShift = schedulingOptions.UseBlock && schedulingOptions.BlockSameShift;
			if (schedulingOptions.WorkShiftLengthHintOption != WorkShiftLengthHintOption.AverageWorkTime &&
			    !usingTeamBlockAndSameShift)
				return shiftList;

			if (!schedulingOptions.UseAverageShiftLengths && !usingTeamBlockAndSameShift)
				return shiftList;

			//ta reda på alla skiftlängder i _shiftList, som en lista
			var contractTimes = shiftList.ToLookup(s => s.WorkShiftProjectionContractTime());
			var resultingTimes = contractTimes.Select(x => x.Key).ToArray();
			
			//hämta önskad skiftlängd
			TimeSpan shiftLength = _desiredShiftLengthCalculator.FindAverageLength(workShiftMinMaxCalculator, matrix, schedulingOptions, maxWorkTimeDictionary);
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