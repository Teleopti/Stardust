using System;
using System.Collections.Generic;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IList<IShiftProjectionCache> FilterList(IList<IShiftProjectionCache> shiftList, IWorkShiftMinMaxCalculator workShiftMinMaxCalculator, IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions)
		{
			if (shiftList == null) return null;
			bool usingTeamBlockAndSameShift = schedulingOptions.UseTeamBlockPerOption && schedulingOptions.UseTeamBlockSameShift;
			if (schedulingOptions.WorkShiftLengthHintOption != WorkShiftLengthHintOption.AverageWorkTime &&
			    !usingTeamBlockAndSameShift)
				return shiftList;

			if (!schedulingOptions.UseAverageShiftLengths && !usingTeamBlockAndSameShift)
				return shiftList;

			//ta reda på alla skiftlängder i _shiftList, som en lista
			HashSet<TimeSpan> resultingTimes = new HashSet<TimeSpan>();
			foreach (var shiftProjectionCache in shiftList)
			{
				resultingTimes.Add(shiftProjectionCache.WorkShiftProjectionContractTime);
			}
			//hämta önskad skiftlängd
			TimeSpan shiftLength = _desiredShiftLengthCalculator.FindAverageLength(workShiftMinMaxCalculator, matrix,
			                                                                       schedulingOptions);
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
				var resultList = new List<IShiftProjectionCache>();
				var selectedTime = resultingList[selectedIndex];
				foreach (var shiftProjectionCache in shiftList)
				{
					if (shiftProjectionCache.WorkShiftProjectionContractTime == selectedTime)
						resultList.Add(shiftProjectionCache);
				}
				//om ingen träff kör på näst närmaste o.s.v
				if (resultList.Count > 0)
					return resultList;

				resultingList.RemoveAt(selectedIndex);
			}

			return new List<IShiftProjectionCache>();
		}
	}
}