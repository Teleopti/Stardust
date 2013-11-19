using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Optimization.Fairness;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Commands
{
    
    public interface IFairnessOptimizationCommand
    {
        void Execute(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPerson, IList<IScheduleDay> scheduleDays, IList<IShiftCategory> shiftCategories, ISchedulingOptions schedulingOptions);
    }
    public class FairnessOptimizationCommand : IFairnessOptimizationCommand 
    {
        private readonly IMatrixListFactory _matrixListFactory;
        private readonly IGroupPersonBuilderForOptimizationFactory _groupPersonBuilderForOptimizationFactory;
        private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
        private readonly ITeamBlockSizeClassifier _teamBlockSizeClassifier;
        private readonly IDetermineTeamBlockPriority _determineTeamBlockPriority;

        public FairnessOptimizationCommand(IMatrixListFactory matrixListFactory, IGroupPersonBuilderForOptimizationFactory groupPersonBuilderForOptimizationFactory, ITeamBlockInfoFactory teamBlockInfoFactory, ITeamBlockSizeClassifier teamBlockSizeClassifier, IDetermineTeamBlockPriority determineTeamBlockPriority)
        {
            _matrixListFactory = matrixListFactory;
            _groupPersonBuilderForOptimizationFactory = groupPersonBuilderForOptimizationFactory;
            _teamBlockInfoFactory = teamBlockInfoFactory;
            _teamBlockSizeClassifier = teamBlockSizeClassifier;
            _determineTeamBlockPriority = determineTeamBlockPriority;
        }

        public void Execute(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPerson, IList<IScheduleDay> scheduleDays, IList<IShiftCategory> shiftCategories, ISchedulingOptions schedulingOptions)
        {
            var allVisibleMatrixes = _matrixListFactory.CreateMatrixListAll(selectedPeriod);
            var groupPersonBuilderForOptimization = _groupPersonBuilderForOptimizationFactory.Create(schedulingOptions);
            ITeamInfoFactory teamInfoFactory = new TeamInfoFactory(groupPersonBuilderForOptimization);
            IConstructTeamBlock constructTeamBlock = new ConstructTeamBlock(teamInfoFactory,_teamBlockInfoFactory);
            var teamBlockFairnessOptimizer = new TeamBlockFairnessOptimizer(constructTeamBlock, _teamBlockSizeClassifier,
                                                                            _determineTeamBlockPriority);
            teamBlockFairnessOptimizer.Exectue(allVisibleMatrixes, selectedPeriod, selectedPerson, schedulingOptions, shiftCategories);
        }
    }
}
