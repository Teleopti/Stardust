using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class ScheduleMatrixLockableBitArrayConverter : IScheduleMatrixLockableBitArrayConverter
    {
        private readonly IScheduleMatrixPro _matrix;
	    private readonly IScheduleMatrixLockableBitArrayConverterEx _scheduleMatrixLockableBitArrayConverterEx;

	    public ScheduleMatrixLockableBitArrayConverter(IScheduleMatrixPro matrix, IScheduleMatrixLockableBitArrayConverterEx scheduleMatrixLockableBitArrayConverterEx)
        {
	        _matrix = matrix;
	        _scheduleMatrixLockableBitArrayConverterEx = scheduleMatrixLockableBitArrayConverterEx;
        }

	    public ILockableBitArray Convert(bool useWeekBefore, bool useWeekAfter)
	    {
		    var lockableBitArray = _scheduleMatrixLockableBitArrayConverterEx.Convert(_matrix, useWeekBefore, useWeekAfter);

		    return lockableBitArray;
        }

        public IScheduleMatrixPro SourceMatrix
        {
            get { return _matrix; }
        }

        public int Workdays()
        {
            int workdayCount = 0;
            foreach (IScheduleDayPro scheduleDayPro in _matrix.EffectivePeriodDays)
            {
                if (scheduleDayPro.DaySchedulePart().SignificantPart() == SchedulePartView.MainShift)
                    workdayCount++;
            }
            return workdayCount;
        }

        public int DayOffs()
        {
            int count = 0;
            foreach (IScheduleDayPro scheduleDayPro in _matrix.EffectivePeriodDays)
            {
                SchedulePartView significant = scheduleDayPro.DaySchedulePart().SignificantPart();
                if(significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff)
                    count++;
            }
            return count;
        }
    }
}