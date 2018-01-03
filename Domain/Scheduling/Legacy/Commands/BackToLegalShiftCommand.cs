using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.BackToLegalShift;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class BackToLegalShiftCommand
	{
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly IGroupPersonBuilderForOptimizationFactory _groupPersonBuilderForOptimizationFactory;
		private readonly MatrixListFactory _matrixListFactory;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly BackToLegalShiftService _backToLegalShiftService;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;
		private readonly IPersonListExtractorFromScheduleParts _extractor;
		private readonly PeriodExtractorFromScheduleParts _periodExtractor;
		private readonly IUserTimeZone _userTimeZone;
		private ISchedulingProgress _backgroundWorker;
		private readonly BlockPreferencesMapper _blockPreferencesMapper;

		public BackToLegalShiftCommand(ITeamBlockInfoFactory teamBlockInfoFactory,
			IGroupPersonBuilderForOptimizationFactory groupPersonBuilderForOptimizationFactory,
			MatrixListFactory matrixListFactory,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			BackToLegalShiftService backToLegalShiftService,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			IResourceCalculation resourceOptimizationHelper,
			IGroupPersonBuilderWrapper groupPersonBuilderWrapper,
			IPersonListExtractorFromScheduleParts extractor,
			PeriodExtractorFromScheduleParts periodExtractor,
			IUserTimeZone userTimeZone,
			BlockPreferencesMapper blockPreferencesMapper)
		{
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_groupPersonBuilderForOptimizationFactory = groupPersonBuilderForOptimizationFactory;
			_matrixListFactory = matrixListFactory;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_backToLegalShiftService = backToLegalShiftService;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
			_extractor = extractor;
			_periodExtractor = periodExtractor;
			_userTimeZone = userTimeZone;
			_blockPreferencesMapper = blockPreferencesMapper;
		}

		public void Execute(ISchedulingProgress backgroundWorker,
			IList<IScheduleDay> selectedSchedules,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IEnumerable<IPerson> allPermittedPersons)
		{
			_backgroundWorker = backgroundWorker;
			var optimizationPreferences = new OptimizationPreferences();
			setupPreferences(optimizationPreferences);
			var schedulingOptions = _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);
			schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SingleDay;

			_groupPersonBuilderWrapper.Reset();
			var groupPageType = schedulingOptions.GroupOnGroupPageForTeamBlockPer.Type;

			if (groupPageType == GroupPageType.SingleAgent)
				_groupPersonBuilderWrapper.SetSingleAgentTeam();
			else
				_groupPersonBuilderForOptimizationFactory.Create(allPermittedPersons, schedulingResultStateHolder.Schedules, schedulingOptions.GroupOnGroupPageForTeamBlockPer);

			var teamInfoFactory = new TeamInfoFactory(_groupPersonBuilderWrapper);

			var teamBlockGenerator = new TeamBlockGenerator(teamInfoFactory, _teamBlockInfoFactory, _blockPreferencesMapper);
			var selectedPeriod = _periodExtractor.ExtractPeriod(selectedSchedules);
			var allMatrixes = selectedPeriod.HasValue ? _matrixListFactory.CreateMatrixListAllForLoadedPeriod(schedulingResultStateHolder.Schedules, schedulingResultStateHolder.LoadedAgents, selectedPeriod.Value) : new List<IScheduleMatrixPro>();
			var selectedPersons = _extractor.ExtractPersons(selectedSchedules);
			var selectedTeamBlocks = teamBlockGenerator.Generate(schedulingResultStateHolder.LoadedAgents, allMatrixes, selectedPeriod.GetValueOrDefault(), selectedPersons, schedulingOptions);
			var tagSetter = new ScheduleTagSetter(KeepOriginalScheduleTag.Instance);
			var rollbackService = new SchedulePartModifyAndRollbackService(schedulingResultStateHolder,
				_scheduleDayChangeCallback,
				tagSetter);
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, true, schedulingResultStateHolder, _userTimeZone);
			_backToLegalShiftService.Progress += _backToLegalShiftService_Progress;
			_backToLegalShiftService.Execute(selectedTeamBlocks, schedulingOptions, schedulingResultStateHolder, rollbackService,
				resourceCalculateDelayer);
			_backToLegalShiftService.Progress -= _backToLegalShiftService_Progress;
		}

		void _backToLegalShiftService_Progress(object sender, BackToLegalShiftArgs e)
		{
			_backgroundWorker.ReportProgress(1, e);
			if (_backgroundWorker.CancellationPending)
			{
				e.Cancel = true;
			}
		}

		private static void setupPreferences(OptimizationPreferences optimizationPreferences)
		{
			var extraPreferences = optimizationPreferences.Extra;
			extraPreferences.TeamGroupPage = GroupPageLight.SingleAgentGroup(Resources.SingleAgentTeam);
			extraPreferences.BlockTypeValue = BlockFinderType.SingleDay;
			extraPreferences.UseTeamBlockOption = true;
			extraPreferences.UseTeams = false;
			extraPreferences.UseBlockSameStartTime = true;
			extraPreferences.UseBlockSameEndTime = true;
			var generalPreferences = optimizationPreferences.General;
			generalPreferences.UseAvailabilities = false;
			generalPreferences.UsePreferences = false;
			generalPreferences.UseRotations = false;
			generalPreferences.UseShiftCategoryLimitations = false;
			generalPreferences.UseStudentAvailabilities = false;
			optimizationPreferences.Advanced.UseAverageShiftLengths = false;
			optimizationPreferences.ShiftBagBackToLegal = true;
		}
	}
}