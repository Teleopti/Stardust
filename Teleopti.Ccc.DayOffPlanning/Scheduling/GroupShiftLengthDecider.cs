using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning.Scheduling
{
	public interface IGroupShiftLengthDecider
	{
		IList<IShiftProjectionCache> FilterList(IList<IShiftProjectionCache> shiftList, IList<IPerson> persons, 
			ISchedulingOptions schedulingOptions, IScheduleDictionary scheduleDictionary, DateOnly dateOnly);
	}

	public class GroupShiftLengthDecider : IGroupShiftLengthDecider
	{
		private readonly IShiftLengthDecider _shiftLengthDecider;
		private readonly IScheduleMatrixListCreator _scheduleMatrixListCreator;
		private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;

		public GroupShiftLengthDecider(IShiftLengthDecider shiftLengthDecider, IScheduleMatrixListCreator scheduleMatrixListCreator,
			IWorkShiftMinMaxCalculator workShiftMinMaxCalculator)
		{
			_shiftLengthDecider = shiftLengthDecider;
			_scheduleMatrixListCreator = scheduleMatrixListCreator;
			_workShiftMinMaxCalculator = workShiftMinMaxCalculator;
		}

		public IList<IShiftProjectionCache> FilterList(IList<IShiftProjectionCache> shiftList, IList<IPerson> persons, 
			ISchedulingOptions schedulingOptions, IScheduleDictionary scheduleDictionary, DateOnly dateOnly)
		{
			var allShifts = new HashSet<IShiftProjectionCache>();
			foreach (var person in persons)
			{
				var scheduleDay = scheduleDictionary[person].ScheduledDay(dateOnly);
				var matrixList = _scheduleMatrixListCreator.CreateMatrixListFromScheduleParts(
								new List<IScheduleDay> { scheduleDay });
				if (matrixList.Count == 0)
					continue;
				var matrix = matrixList[0];

				var tempShift = _shiftLengthDecider.FilterList(shiftList, _workShiftMinMaxCalculator, matrix, schedulingOptions);
				foreach (var shiftProjectionCach in tempShift)
				{
					allShifts.Add(shiftProjectionCach);
				}
			}
			return allShifts.ToList();
		}
	}
}