using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class TeamBlockSchedulingService
    {
	    private readonly TeamBlockScheduler _teamBlockScheduler;
	    private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
	    private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
        private readonly ValidatedTeamBlockInfoExtractor  _validatedTeamBlockExtractor;
	    private readonly ITeamMatrixChecker _teamMatrixChecker;
	    private readonly IWorkShiftSelector _workShiftSelector;
	    private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;

	    public TeamBlockSchedulingService(TeamBlockScheduler teamBlockScheduler,
			    ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			    IWorkShiftMinMaxCalculator workShiftMinMaxCalculator,
			    ValidatedTeamBlockInfoExtractor validatedTeamBlockExtractor,
			ITeamMatrixChecker teamMatrixChecker,
			IWorkShiftSelector workShiftSelector,
			IGroupPersonSkillAggregator groupPersonSkillAggregator)
	    {
		    _teamBlockScheduler = teamBlockScheduler;
		    _safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
		    _workShiftMinMaxCalculator = workShiftMinMaxCalculator;
		    _validatedTeamBlockExtractor = validatedTeamBlockExtractor;
		    _teamMatrixChecker = teamMatrixChecker;
		    _workShiftSelector = workShiftSelector;
		    _groupPersonSkillAggregator = groupPersonSkillAggregator;
	    }

		public IWorkShiftFinderResultHolder ScheduleSelected(ISchedulingCallback schedulingCallback, IEnumerable<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
	                                 IEnumerable<IPerson> selectedPersons,
	                                 ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
	                                 IResourceCalculateDelayer resourceCalculateDelayer,
										ISchedulingResultStateHolder schedulingResultStateHolder,
										SchedulingOptions schedulingOption,
										ITeamInfoFactory teamInfoFactory)
		{
			var workShiftFinderResultHolder = new WorkShiftFinderResultHolder();

		    if (schedulePartModifyAndRollbackService == null)
		    {
				return workShiftFinderResultHolder;    
		    }

		    var dateOnlySkipList = new List<DateOnly>();
		    foreach (var datePointer in selectedPeriod.DayCollection())
		    {
			    if (schedulingCallback.IsCancelled) break;

				if (dateOnlySkipList.Contains(datePointer))
				    continue;

			    var allTeamInfoListOnStartDate = getAllTeamInfoList(schedulingResultStateHolder, allPersonMatrixList, selectedPeriod, selectedPersons, teamInfoFactory);

			    var checkedTeams = _teamMatrixChecker.CheckTeamList(allTeamInfoListOnStartDate, selectedPeriod);

			    foreach (var teamInfo in checkedTeams.BannedList)
			    {
				    foreach (var member in teamInfo.GroupMembers)
				    {
						workShiftFinderResultHolder.AddFilterToResult(member, datePointer, Resources.AllTeamMembersMustBeLoaded);
				    }   
			    }
	
			    runSchedulingForAllTeamInfoOnStartDate(schedulingCallback, allPersonMatrixList, selectedPersons, selectedPeriod,
			                                           schedulePartModifyAndRollbackService,
													   checkedTeams.OkList, datePointer, dateOnlySkipList,
			                                           resourceCalculateDelayer, schedulingResultStateHolder, schedulingOption);
		    }

			return workShiftFinderResultHolder;
	    }

	    private void runSchedulingForAllTeamInfoOnStartDate(ISchedulingCallback schedulingCallback, IEnumerable<IScheduleMatrixPro> allPersonMatrixList, IEnumerable<IPerson> selectedPersons, DateOnlyPeriod selectedPeriod,
                                     ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
                                     IEnumerable<ITeamInfo> allTeamInfoListOnStartDate, DateOnly datePointer, List<DateOnly> dateOnlySkipList,
									 IResourceCalculateDelayer resourceCalculateDelayer,
									 ISchedulingResultStateHolder schedulingResultStateHolder, 
									SchedulingOptions schedulingOption)
	    {
            foreach (var teamInfo in allTeamInfoListOnStartDate.GetRandom(allTeamInfoListOnStartDate.Count(), true))
            {
				var teamBlockInfo = _validatedTeamBlockExtractor.GetTeamBlockInfo(teamInfo, datePointer, allPersonMatrixList, schedulingOption, selectedPeriod);
                if (teamBlockInfo == null) continue;

                schedulePartModifyAndRollbackService.ClearModificationCollection();
	            //TODO: check assignmentthingy #45540
				if (_teamBlockScheduler.ScheduleTeamBlockDay(Enumerable.Empty<IPersonAssignment>(), schedulingCallback, _workShiftSelector, teamBlockInfo, datePointer, schedulingOption,
	                                                          schedulePartModifyAndRollbackService,
	                                                         resourceCalculateDelayer, schedulingResultStateHolder.AllSkillDays(), schedulingResultStateHolder.Schedules, new ShiftNudgeDirective(), NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder), _groupPersonSkillAggregator))
				{
					verifyScheduledTeamBlock(schedulingCallback, selectedPersons, schedulePartModifyAndRollbackService, datePointer,
		                                     dateOnlySkipList, teamBlockInfo, schedulingOption);
				}
				else
				{
					schedulingCallback.Scheduled(new SchedulingCallbackInfo(null, false));
				}
	            if (schedulingCallback.IsCancelled) break;
			}
        }

		private void verifyScheduledTeamBlock(ISchedulingCallback schedulingCallback, IEnumerable<IPerson> selectedPersons,
                                              ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
                                              DateOnly datePointer, List<DateOnly> dateOnlySkipList, ITeamBlockInfo teamBlockInfo, SchedulingOptions schedulingOption)
        {
	        var dayCollection = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
	        foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(datePointer))
            {
	            if (schedulingCallback.IsCancelled) break;

				if (!selectedPersons.Contains(matrix.Person)) continue;
                _workShiftMinMaxCalculator.ResetCache();
                if (!_workShiftMinMaxCalculator.IsPeriodInLegalState(matrix, schedulingOption))
                {
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOption);
					dateOnlySkipList.AddRange(dayCollection);
                    break;
                }
            }
        }

        private HashSet<ITeamInfo> getAllTeamInfoList(ISchedulingResultStateHolder schedulingResultStateHolder, IEnumerable<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedPersons, ITeamInfoFactory teamInfoFactory)
        {
            var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
            foreach (var selectedPerson in selectedPersons)
            {
                var teamInfo = teamInfoFactory.CreateTeamInfo(schedulingResultStateHolder.PersonsInOrganization, selectedPerson, selectedPeriod, allPersonMatrixList);
                if (teamInfo != null)
                    allTeamInfoListOnStartDate.Add(teamInfo);
            }

	        foreach (var teamInfo in allTeamInfoListOnStartDate)
	        {
		        foreach (var groupMember in teamInfo.GroupMembers)
		        {
			        if(!selectedPersons.Contains(groupMember))
						teamInfo.LockMember(selectedPeriod, groupMember);
		        }
	        }

            return allTeamInfoListOnStartDate;
        }
    }
}