﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
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
        private readonly ISwapScheduleDays _swapScheduleDays;
        private readonly IValidateScheduleDays _validateScheduleDays;

        public FairnessOptimizationCommand(IMatrixListFactory matrixListFactory, IGroupPersonBuilderForOptimizationFactory groupPersonBuilderForOptimizationFactory, ITeamBlockInfoFactory teamBlockInfoFactory, ITeamBlockSizeClassifier teamBlockSizeClassifier, ISwapScheduleDays swapScheduleDays, IValidateScheduleDays validateScheduleDays)
        {
            _matrixListFactory = matrixListFactory;
            _groupPersonBuilderForOptimizationFactory = groupPersonBuilderForOptimizationFactory;
            _teamBlockInfoFactory = teamBlockInfoFactory;
            _teamBlockSizeClassifier = teamBlockSizeClassifier;
            _swapScheduleDays = swapScheduleDays;
            _validateScheduleDays = validateScheduleDays;
        }

        public void Execute(DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPerson, IList<IScheduleDay> scheduleDays, IList<IShiftCategory> shiftCategories, ISchedulingOptions schedulingOptions)
        {
            var allVisibleMatrixes = _matrixListFactory.CreateMatrixListAll(selectedPeriod);
            var groupPersonBuilderForOptimization = _groupPersonBuilderForOptimizationFactory.Create(schedulingOptions);
            var teamInfoFactory = new TeamInfoFactory(groupPersonBuilderForOptimization);
            var constructTeamBlock = new ConstructTeamBlock(teamInfoFactory,_teamBlockInfoFactory);
	        var seniorityExtractor = new SeniorityExtractor();
	        var shiftCategoryPointExtractor = new ShiftCategoryPointExtractor(shiftCategories);
	        var shiftCategoryPointInfoExtractor = new ShiftCategoryPointInfoExtractor(shiftCategoryPointExtractor);
			var determineTeamBlockPriority = new DetermineTeamBlockPriority(seniorityExtractor, shiftCategoryPointInfoExtractor);
            var teamBlockListSwapAnalyzer = new TeamBlockListSwapAnalyzer(determineTeamBlockPriority ,_swapScheduleDays,_validateScheduleDays);
            var teamBlockFairnessOptimizer = new TeamBlockFairnessOptimizationService(constructTeamBlock, _teamBlockSizeClassifier, teamBlockListSwapAnalyzer);
            teamBlockFairnessOptimizer.Exectue(allVisibleMatrixes, selectedPeriod, selectedPerson, schedulingOptions, shiftCategories);
        }
    }
}
