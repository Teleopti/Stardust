using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
    public interface ITeamBlockFairnessOptimizer
    {
        void Exectue(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
                                     IList<IPerson> selectedPersons, 
                                     ISchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories);
    }

    public class TeamBlockFairnessOptimizer : ITeamBlockFairnessOptimizer
    {
        private readonly IConstructTeamBlock _constructTeamBlock;
        private readonly IDetermineTeamBlockPriority _determineTeamBlockPriority;
        private readonly ITeamBlockSizeClassifier _teamBlockSizeClassifier;

        public TeamBlockFairnessOptimizer(IConstructTeamBlock constructTeamBlock,
                                          ITeamBlockSizeClassifier teamBlockSizeClassifier,
                                          IDetermineTeamBlockPriority determineTeamBlockPriority)
        {
            _constructTeamBlock = constructTeamBlock;
            _teamBlockSizeClassifier = teamBlockSizeClassifier;
            _determineTeamBlockPriority = determineTeamBlockPriority;
        }

        public void Exectue(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
                            IList<IPerson> selectedPersons, 
                            ISchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories)
        {
            IList<ITeamBlockInfo> listOfAllTeamBlock = _constructTeamBlock.Constract(allPersonMatrixList, selectedPeriod,
                                                                                     selectedPersons,
                                                                                     schedulingOptions);
            HashSet<IList<ITeamBlockInfo>> listOfMultipleLengthBlocks =
                _teamBlockSizeClassifier.SplitTeamBlockInfo(listOfAllTeamBlock);
            foreach (var teamBlockList in listOfMultipleLengthBlocks.GetRandom(listOfAllTeamBlock.Count, true))
            {
                analyzeListForSwapping(teamBlockList, shiftCategories);
            }
        }

        private void analyzeListForSwapping(IList<ITeamBlockInfo> teamBlockList, IList<IShiftCategory> shiftCategories)
        {
            var teamBlockPriorityDefinition = new TeamBlockPriorityDefinitionInfo( _determineTeamBlockPriority.CalculatePriority(teamBlockList, shiftCategories));
            foreach (int higherPriority in teamBlockPriorityDefinition.HighToLowAgentPriorityList)
            {
                foreach (int lowerPriority in teamBlockPriorityDefinition.LowToHighAgentPriorityList)
                {
                    ITeamBlockInfo higherPriorityBlock = teamBlockPriorityDefinition.BlockOnAgentPriority(higherPriority);
                    int lowestShiftCategoryPrioirty =
                        teamBlockPriorityDefinition.GetShiftCategoryPriorityOfBlock(higherPriorityBlock);
                    if (
                        teamBlockPriorityDefinition.HighToLowShiftCategoryPriorityList.Any(
                            higherShiftCategoryPriority => higherShiftCategoryPriority > lowestShiftCategoryPrioirty))
                    {
                        ITeamBlockInfo lowestPriorityBlock =
                            teamBlockPriorityDefinition.BlockOnAgentPriority(lowerPriority);
                        if (validateBlock(higherPriorityBlock, lowestPriorityBlock))
                            swapBlock(higherPriorityBlock, lowestPriorityBlock);
                    }
                }
            }
        }

        private void swapBlock(ITeamBlockInfo higherPriorityBlock, ITeamBlockInfo lowestPriorityBlock)
        {
            
        }

        private bool validateBlock(ITeamBlockInfo higherPriorityBlock, ITeamBlockInfo lowestPriorityBlock)
        {
            return true;
        }

        //void dayScheduled(object sender, SchedulingServiceBaseEventArgs e)
        //{
        //    OnDayScheduled(e);
        //}
        //public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        //protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs scheduleServiceBaseEventArgs)
        //{
        //    EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
        //    if (temp != null)
        //    {
        //        temp(this, scheduleServiceBaseEventArgs);
        //    }
        //    _cancelMe = scheduleServiceBaseEventArgs.Cancel;
        //}

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        //public void RaiseEventForTest(object sender, SchedulingServiceBaseEventArgs e)
        //{
        //    dayScheduled(sender, e);
        //}
    }
}