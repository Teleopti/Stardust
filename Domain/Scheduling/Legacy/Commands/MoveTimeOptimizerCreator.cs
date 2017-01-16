using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class MoveTimeOptimizerCreator
	{
		private readonly IMoveTimeDecisionMaker _decisionMaker;
		private readonly IScheduleService _scheduleService;
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
		private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
		private readonly IUserTimeZone _userTimeZone;

		public MoveTimeOptimizerCreator(
			IMoveTimeDecisionMaker decisionMaker,
			IScheduleService scheduleService, 
			Func<ISchedulingResultStateHolder> schedulingResultStateHolder,
			IEffectiveRestrictionCreator effectiveRestrictionCreator, 
			IResourceCalculation resourceOptimizationHelper, 
			IDeleteAndResourceCalculateService deleteAndResourceCalculateService, 
			IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider, 
			IUserTimeZone userTimeZone) 
		{
			_decisionMaker = decisionMaker;
			_scheduleService = scheduleService;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_deleteAndResourceCalculateService = deleteAndResourceCalculateService;
			_scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
			_userTimeZone = userTimeZone;
		}

		public IList<IMoveTimeOptimizer> Create(IList<IScheduleMatrixOriginalStateContainer> scheduleMatrixContainerList,
						IList<IScheduleMatrixOriginalStateContainer> workShiftContainerList,
						IOptimizationPreferences optimizerPreferences,
						IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
						ISchedulePartModifyAndRollbackService rollbackService)
		{
			IList<IMoveTimeOptimizer> result = new List<IMoveTimeOptimizer>();

			for (int index = 0; index < scheduleMatrixContainerList.Count; index++)
			{

				IScheduleMatrixOriginalStateContainer scheduleMatrixContainer = scheduleMatrixContainerList[index];

				IScheduleMatrixPro scheduleMatrixPro = scheduleMatrixContainer.ScheduleMatrix;

				IScheduleResultDataExtractorProvider dataExtractorProvider = _scheduleResultDataExtractorProvider;
				IScheduleResultDataExtractor personalSkillsDataExtractor = dataExtractorProvider.CreatePersonalSkillDataExtractor(scheduleMatrixPro, optimizerPreferences.Advanced, _schedulingResultStateHolder());

				IPeriodValueCalculatorProvider periodValueCalculatorProvider = new PeriodValueCalculatorProvider();
				IPeriodValueCalculator periodValueCalculator =
					periodValueCalculatorProvider.CreatePeriodValueCalculator(optimizerPreferences.Advanced, personalSkillsDataExtractor);

				IScheduleMatrixOriginalStateContainer workShiftContainer = workShiftContainerList[index];

				var restrictionChecker = new RestrictionChecker();

				var dayOffOptimizationPreference = dayOffOptimizationPreferenceProvider.ForAgent(scheduleMatrixPro.Person, scheduleMatrixPro.EffectivePeriodDays.First().Day);

				var optimizerOverLimitDecider = new OptimizationOverLimitByRestrictionDecider(restrictionChecker, optimizerPreferences, scheduleMatrixContainer, dayOffOptimizationPreference);
				var optimizationLimits = new OptimizationLimits(optimizerOverLimitDecider);

				var schedulingOptionsCreator = new SchedulingOptionsCreator();
				IMainShiftOptimizeActivitySpecificationSetter mainShiftOptimizeActivitySpecificationSetter = new MainShiftOptimizeActivitySpecificationSetter();

				IMoveTimeOptimizer optimizer =
					new MoveTimeOptimizer(
						periodValueCalculator,
						personalSkillsDataExtractor,
						_decisionMaker,
						_scheduleService,
						optimizerPreferences,
						rollbackService,
						_deleteAndResourceCalculateService,
						_resourceOptimizationHelper,
						_effectiveRestrictionCreator,
						workShiftContainer,
						optimizationLimits,
						schedulingOptionsCreator,
						mainShiftOptimizeActivitySpecificationSetter,
						scheduleMatrixPro,
						_schedulingResultStateHolder(),
						_userTimeZone);

				result.Add(optimizer);
			}
			return result;
		}
	}
}
