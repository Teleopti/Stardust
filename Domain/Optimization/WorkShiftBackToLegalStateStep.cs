using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IWorkShiftBackToLegalStateStep
    {
		IScheduleDay ExecuteWeekStep(int weekIndex, IScheduleMatrixPro scheduleMatrix, ISchedulePartModifyAndRollbackService rollbackService);
		IScheduleDay ExecutePeriodStep(bool raise, IScheduleMatrixPro scheduleMatrix, ISchedulePartModifyAndRollbackService rollbackService);
    }

    public class WorkShiftBackToLegalStateStep : IWorkShiftBackToLegalStateStep
    {
        private readonly IWorkShiftBackToLegalStateBitArrayCreator _bitArrayCreator;
        private readonly IWorkShiftBackToLegalStateDecisionMaker _decisionMaker;
        private readonly IDeleteSchedulePartService _deleteService;
        
        public WorkShiftBackToLegalStateStep(
            IWorkShiftBackToLegalStateBitArrayCreator bitArrayCreator, 
            IWorkShiftBackToLegalStateDecisionMaker decisionMaker, 
            IDeleteSchedulePartService deleteService)
        {
            _bitArrayCreator = bitArrayCreator;
            _decisionMaker = decisionMaker;
            _deleteService = deleteService;
        }

		public IScheduleDay ExecuteWeekStep(int weekIndex, IScheduleMatrixPro scheduleMatrix, ISchedulePartModifyAndRollbackService rollbackService)
        {
            ILockableBitArray weeklyBitArray = _bitArrayCreator.CreateWeeklyBitArray(weekIndex,  scheduleMatrix);
	        var scheduleMatrixFullWeeksPeriodDays = scheduleMatrix.FullWeeksPeriodDays;
	        int? indexToRemove = _decisionMaker.Execute(weeklyBitArray, false, new DateOnlyPeriod(scheduleMatrixFullWeeksPeriodDays[0].Day, scheduleMatrixFullWeeksPeriodDays[scheduleMatrixFullWeeksPeriodDays.Length - 1].Day));
            if (!indexToRemove.HasValue)
                return null;
            IScheduleDayPro foundDay = scheduleMatrixFullWeeksPeriodDays[indexToRemove.Value];
			var scheduleDay = (IScheduleDay)foundDay.DaySchedulePart().Clone();
			deleteWorkShift(scheduleDay, rollbackService);
			return scheduleDay;
        }

		public IScheduleDay ExecutePeriodStep(bool raise, IScheduleMatrixPro scheduleMatrix, ISchedulePartModifyAndRollbackService rollbackService)
        {
            ILockableBitArray periodBitArray = _bitArrayCreator.CreatePeriodBitArray(scheduleMatrix);
	        var scheduleMatrixFullWeeksPeriodDays = scheduleMatrix.FullWeeksPeriodDays;
	        int? indexToRemove = _decisionMaker.Execute(periodBitArray, raise, new DateOnlyPeriod(scheduleMatrixFullWeeksPeriodDays[0].Day, scheduleMatrixFullWeeksPeriodDays[scheduleMatrixFullWeeksPeriodDays.Length - 1].Day));
            if (!indexToRemove.HasValue)
                return null;
            IScheduleDayPro foundDay = scheduleMatrixFullWeeksPeriodDays[indexToRemove.Value];
			var scheduleDay = (IScheduleDay)foundDay.DaySchedulePart().Clone();
			deleteWorkShift(scheduleDay, rollbackService);
			return scheduleDay;
        }

        private void deleteWorkShift(IScheduleDay foundDay, ISchedulePartModifyAndRollbackService rollbackService)
        {
            IList<IScheduleDay> deleteList = new List<IScheduleDay> { foundDay };
            _deleteService.Delete(deleteList, rollbackService);
        }
    }
}
