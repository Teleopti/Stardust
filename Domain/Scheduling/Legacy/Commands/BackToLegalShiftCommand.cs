using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.BackToLegalShift;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class BackToLegalShiftCommand
	{
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly IGroupPersonBuilderForOptimizationFactory _groupPersonBuilderForOptimizationFactory;
		private readonly ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
		private readonly IBackToLegalShiftService _backToLegalShiftService;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IOptimizerHelperHelper _optimizerHelper;
		private readonly IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;
		private IBackgroundWorkerWrapper _backgroundWorker;

		public BackToLegalShiftCommand(ITeamBlockInfoFactory teamBlockInfoFactory,
			IGroupPersonBuilderForOptimizationFactory groupPersonBuilderForOptimizationFactory,
			ITeamBlockSchedulingOptions teamBlockSchedulingOptions,
			IMatrixListFactory matrixListFactory,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			IBackToLegalShiftService backToLegalShiftService,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			IResourceOptimizationHelper resourceOptimizationHelper,
			IOptimizerHelperHelper optimizerHelper,
			IGroupPersonBuilderWrapper groupPersonBuilderWrapper)
		{
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_groupPersonBuilderForOptimizationFactory = groupPersonBuilderForOptimizationFactory;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_matrixListFactory = matrixListFactory;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_backToLegalShiftService = backToLegalShiftService;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_optimizerHelper = optimizerHelper;
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
		}

		public void Execute(IBackgroundWorkerWrapper backgroundWorker,
			IList<IScheduleDay> selectedSchedules,
			ISchedulingResultStateHolder schedulingResultStateHolder)
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
				_groupPersonBuilderForOptimizationFactory.Create(schedulingOptions);

			var teamInfoFactory = new TeamInfoFactory(_groupPersonBuilderWrapper);

			var teamBlockGenerator = new TeamBlockGenerator(teamInfoFactory, _teamBlockInfoFactory,
				_teamBlockSchedulingOptions);
			var selectedPeriod = _optimizerHelper.GetSelectedPeriod(selectedSchedules);
			IList<IScheduleMatrixPro> allMatrixes = _matrixListFactory.CreateMatrixListAllForLoadedPeriod(selectedPeriod);
			var extractor = new PersonListExtractorFromScheduleParts(selectedSchedules);
			var selectedPersons = extractor.ExtractPersons().ToList();
			var selectedTeamBlocks = teamBlockGenerator.Generate(allMatrixes, selectedPeriod, selectedPersons, schedulingOptions);
			var tagSetter = new ScheduleTagSetter(KeepOriginalScheduleTag.Instance);
			var rollbackService = new SchedulePartModifyAndRollbackService(schedulingResultStateHolder,
				_scheduleDayChangeCallback,
				tagSetter);
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, true);
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
		}
	}
}