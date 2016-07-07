﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class MoveTimeOptimizerCreator : IMoveTimeOptimizerCreator
	{
		private readonly IList<IScheduleMatrixOriginalStateContainer> _scheduleMatrixContainerList;
		private readonly IList<IScheduleMatrixOriginalStateContainer> _workShiftContainerList;
		private readonly IMoveTimeDecisionMaker _decisionMaker;
		private readonly IScheduleService _scheduleService;
		private readonly IOptimizationPreferences _optimizerPreferences;
		private readonly ISchedulePartModifyAndRollbackService _rollbackService;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IDayOffOptimizationPreferenceProvider _dayOffOptimizationPreferenceProvider;
		private readonly IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;

		public MoveTimeOptimizerCreator(
			IList<IScheduleMatrixOriginalStateContainer> scheduleMatrixContainerList,
			IList<IScheduleMatrixOriginalStateContainer> workShiftContainerList,
			IMoveTimeDecisionMaker decisionMaker,
			IScheduleService scheduleService,
			IOptimizationPreferences optimizerPreferences,
			ISchedulePartModifyAndRollbackService rollbackService,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IResourceOptimizationHelper resourceOptimizationHelper,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			IDeleteAndResourceCalculateService deleteAndResourceCalculateService)
		{
			_scheduleMatrixContainerList = scheduleMatrixContainerList;
			_workShiftContainerList = workShiftContainerList;
			_decisionMaker = decisionMaker;
			_scheduleService = scheduleService;
			_optimizerPreferences = optimizerPreferences;
			_rollbackService = rollbackService;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_dayOffOptimizationPreferenceProvider = dayOffOptimizationPreferenceProvider;
			_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
		}

		/// <summary>
		/// Creates the list of optimizers.
		/// </summary>
		/// <returns></returns>
		public IList<IMoveTimeOptimizer> Create()
		{
			IList<IMoveTimeOptimizer> result = new List<IMoveTimeOptimizer>();

			for (int index = 0; index < _scheduleMatrixContainerList.Count; index++)
			{

				IScheduleMatrixOriginalStateContainer scheduleMatrixContainer = _scheduleMatrixContainerList[index];

				IScheduleMatrixPro scheduleMatrixPro = scheduleMatrixContainer.ScheduleMatrix;

				IScheduleResultDataExtractorProvider dataExtractorProvider = new ScheduleResultDataExtractorProvider();
				IScheduleResultDataExtractor personalSkillsDataExtractor = dataExtractorProvider.CreatePersonalSkillDataExtractor(scheduleMatrixPro, _optimizerPreferences.Advanced);

				IPeriodValueCalculatorProvider periodValueCalculatorProvider = new PeriodValueCalculatorProvider();
				IPeriodValueCalculator periodValueCalculator =
					periodValueCalculatorProvider.CreatePeriodValueCalculator(_optimizerPreferences.Advanced, personalSkillsDataExtractor);

				IDeleteSchedulePartService deleteSchedulePartService =
					new DeleteSchedulePartService(()=>_schedulingResultStateHolder);

				IScheduleMatrixOriginalStateContainer workShiftContainer = _workShiftContainerList[index];

				var restrictionChecker = new RestrictionChecker();

				var dayOffOptimizationPreference = _dayOffOptimizationPreferenceProvider.ForAgent(scheduleMatrixPro.Person, scheduleMatrixPro.EffectivePeriodDays.First().Day);

				var optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(restrictionChecker, _optimizerPreferences, scheduleMatrixContainer, dayOffOptimizationPreference);
				var optimizationLimits = new OptimizationLimits(optimizerOverLimitDecider);

				var schedulingOptionsCreator = new SchedulingOptionsCreator();
				IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter = new MainShiftOptimizeActivitySpecificationSetter();

				IMoveTimeOptimizer optimizer =
					new MoveTimeOptimizer(
						periodValueCalculator,
						personalSkillsDataExtractor,
						_decisionMaker,
						_scheduleService,
						_optimizerPreferences,
						_rollbackService,
						_deleteAndResourceCalculateService,
						_resourceOptimizationHelper,
						_effectiveRestrictionCreator,
						workShiftContainer,
						optimizationLimits,
						schedulingOptionsCreator,
						mainShiftOptimizeActivitySpecificationSetter,
						scheduleMatrixPro,
						_schedulingResultStateHolder);

				result.Add(optimizer);
			}
			return result;
		}
	}
}
