using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface ITeamBlockIntradayOptimizationService
    {
        void Optimize(IList<IScheduleMatrixPro> allPersonMatrixList,
                      DateOnlyPeriod selectedPeriod,
                      IList<IPerson> selectedPersons,
                      IOptimizationPreferences optimizationPreferences,
                      ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService);
    }

    public class TeamBlockIntradayOptimizationService : ITeamBlockIntradayOptimizationService
    {
        private readonly ITeamInfoFactory _teamInfoFactory;
        private readonly IBlockProvider _blockProvider;
        private readonly ITeamBlockScheduler _teamBlockScheduler;
        private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
        private readonly ILockableBitArrayFactory _lockableBitArrayFactory;
        private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
        private readonly ISchedulingOptionsCreator _schedulingOptionsCreator;
        private readonly ISchedulingResultStateHolder _stateHolder;
        private readonly IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;

        public TeamBlockIntradayOptimizationService(ITeamInfoFactory teamInfoFactory,
                                                    ITeamBlockInfoFactory teamBlockInfoFactory,
                                                    IBlockProvider blockProvider, ITeamBlockScheduler teamBlockScheduler,
                                                    ILockableBitArrayFactory lockableBitArrayFactory,
                                                    IScheduleResultDataExtractorProvider
                                                        scheduleResultDataExtractorProvider,
                                                    ISchedulingOptionsCreator schedulingOptionsCreator,
                                                    ISchedulingResultStateHolder stateHolder,
                                                    IDeleteAndResourceCalculateService deleteAndResourceCalculateService)
        {
            _teamInfoFactory = teamInfoFactory;
            _teamBlockInfoFactory = teamBlockInfoFactory;
            _blockProvider = blockProvider;
            _teamBlockScheduler = teamBlockScheduler;
            _lockableBitArrayFactory = lockableBitArrayFactory;
            _scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
            _schedulingOptionsCreator = schedulingOptionsCreator;
            _stateHolder = stateHolder;
            _deleteAndResourceCalculateService = deleteAndResourceCalculateService;
        }

        public void Optimize(IList<IScheduleMatrixPro> allPersonMatrixList,
                             DateOnlyPeriod selectedPeriod,
                             IList<IPerson> selectedPersons,
                             IOptimizationPreferences optimizationPreferences,
                             ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
        {
            ISchedulingOptions schedulingOptions =
                _schedulingOptionsCreator.CreateSchedulingOptions(optimizationPreferences);

            var dataExtractorValuesForMatrixes = new DataExtractorValuesForMatrixes();
            foreach (var scheduleMatrixPro in allPersonMatrixList)
            {
                var scheduleResultDataExtractor =
                    new RelativeDailyStandardDeviationsByAllSkillsExtractor(scheduleMatrixPro, schedulingOptions);
                dataExtractorValuesForMatrixes.Add(scheduleMatrixPro, scheduleResultDataExtractor);
            }

            var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
            foreach (var selectedPerson in selectedPersons)
            {
                allTeamInfoListOnStartDate.Add(_teamInfoFactory.CreateTeamInfo(selectedPerson, selectedPeriod,
                                                                               allPersonMatrixList));
            }
            var remainingInfoList = new List<ITeamInfo>(allTeamInfoListOnStartDate);

            var allTeamBlocks = new List<ITeamBlockInfo>();


            foreach (var teamInfo in remainingInfoList.GetRandom(remainingInfoList.Count, true))
            {
               var decisionMaker = new TeamBlockIntradayDecisionMaker(_lockableBitArrayFactory, _blockProvider);
                    var block = decisionMaker.Decide(selectedPeriod, selectedPersons, allPersonMatrixList,dataExtractorValuesForMatrixes,
                                                     schedulingOptions, optimizationPreferences);

                    //clear block
                    foreach (var dateOnly in block.BlockPeriod.DayCollection())
                    {
                        IList<IScheduleDay> toRemove = new List<IScheduleDay>();
                        foreach (var person in teamInfo.GroupPerson.GroupMembers)
                        {
                            IScheduleDay scheduleDay = _stateHolder.Schedules[person].ScheduledDay(dateOnly);
                            SchedulePartView significant = scheduleDay.SignificantPart();
                            if (significant != SchedulePartView.FullDayAbsence && significant != SchedulePartView.DayOff &&
                                significant != SchedulePartView.ContractDayOff)
                                toRemove.Add(scheduleDay);
                        }
                        _deleteAndResourceCalculateService.DeleteWithResourceCalculation(toRemove,
                                                                                         schedulePartModifyAndRollbackService,
                                                                                         schedulingOptions
                                                                                             .ConsiderShortBreaks);
                    }

                    var datePoint = block.BlockPeriod.DayCollection().First();
                    ITeamBlockInfo teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, datePoint,
                                                                                             schedulingOptions
                                                                                                 .BlockFinderTypeForAdvanceScheduling);
                    _teamBlockScheduler.ScheduleTeamBlock(teamBlockInfo, datePoint, schedulingOptions);
            }
        }
    }

}
