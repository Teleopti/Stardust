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
		bool Execute(IScheduleMatrixPro matrix, IList<IScheduleMatrixPro> allMatrixes, ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, ITeamSteadyStateMainShiftScheduler teamSteadyStateMainShiftScheduler, IDictionary<Guid, bool> teamSteadyStates, IScheduleDictionary scheduleDictionary);
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
			IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization)
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
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public bool Execute(IScheduleMatrixPro matrix, IList<IScheduleMatrixPro> allMatrixes, ISchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences, ITeamSteadyStateMainShiftScheduler teamSteadyStateMainShiftScheduler, IDictionary<Guid, bool> teamSteadyStates, IScheduleDictionary scheduleDictionary)
		{
			ILockableBitArray originalArray = _converter.Convert(_daysOffPreferences.ConsiderWeekBefore,
			                                                     _daysOffPreferences.ConsiderWeekAfter);
			ILockableBitArray workingBitArray = _converter.Convert(_daysOffPreferences.ConsiderWeekBefore,
			                                                       _daysOffPreferences.ConsiderWeekAfter);

			IScheduleResultDataExtractor scheduleResultDataExtractor =
				_scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(matrix);
			bool decisionMakerFoundDays = _decisionMaker.Execute(workingBitArray, scheduleResultDataExtractor.Values());
			if (!decisionMakerFoundDays)
				return false;

			IList<DateOnly> daysOffToRemove = _changesTracker.DaysOffRemoved(workingBitArray, originalArray, matrix,
			                                                                 _daysOffPreferences.ConsiderWeekBefore);
			IList<DateOnly> daysOffToAdd = _changesTracker.DaysOffAdded(workingBitArray, originalArray, matrix,
			                                                            _daysOffPreferences.ConsiderWeekBefore);

			

			IList<GroupMatrixContainer> containers;
			IGroupPerson groupPerson = _groupPersonBuilderForOptimization.BuildGroupPerson(matrix.Person, daysOffToRemove[0]);
			if (groupPerson == null)
				return false;

			ValidatorResult result = new ValidatorResult();
			result.Success = true;

			if(schedulingOptions.UseSameDayOffs)
			{
				//Will always return true IFairnessValueCalculator not using GroupOptimizerValidateProposedDatesInSameMatrix daysoff
				result = _groupOptimizationValidatorRunner.Run(matrix.Person, daysOffToRemove, daysOffToAdd, schedulingOptions.UseSameDayOffs);
				if (!result.Success)
				{
					return false;
				}
				
				containers = _groupMatrixHelper.CreateGroupMatrixContainers(allMatrixes, daysOffToRemove, daysOffToAdd, groupPerson, _daysOffPreferences);
			}
			else
			{
				containers = _groupMatrixHelper.CreateGroupMatrixContainers(allMatrixes, daysOffToRemove, daysOffToAdd, matrix.Person, _daysOffPreferences);
			}
			
			if (containers == null || containers.Count() == 0)
				return false;

			if(result.MatrixList.Count == 0)
			{
				//kan flytta hur mycket som helst, behöver få veta vad som ska schemaläggas
				//int x = daysOffToRemove.Count;
				if (!_groupMatrixHelper.ExecuteDayOffMoves(containers, _dayOffDecisionMakerExecuter, _schedulePartModifyAndRollbackService))
					return false;
				daysOffToRemove = _changesTracker.DaysOffRemoved(workingBitArray, originalArray, matrix,
																			 _daysOffPreferences.ConsiderWeekBefore);
				//if (daysOffToRemove.Count != x)
				//    return false;

				IList<IScheduleDay> removedDays = _groupMatrixHelper.GoBackToLegalState(daysOffToRemove, groupPerson, schedulingOptions, allMatrixes, _schedulePartModifyAndRollbackService);
				if (removedDays == null)
					return false;

				var teamSteadyStateSuccess = false;
				if (groupPerson.Id.HasValue && teamSteadyStates.ContainsKey(groupPerson.Id.Value) && teamSteadyStates[groupPerson.Id.Value])
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


    }

    public class GroupMatrixContainer
    {
        public IScheduleMatrixPro Matrix { get; set; }
        public ILockableBitArray OriginalArray { get; set; }
        public ILockableBitArray WorkingArray { get; set; }
    }
}
