using System.Collections.Generic;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IWorkShiftBackToLegalStateStep
    {
        IScheduleDay ExecuteWeekStep(int weekIndex,IScheduleMatrixPro scheduleMatrix);
		IScheduleDay ExecutePeriodStep(bool raise, IScheduleMatrixPro scheduleMatrix);
    }

    public class WorkShiftBackToLegalStateStep : IWorkShiftBackToLegalStateStep
    {
        private readonly IWorkShiftBackToLegalStateBitArrayCreator _bitArrayCreator;
        private readonly IWorkShiftBackToLegalStateDecisionMaker _decisionMaker;
        private readonly IDeleteSchedulePartService _deleteService;
        private readonly ISchedulePartModifyAndRollbackService _modifyAndRollbackService;
        
        public WorkShiftBackToLegalStateStep(
            IWorkShiftBackToLegalStateBitArrayCreator bitArrayCreator, 
            IWorkShiftBackToLegalStateDecisionMaker decisionMaker, 
            IDeleteSchedulePartService deleteService, 
            ISchedulePartModifyAndRollbackService modifyAndRollbackService 
            )
        {
            _bitArrayCreator = bitArrayCreator;
            _decisionMaker = decisionMaker;
            _deleteService = deleteService;
            _modifyAndRollbackService = modifyAndRollbackService;  
        }

		public IScheduleDay ExecuteWeekStep(int weekIndex, IScheduleMatrixPro scheduleMatrix)
        {
            ILockableBitArray weeklyBitArray = _bitArrayCreator.CreateWeeklyBitArray(weekIndex,  scheduleMatrix);
			int? indexToRemove = _decisionMaker.Execute(weeklyBitArray, false, new DateOnlyPeriod(scheduleMatrix.FullWeeksPeriodDays[0].Day, scheduleMatrix.FullWeeksPeriodDays[scheduleMatrix.FullWeeksPeriodDays.Count - 1].Day));
            if (!indexToRemove.HasValue)
                return null;
            IScheduleDayPro foundDay = scheduleMatrix.FullWeeksPeriodDays[indexToRemove.Value];
			IScheduleDay scheduleDay = (IScheduleDay)foundDay.DaySchedulePart().Clone();
			deleteWorkShift(scheduleDay);
			return scheduleDay;
        }

		public IScheduleDay ExecutePeriodStep(bool raise, IScheduleMatrixPro scheduleMatrix)
        {
            ILockableBitArray periodBitArray = _bitArrayCreator.CreatePeriodBitArray(raise, scheduleMatrix);
			int? indexToRemove = _decisionMaker.Execute(periodBitArray, raise, new DateOnlyPeriod(scheduleMatrix.FullWeeksPeriodDays[0].Day, scheduleMatrix.FullWeeksPeriodDays[scheduleMatrix.FullWeeksPeriodDays.Count - 1].Day));
            if (!indexToRemove.HasValue)
                return null;
            IScheduleDayPro foundDay = scheduleMatrix.FullWeeksPeriodDays[indexToRemove.Value];
			IScheduleDay scheduleDay = (IScheduleDay)foundDay.DaySchedulePart().Clone();
			deleteWorkShift(scheduleDay);
			return scheduleDay;
        }

        private void deleteWorkShift(IScheduleDay foundDay)
        {
            IList<IScheduleDay> deleteList = new List<IScheduleDay> { foundDay };
            _deleteService.Delete(deleteList, _modifyAndRollbackService);
        }
    }
}
