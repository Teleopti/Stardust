using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
    public interface ITeamBlockFairnessOptimizationService
    {
        void Exectue(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
                                     IList<IPerson> selectedPersons, 
                                     ISchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories);
    }

    public class TeamBlockFairnessOptimizationService : ITeamBlockFairnessOptimizationService
    {
        private readonly IConstructTeamBlock _constructTeamBlock;
        private readonly ITeamBlockSizeClassifier _teamBlockSizeClassifier;
        private readonly ITeamBlockListSwapAnalyzer _teamBlockListSwapAnalyzer;

        public TeamBlockFairnessOptimizationService(IConstructTeamBlock constructTeamBlock,
                                          ITeamBlockSizeClassifier teamBlockSizeClassifier, ITeamBlockListSwapAnalyzer teamBlockListSwapAnalyzer)
        {
            _constructTeamBlock = constructTeamBlock;
            _teamBlockSizeClassifier = teamBlockSizeClassifier;
            _teamBlockListSwapAnalyzer = teamBlockListSwapAnalyzer;
        }

        public void Exectue(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
                            IList<IPerson> selectedPersons, 
                            ISchedulingOptions schedulingOptions, IList<IShiftCategory> shiftCategories)
        {
            IList<ITeamBlockInfo> listOfAllTeamBlock = _constructTeamBlock.Construct(allPersonMatrixList, selectedPeriod,
                                                                                     selectedPersons,
                                                                                     schedulingOptions);
            HashSet<IList<ITeamBlockInfo>> listOfMultipleLengthBlocks =
                _teamBlockSizeClassifier.SplitTeamBlockInfo(listOfAllTeamBlock);
            //Parallel.ForEach(listOfMultipleLengthBlocks.GetRandom(listOfAllTeamBlock.Count, true), teamBlockList => _teamBlockListSwapAnalyzer.AnalyzeTeamBlock(teamBlockList, shiftCategories));
            foreach (var teamBlockList in listOfMultipleLengthBlocks.GetRandom(listOfAllTeamBlock.Count, true))
            {
                _teamBlockListSwapAnalyzer.AnalyzeTeamBlock(teamBlockList, shiftCategories);
            }
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