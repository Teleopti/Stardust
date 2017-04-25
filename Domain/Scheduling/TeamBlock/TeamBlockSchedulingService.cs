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

    public interface ITeamBlockSchedulingService
    {
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		IWorkShiftFinderResultHolder ScheduleSelected(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
	                          IList<IPerson> selectedPersons,
	                          ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
	                          IResourceCalculateDelayer resourceCalculateDelayer,
	                          ISchedulingResultStateHolder schedulingResultStateHolder);
    }

    public class TeamBlockSchedulingService : ITeamBlockSchedulingService
    {
	    private readonly ITeamInfoFactory _teamInfoFactory;
	    private readonly ITeamBlockScheduler _teamBlockScheduler;
	    private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
	    private readonly SchedulingOptions _schedulingOptions;
	    private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
        private readonly IValidatedTeamBlockInfoExtractor  _validatedTeamBlockExtractor;
	    private readonly ITeamMatrixChecker _teamMatrixChecker;
	    private readonly IWorkShiftSelector _workShiftSelector;
	    private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;

	    public TeamBlockSchedulingService
		    (SchedulingOptions schedulingOptions, ITeamInfoFactory teamInfoFactory, ITeamBlockScheduler teamBlockScheduler,
			    ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			    IWorkShiftMinMaxCalculator workShiftMinMaxCalculator,
			    IValidatedTeamBlockInfoExtractor validatedTeamBlockExtractor,
			ITeamMatrixChecker teamMatrixChecker,
			IWorkShiftSelector workShiftSelector,
			IGroupPersonSkillAggregator groupPersonSkillAggregator)
	    {
		    _teamInfoFactory = teamInfoFactory;
		    _teamBlockScheduler = teamBlockScheduler;
		    _safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
		    _workShiftMinMaxCalculator = workShiftMinMaxCalculator;
		    _validatedTeamBlockExtractor = validatedTeamBlockExtractor;
		    _teamMatrixChecker = teamMatrixChecker;
		    _workShiftSelector = workShiftSelector;
		    _groupPersonSkillAggregator = groupPersonSkillAggregator;
		    _schedulingOptions = schedulingOptions;
	    }

	    public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public IWorkShiftFinderResultHolder ScheduleSelected(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
	                                 IList<IPerson> selectedPersons,
	                                 ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
	                                 IResourceCalculateDelayer resourceCalculateDelayer,
										ISchedulingResultStateHolder schedulingResultStateHolder)
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

			    var allTeamInfoListOnStartDate = getAllTeamInfoList(schedulingResultStateHolder, allPersonMatrixList, selectedPeriod, selectedPersons);

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
			                                           resourceCalculateDelayer, schedulingResultStateHolder, () => cancelMe);
		    }

		    _teamBlockScheduler.DayScheduled -= dayScheduled;
			return workShiftFinderResultHolder;
	    }

	    private void runSchedulingForAllTeamInfoOnStartDate(IList<IScheduleMatrixPro> allPersonMatrixList, IList<IPerson> selectedPersons, DateOnlyPeriod selectedPeriod,
                                     ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
                                     IEnumerable<ITeamInfo> allTeamInfoListOnStartDate, DateOnly datePointer, List<DateOnly> dateOnlySkipList,
									 IResourceCalculateDelayer resourceCalculateDelayer,
									 ISchedulingResultStateHolder schedulingResultStateHolder, Func<bool> isCancelled)
	    {
			var cancel = false;
            foreach (var teamInfo in allTeamInfoListOnStartDate.GetRandom(allTeamInfoListOnStartDate.Count(), true))
            {
				var teamBlockInfo = _validatedTeamBlockExtractor.GetTeamBlockInfo(teamInfo, datePointer, allPersonMatrixList, _schedulingOptions, selectedPeriod);
                if (teamBlockInfo == null) continue;

                schedulePartModifyAndRollbackService.ClearModificationCollection();
	            if (_teamBlockScheduler.ScheduleTeamBlockDay(_workShiftSelector, teamBlockInfo, datePointer, _schedulingOptions,
	                                                          schedulePartModifyAndRollbackService,
	                                                         resourceCalculateDelayer, schedulingResultStateHolder.AllSkillDays(), schedulingResultStateHolder.Schedules, new ShiftNudgeDirective(), NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder), _groupPersonSkillAggregator))
		            verifyScheduledTeamBlock(selectedPersons, schedulePartModifyAndRollbackService, datePointer,
		                                     dateOnlySkipList, teamBlockInfo, isCancelled);
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
                                              DateOnly datePointer, List<DateOnly> dateOnlySkipList, ITeamBlockInfo teamBlockInfo, Func<bool> isCancelled)
        {
	        var dayCollection = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
	        foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(datePointer))
            {
                if (isCancelled()) break;

                if (!selectedPersons.Contains(matrix.Person)) continue;
                _workShiftMinMaxCalculator.ResetCache();
                if (!_workShiftMinMaxCalculator.IsPeriodInLegalState(matrix, _schedulingOptions))
                {
                    executeRollback(schedulePartModifyAndRollbackService);
                    dateOnlySkipList.AddRange(dayCollection);
                    break;
                }
            }
        }
        

        private void executeRollback(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
        {
            _safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService,
                                                        _schedulingOptions);
        }

        private HashSet<ITeamInfo> getAllTeamInfoList(ISchedulingResultStateHolder schedulingResultStateHolder, IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons)
        {
            var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
            foreach (var selectedPerson in selectedPersons)
            {
                var teamInfo = _teamInfoFactory.CreateTeamInfo(schedulingResultStateHolder.PersonsInOrganization, selectedPerson, selectedPeriod, allPersonMatrixList);
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