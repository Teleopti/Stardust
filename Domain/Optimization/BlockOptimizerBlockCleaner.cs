using System.Collections.Generic;
using System.ComponentModel;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IBlockOptimizerBlockCleaner
    {
        IList<DateOnly> ClearSchedules(IScheduleMatrixPro matrix, IList<DateOnly> dates, ISchedulingOptions schedulingOptions);
    }

    public class BlockOptimizerBlockCleaner : IBlockOptimizerBlockCleaner
    {
        private readonly ISchedulePartModifyAndRollbackService _modifyAndRollbackService;
        private readonly IBlockFinderFactory _blockFinderFactory;
        private readonly IDeleteSchedulePartService _deleteSchedulePartService;

        public BlockOptimizerBlockCleaner(ISchedulePartModifyAndRollbackService modifyAndRollbackService, 
            IBlockFinderFactory blockFinderFactory, IDeleteSchedulePartService deleteSchedulePartService)
        {
            _modifyAndRollbackService = modifyAndRollbackService;
            _blockFinderFactory = blockFinderFactory;
            _deleteSchedulePartService = deleteSchedulePartService;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public IList<DateOnly> ClearSchedules(IScheduleMatrixPro matrix, IList<DateOnly> dates, ISchedulingOptions schedulingOptions)
        {
            var retList = new List<DateOnly>();
            foreach (var dateOnly in dates)
            {
                if (schedulingOptions.UseBlockOptimizing == BlockFinderType.BetweenDayOff)
                    retList.AddRange(clearBetweenDayOffs(matrix, dateOnly));
                else
                    retList.AddRange(clearInSchedulePeriod(matrix));
            }
            return retList;
        }

        private IList<DateOnly>  clearInSchedulePeriod(IScheduleMatrixPro matrix)
        {
            var retList = new List<DateOnly>();
            var parts = new List<IScheduleDay>();
            foreach (var scheduleDayPro in  matrix.UnlockedDays)
            {
                parts.Add(scheduleDayPro.DaySchedulePart());
            }

            if (parts.Count == 0)
                return retList;


            using (var bg = new BackgroundWorker())
            {
                _deleteSchedulePartService.Delete(parts, new DeleteOption { MainShift = true }, _modifyAndRollbackService, bg);
            }

            foreach (var scheduleDay in parts)
            {
                retList.Add(scheduleDay.DateOnlyAsPeriod.DateOnly);
            }

            return retList;
        }

        private IList<DateOnly> clearBetweenDayOffs(IScheduleMatrixPro matrix, DateOnly dateOnly)
        {
            var finder = _blockFinderFactory.CreateBetweenDayOffBlockFinder(matrix);
            var days = finder.FindValidBlockForDate(dateOnly);
            var parts = new List<IScheduleDay>();
            var retList = new List<DateOnly>();
            var unlockedDays = matrix.UnlockedDays;
            foreach (var day in days.BlockDays)
            {
                var scheduleDayPro = matrix.GetScheduleDayByKey(day);
                if (unlockedDays.Contains(scheduleDayPro))
                    parts.Add(scheduleDayPro.DaySchedulePart());
            }
            if (parts.Count == 0)
                return retList;


            using (var bg = new BackgroundWorker())
            {
                _deleteSchedulePartService.Delete(parts, new DeleteOption { MainShift = true }, _modifyAndRollbackService, bg);
            }

            foreach (var scheduleDay in parts)
            {
                retList.Add(scheduleDay.DateOnlyAsPeriod.DateOnly);
            }

            return retList;
        }
    }
}