using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public interface IBlockFinderFactory
    {
        IBlockFinder CreateFinder(IScheduleMatrixPro matrix, BlockFinderType blockFinderType);
        IBetweenDayOffBlockFinder CreateBetweenDayOffBlockFinder(IScheduleMatrixPro matrix);
    }

    public class BlockFinderFactory : IBlockFinderFactory
    {
        public IBlockFinder CreateFinder(IScheduleMatrixPro matrix, BlockFinderType blockFinderType)
        {
            IBlockFinder finder = null;
            switch (blockFinderType)
            {
                case BlockFinderType.BetweenDayOff:
                    {
                        finder = new BetweenDayOffBlockFinder(matrix, new EmptyDaysInBlockOutsideSelectedHandler());
                        break;
                    }

                case BlockFinderType.SchedulePeriod:
                    {
                        finder = new SchedulePeriodBlockFinder(matrix, new EmptyDaysInBlockOutsideSelectedHandler());
                        break;
                    }

            }

            return finder;
        }

        public IBetweenDayOffBlockFinder CreateBetweenDayOffBlockFinder(IScheduleMatrixPro matrix)
        {
			return new BetweenDayOffBlockFinder(matrix, new EmptyDaysInBlockOutsideSelectedHandler());
        }
    }

    public class BlockSchedulingService : IBlockSchedulingService
    {
        private readonly IBestBlockShiftCategoryFinder _blockShiftCategoryFinder;
        private readonly IScheduleDayService _scheduleDayService;
        private readonly IBlockFinderFactory _blockFinderFactory;
        private bool _cancelMe;

        public BlockSchedulingService( IBestBlockShiftCategoryFinder blockShiftCategoryFinder,
            IScheduleDayService scheduleDayService, IBlockFinderFactory blockFinderFactory )
        {
            _blockShiftCategoryFinder = blockShiftCategoryFinder;
            _scheduleDayService = scheduleDayService;
            _blockFinderFactory = blockFinderFactory;
        }

        public event EventHandler<BlockSchedulingServiceEventArgs> BlockScheduled;

		public bool Execute(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions)
        {
			return Execute(matrixList, schedulingOptions, new Dictionary<string, IWorkShiftFinderResult>());
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public bool Execute(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions,
            IDictionary<string, IWorkShiftFinderResult> workShiftFinderResultList)
        {
            bool success = true;
            _cancelMe = false;
            IList<IBlockFinder> finders = CreateFinders(matrixList, BlockFinderType.None );

            IBlockFinderResult result;
            int blockCounter = 0;
            foreach (var blockFinder in finders)
            {
                do
                {
                    result = blockFinder.NextBlock();
                    if (result.BlockDays.Count > 0)
                        blockCounter++;
                } while (result.BlockDays.Count > 0);
                blockFinder.ResetBlockPointer();
            }

            bool validBlockFound;
            int blocksScheduled = 0;
            do
            {
                validBlockFound = false;
                foreach (var blockFinder in finders.GetRandom(finders.Count, true))
                {
                    if (_cancelMe) return false;

                    result = blockFinder.NextBlock();
                    if (result.WorkShiftFinderResult.Count > 0)
                    {
                        foreach (KeyValuePair<string, IWorkShiftFinderResult> pair in result.WorkShiftFinderResult)
                        {
                            if(!workShiftFinderResultList.ContainsKey(pair.Key))
                                workShiftFinderResultList.Add(pair);
                        }
                        success = false;
                    }

                    if (result.BlockDays.Count != 0)
                    {
                        blocksScheduled++;
                        OnBlockScheduled(blockCounter, blocksScheduled);
                        if (!ActOnResult(result, blockFinder.ScheduleMatrix, schedulingOptions))
                            success = false;
                        validBlockFound = true;
                    }
                    else
                    {
                        finders.Remove(blockFinder);
                    }
                }
            } while (validBlockFound);

            return success;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ActOn"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool ActOnResult(IBlockFinderResult result, IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions)
        {
            bool success = true;
            schedulingOptions.NotAllowedShiftCategories.Clear();

            if(result.BlockDays.Count == 0)
                return false;
            IList<IShiftCategory> bannedCategories = new List<IShiftCategory>();
            IShiftCategory bestCategory = null;
            bool scheduled;
            if(result.ShiftCategory == null)
            {
				var dictionary = _blockShiftCategoryFinder.ScheduleDictionary;
                do
                {
                    if (bestCategory != null)
                    {
                        bannedCategories.Add(bestCategory);
                        schedulingOptions.NotAllowedShiftCategories.Clear();
                        foreach (IShiftCategory category in bannedCategories)
                        {
                            schedulingOptions.NotAllowedShiftCategories.Add(category);
                        }
                    }

                	var best =
						_blockShiftCategoryFinder.BestShiftCategoryForDays(result, matrix.Person, schedulingOptions, dictionary[matrix.Person].FairnessPoints()).
                			BestPossible;
					if (best != null)
						bestCategory = best.ShiftCategory;
					else
						bestCategory = null;

					if (bestCategory == null)
                        return false;

                    schedulingOptions.ShiftCategory = bestCategory;
                    scheduled = TryScheduleBlock(result, matrix, schedulingOptions);
                    if (!scheduled)
                        success = false;

                } while (!scheduled);
            }
            else
            {
                bestCategory = result.ShiftCategory;
                schedulingOptions.ShiftCategory = bestCategory;
                scheduled = TryScheduleBlock(result, matrix, schedulingOptions);
                if (!scheduled)
                    success = false;
            }

            return success;
        }

        public IList<IBlockFinder> CreateFinders(IList<IScheduleMatrixPro> matrixList, BlockFinderType blockFinderType)
        {
            IList<IBlockFinder> ret = new List<IBlockFinder>();
            foreach (var matrixPro in matrixList)
            {
                IBlockFinder finder = CreateFinder(matrixPro, blockFinderType);
                ret.Add(finder);
            }

            return ret;
        }

        public IBlockFinder CreateFinder(IScheduleMatrixPro matrixPro, BlockFinderType blockFinderType)
        {
            return _blockFinderFactory.CreateFinder(matrixPro, blockFinderType);
        }

        protected void OnBlockScheduled(int totalBlocks, int currentScheduled)
        {
            var args = new BlockSchedulingServiceEventArgs
                           {
                               PercentageCompleted = (int) (currentScheduled/(double) totalBlocks*100)
                           };
            EventHandler<BlockSchedulingServiceEventArgs> temp = BlockScheduled;
            if (temp != null)
            {
                temp(this, args);
                if (args.Cancel) _cancelMe = true;
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool TryScheduleBlock(IBlockFinderResult result, IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions)
        {
            IList<IScheduleDay> scheduledDays = new List<IScheduleDay>();
            foreach (var blockDay in result.BlockDays)
            {
                IScheduleDayPro scheduleDay = matrix.GetScheduleDayByKey(blockDay);
                if (matrix.UnlockedDays.Contains(scheduleDay))
                {
                    IScheduleDay schedulePart = scheduleDay.DaySchedulePart();
                    bool scheduleResult = _scheduleDayService.ScheduleDay(schedulePart, schedulingOptions);
                    if(!scheduleResult)
                    {
						if (!scheduledDays.IsEmpty())
							_scheduleDayService.DeleteMainShift(scheduledDays, schedulingOptions);
                        
                        return false;
                    }
                    scheduledDays.Add(schedulePart);
                }           
            }
            return true;
        }
    }
}