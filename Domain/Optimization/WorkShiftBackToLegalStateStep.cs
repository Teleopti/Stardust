using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public IScheduleDay ExecuteWeekStep(int weekIndex, IScheduleMatrixPro scheduleMatrix, ISchedulePartModifyAndRollbackService rollbackService)
        {
            ILockableBitArray weeklyBitArray = _bitArrayCreator.CreateWeeklyBitArray(weekIndex,  scheduleMatrix);
			int? indexToRemove = _decisionMaker.Execute(weeklyBitArray, false, new DateOnlyPeriod(scheduleMatrix.FullWeeksPeriodDays[0].Day, scheduleMatrix.FullWeeksPeriodDays[scheduleMatrix.FullWeeksPeriodDays.Count - 1].Day));
            if (!indexToRemove.HasValue)
                return null;
            IScheduleDayPro foundDay = scheduleMatrix.FullWeeksPeriodDays[indexToRemove.Value];
			var scheduleDay = (IScheduleDay)foundDay.DaySchedulePart().Clone();
			deleteWorkShift(scheduleDay, rollbackService);
			return scheduleDay;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public IScheduleDay ExecutePeriodStep(bool raise, IScheduleMatrixPro scheduleMatrix, ISchedulePartModifyAndRollbackService rollbackService)
        {
            ILockableBitArray periodBitArray = _bitArrayCreator.CreatePeriodBitArray(raise, scheduleMatrix);
			int? indexToRemove = _decisionMaker.Execute(periodBitArray, raise, new DateOnlyPeriod(scheduleMatrix.FullWeeksPeriodDays[0].Day, scheduleMatrix.FullWeeksPeriodDays[scheduleMatrix.FullWeeksPeriodDays.Count - 1].Day));
            if (!indexToRemove.HasValue)
                return null;
            IScheduleDayPro foundDay = scheduleMatrix.FullWeeksPeriodDays[indexToRemove.Value];
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
