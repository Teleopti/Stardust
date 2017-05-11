using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
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
	    private readonly ITeamBlockScheduler _teamBlockScheduler;
	    private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
	    private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
        private readonly ValidatedTeamBlockInfoExtractor  _validatedTeamBlockExtractor;
	    private readonly ITeamMatrixChecker _teamMatrixChecker;
	    private readonly IWorkShiftSelector _workShiftSelector;
	    private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;

	    public TeamBlockSchedulingService(ITeamBlockScheduler teamBlockScheduler,
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

	    public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public IWorkShiftFinderResultHolder ScheduleSelected(IEnumerable<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
	                                 IList<IPerson> selectedPersons,
	                                 ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
	                                 IResourceCalculateDelayer resourceCalculateDelayer,
										ISchedulingResultStateHolder schedulingResultStateHolder,
										SchedulingOptions schedulingOption,
										ITeamInfoFactory teamInfoFactory)
		{
			var workShiftFinderResultHolder = new WorkShiftFinderResultHolder();
			var cancelMe = false;
			EventHandler<SchedulingServiceBaseEventArgs> dayScheduled = (sender, e) =>
			{
				var handler = DayScheduled;
				if (handler != null)
				{
					e.AppendCancelAction(()=>cancelMe=true);
					handler(this, e);
					if (e.Cancel) cancelMe = true;
				}
			};

			_teamBlockScheduler.DayScheduled += dayScheduled;
		    if (schedulePartModifyAndRollbackService == null)
		    {
				return workShiftFinderResultHolder;    
		    }

		    var dateOnlySkipList = new List<DateOnly>();
		    foreach (var datePointer in selectedPeriod.DayCollection())
		    {
			    if (cancelMe) break;

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
	
			    runSchedulingForAllTeamInfoOnStartDate(allPersonMatrixList, selectedPersons, selectedPeriod,
			                                           schedulePartModifyAndRollbackService,
													   checkedTeams.OkList, datePointer, dateOnlySkipList,
			                                           resourceCalculateDelayer, schedulingResultStateHolder, () => cancelMe, schedulingOption);
		    }

		    _teamBlockScheduler.DayScheduled -= dayScheduled;
			return workShiftFinderResultHolder;
	    }

	    private void runSchedulingForAllTeamInfoOnStartDate(IEnumerable<IScheduleMatrixPro> allPersonMatrixList, IList<IPerson> selectedPersons, DateOnlyPeriod selectedPeriod,
                                     ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
                                     IEnumerable<ITeamInfo> allTeamInfoListOnStartDate, DateOnly datePointer, List<DateOnly> dateOnlySkipList,
									 IResourceCalculateDelayer resourceCalculateDelayer,
									 ISchedulingResultStateHolder schedulingResultStateHolder, Func<bool> isCancelled,
									SchedulingOptions schedulingOption)
	    {
			var cancel = false;
            foreach (var teamInfo in allTeamInfoListOnStartDate.GetRandom(allTeamInfoListOnStartDate.Count(), true))
            {
				var teamBlockInfo = _validatedTeamBlockExtractor.GetTeamBlockInfo(teamInfo, datePointer, allPersonMatrixList, schedulingOption, selectedPeriod);
                if (teamBlockInfo == null) continue;

                schedulePartModifyAndRollbackService.ClearModificationCollection();
	            if (_teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelector, teamBlockInfo, datePointer, schedulingOption,
	                                                          schedulePartModifyAndRollbackService,
	                                                         resourceCalculateDelayer, schedulingResultStateHolder.AllSkillDays(), schedulingResultStateHolder.Schedules, new ShiftNudgeDirective(), NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder), _groupPersonSkillAggregator))
		            verifyScheduledTeamBlock(selectedPersons, schedulePartModifyAndRollbackService, datePointer,
		                                     dateOnlySkipList, teamBlockInfo, isCancelled, schedulingOption);
				else
				{
					var progressResult = onDayScheduledFailed(new SchedulingServiceFailedEventArgs(()=>cancel=true));
					if (cancel || progressResult.ShouldCancel) break;
 					continue;
				}
                if (cancel || isCancelled()) break;
            }
        }

		private void verifyScheduledTeamBlock(IList<IPerson> selectedPersons,
                                              ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
                                              DateOnly datePointer, List<DateOnly> dateOnlySkipList, ITeamBlockInfo teamBlockInfo, Func<bool> isCancelled,
			SchedulingOptions schedulingOption)
        {
	        var dayCollection = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
	        foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(datePointer))
            {
                if (isCancelled()) break;

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

        private HashSet<ITeamInfo> getAllTeamInfoList(ISchedulingResultStateHolder schedulingResultStateHolder, IEnumerable<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, ITeamInfoFactory teamInfoFactory)
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

		private CancelSignal onDayScheduledFailed(SchedulingServiceBaseEventArgs args)
		{
			var handler = DayScheduled;
			if (handler != null)
			{
				handler(this, args);
				if (args.Cancel)
					return new CancelSignal{ShouldCancel = true};
			}
			return new CancelSignal();
		}
    }
}