using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.BackToLegalShift;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
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
		private readonly IToggleManager _toggleManager;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private IBackgroundWorkerWrapper _backgroundWorker;

		public BackToLegalShiftCommand(ITeamBlockInfoFactory teamBlockInfoFactory,
			IGroupPersonBuilderForOptimizationFactory groupPersonBuilderForOptimizationFactory,
			ITeamBlockSchedulingOptions teamBlockSchedulingOptions,
			IMatrixListFactory matrixListFactory,
			ISchedulingOptionsCreator schedulingOptionsCreator,
			IBackToLegalShiftService backToLegalShiftService,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			IToggleManager toggleManager,
			IResourceOptimizationHelper resourceOptimizationHelper)
		{
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_groupPersonBuilderForOptimizationFactory = groupPersonBuilderForOptimizationFactory;
			_teamBlockSchedulingOptions = teamBlockSchedulingOptions;
			_matrixListFactory = matrixListFactory;
			_schedulingOptionsCreator = schedulingOptionsCreator;
			_backToLegalShiftService = backToLegalShiftService;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_toggleManager = toggleManager;
			_resourceOptimizationHelper = resourceOptimizationHelper;
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

			var groupPersonBuilderForOptimization = _groupPersonBuilderForOptimizationFactory.Create(schedulingOptions);
			var teamInfoFactory = new TeamInfoFactory(groupPersonBuilderForOptimization);

			var teamBlockGenerator = new TeamBlockGenerator(teamInfoFactory, _teamBlockInfoFactory,
				_teamBlockSchedulingOptions);
			var selectedPeriod = OptimizerHelperHelper.GetSelectedPeriod(selectedSchedules);
			IList<IScheduleMatrixPro> allMatrixes = _matrixListFactory.CreateMatrixListAll(selectedPeriod);
			var extractor = new PersonListExtractorFromScheduleParts(selectedSchedules);
			var selectedPersons = extractor.ExtractPersons().ToList();
			var selectedTeamBlocks = teamBlockGenerator.Generate(allMatrixes, selectedPeriod, selectedPersons, schedulingOptions);
			var tagSetter = new ScheduleTagSetter(KeepOriginalScheduleTag.Instance);
			var rollbackService = new SchedulePartModifyAndRollbackService(schedulingResultStateHolder,
				_scheduleDayChangeCallback,
				tagSetter);
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, false, true);
			var maxSeatToggle = _toggleManager.IsEnabled(Toggles.Scheduler_TeamBlockAdhereWithMaxSeatRule_23419);
			_backToLegalShiftService.Progress += _backToLegalShiftService_Progress;
			_backToLegalShiftService.Execute(selectedTeamBlocks, schedulingOptions, schedulingResultStateHolder, rollbackService,
				resourceCalculateDelayer, maxSeatToggle);
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
			var singleAgentEntry = new GroupPageLight { Key = "SingleAgentTeam", Name = Resources.SingleAgentTeam };
			var extraPreferences = optimizationPreferences.Extra;
			extraPreferences.TeamGroupPage = singleAgentEntry;
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