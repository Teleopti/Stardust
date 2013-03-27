using System;
using System.Collections.Generic;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using System.Linq;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IGroupDayOffOptimizer
    {
		bool Execute(IScheduleMatrixPro matrix, IList<IScheduleMatrixPro> allMatrixes, ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, ITeamSteadyStateMainShiftScheduler teamSteadyStateMainShiftScheduler, ITeamSteadyStateHolder teamSteadyStateHolder, IScheduleDictionary scheduleDictionary);
        ILockableBitArray WorkingBitArray { get; }
    }

    public class GroupDayOffOptimizer : IGroupDayOffOptimizer
    {
        private readonly IScheduleMatrixLockableBitArrayConverter _converter;
        private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
        private readonly IDayOffDecisionMaker _decisionMaker;
        private readonly IDaysOffPreferences _daysOffPreferences;
        private readonly IDayOffDecisionMakerExecuter _dayOffDecisionMakerExecuter;
        private readonly ILockableBitArrayChangesTracker _changesTracker;
        private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
        private readonly IGroupSchedulingService _groupSchedulingService;
        private readonly IGroupMatrixHelper _groupMatrixHelper;
    	private readonly IGroupOptimizationValidatorRunner _groupOptimizationValidatorRunner;
    	private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
    	private readonly ISmartDayOffBackToLegalStateService _smartDayOffBackToLegalStateService;

    	public GroupDayOffOptimizer(IScheduleMatrixLockableBitArrayConverter converter,
            IDayOffDecisionMaker decisionMaker,
            IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider,
            IDaysOffPreferences daysOffPreferences,
            IDayOffDecisionMakerExecuter dayOffDecisionMakerExecuter,
            ILockableBitArrayChangesTracker changesTracker,
            ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
            IGroupSchedulingService groupSchedulingService,
            IGroupMatrixHelper groupMatrixHelper,
			IGroupOptimizationValidatorRunner groupOptimizationValidatorRunner,
			IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization,
			ISmartDayOffBackToLegalStateService smartDayOffBackToLegalStateService)
        {
            _converter = converter;
            _scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
            _decisionMaker = decisionMaker;
            _daysOffPreferences = daysOffPreferences;
            _dayOffDecisionMakerExecuter = dayOffDecisionMakerExecuter;
            _changesTracker = changesTracker;
            _schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
            _groupSchedulingService = groupSchedulingService;
            _groupMatrixHelper = groupMatrixHelper;
    		_groupOptimizationValidatorRunner = groupOptimizationValidatorRunner;
    		_groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
    		_smartDayOffBackToLegalStateService = smartDayOffBackToLegalStateService;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public bool Execute(IScheduleMatrixPro matrix, IList<IScheduleMatrixPro> allMatrixes, ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, ITeamSteadyStateMainShiftScheduler teamSteadyStateMainShiftScheduler, ITeamSteadyStateHolder teamSteadyStateHolder, IScheduleDictionary scheduleDictionary)
		{
			ILockableBitArray originalArray = _converter.Convert(_daysOffPreferences.ConsiderWeekBefore,
			                                                     _daysOffPreferences.ConsiderWeekAfter);
            WorkingBitArray = _converter.Convert(_daysOffPreferences.ConsiderWeekBefore,
			                                                       _daysOffPreferences.ConsiderWeekAfter);

			IScheduleResultDataExtractor scheduleResultDataExtractor =
				_scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(matrix, optimizationPreferences.Advanced);

            bool success = _decisionMaker.Execute(WorkingBitArray, scheduleResultDataExtractor.Values());
			if (!success)
			{
				success = _smartDayOffBackToLegalStateService.Execute(_smartDayOffBackToLegalStateService.BuildSolverList(WorkingBitArray), 100);
				if (!success)
					return false;

				success = _decisionMaker.Execute(WorkingBitArray, scheduleResultDataExtractor.Values());
				if (!success)
					return false;
			}
			// DayOffBackToLegal if decisionMaker did something wrong
            success = _smartDayOffBackToLegalStateService.Execute(_smartDayOffBackToLegalStateService.BuildSolverList(WorkingBitArray), 100);
			if (!success)
				return false;

            IList<DateOnly> daysOffToRemove = _changesTracker.DaysOffRemoved(WorkingBitArray, originalArray, matrix,
			                                                                 _daysOffPreferences.ConsiderWeekBefore);
            IList<DateOnly> daysOffToAdd = _changesTracker.DaysOffAdded(WorkingBitArray, originalArray, matrix,
			                                                            _daysOffPreferences.ConsiderWeekBefore);

            if (daysOffToRemove.Count == 0)
                return false;
			IPerson person = matrix.Person;
			IGroupPerson groupPerson = _groupPersonBuilderForOptimization.BuildGroupPerson(person, daysOffToRemove[0]);
			if (groupPerson == null)
				return false;

			ValidatorResult result = new ValidatorResult();
			result.Success = true;

			IList<GroupMatrixContainer> containers;
			
			if(schedulingOptions.UseSameDayOffs)
			{
				//Will always return true IFairnessValueCalculator not using GroupOptimizerValidateProposedDatesInSameMatrix daysoff
				result = _groupOptimizationValidatorRunner.Run(person, daysOffToRemove, daysOffToAdd, schedulingOptions.UseSameDayOffs);
				if (!result.Success)
				{
					return false;
				}
				
				containers = _groupMatrixHelper.CreateGroupMatrixContainers(allMatrixes, daysOffToRemove, daysOffToAdd, groupPerson, _daysOffPreferences);
			}
			else
			{
				containers = _groupMatrixHelper.CreateGroupMatrixContainers(allMatrixes, daysOffToRemove, daysOffToAdd, person, _daysOffPreferences);
			}
			
			if (containers == null || containers.Count() == 0)
				return false;

			if(result.MatrixList.Count == 0)
			{
				//kan flytta hur mycket som helst, behöver få veta vad som ska schemaläggas
				if (!_groupMatrixHelper.ExecuteDayOffMoves(containers, _dayOffDecisionMakerExecuter, _schedulePartModifyAndRollbackService))
					return false;
                daysOffToRemove = _changesTracker.DaysOffRemoved(WorkingBitArray, originalArray, matrix,
																			 _daysOffPreferences.ConsiderWeekBefore);

				IList<IScheduleDay> removedDays = _groupMatrixHelper.GoBackToLegalState(daysOffToRemove, groupPerson, schedulingOptions, allMatrixes, _schedulePartModifyAndRollbackService);
				if (removedDays == null)
					return false;

				var teamSteadyStateSuccess = false;

				if(teamSteadyStateHolder.IsSteadyState(groupPerson))
				{
					foreach (var dateOnly in daysOffToRemove)
					{
						teamSteadyStateSuccess = teamSteadyStateMainShiftScheduler.ScheduleTeam(dateOnly, groupPerson, _groupSchedulingService, _schedulePartModifyAndRollbackService, schedulingOptions, _groupPersonBuilderForOptimization, allMatrixes, scheduleDictionary);

						if (!teamSteadyStateSuccess)
							break;
					}	
				}

				if (!teamSteadyStateSuccess)
				{
					if (!_groupMatrixHelper.ScheduleRemovedDayOffDays(daysOffToRemove, groupPerson, _groupSchedulingService,_schedulePartModifyAndRollbackService, schedulingOptions,_groupPersonBuilderForOptimization, allMatrixes))
						return false;
				}

				if (!_groupMatrixHelper.ScheduleBackToLegalStateDays(removedDays, _groupSchedulingService, _schedulePartModifyAndRollbackService, schedulingOptions, optimizationPreferences, _groupPersonBuilderForOptimization, allMatrixes))
					return false;
			}
			
			return true;
		}

        public ILockableBitArray WorkingBitArray { get; private set; }
    }

    public class GroupMatrixContainer
    {
        public IScheduleMatrixPro Matrix { get; set; }
        public ILockableBitArray OriginalArray { get; set; }
        public ILockableBitArray WorkingArray { get; set; }
    }
}
