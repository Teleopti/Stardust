using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{

    public interface ITeamBlockSchedulingService
    {
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
		bool ScheduleSelected(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, ITeamSteadyStateHolder teamSteadyStateHolder, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService);
    }

    public class TeamBlockSchedulingService : ITeamBlockSchedulingService
    {
	    private readonly ITeamInfoFactory _teamInfoFactory;
	    private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
	    private readonly ITeamBlockScheduler _teamBlockScheduler;
        private readonly ITeamBlockSteadyStateValidator _teamBlockSteadyStateValidator;
	    private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
	    private readonly ISchedulingOptions _schedulingOptions;
	    private bool _cancelMe;
        private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
        private readonly List<IWorkShiftFinderResult> _advanceSchedulingResults;
        private readonly ITeamBlockMaxSeatChecker  _teamBlockMaxSeat;

        public TeamBlockSchedulingService
		    (ISchedulingOptions schedulingOptions, ITeamInfoFactory teamInfoFactory, ITeamBlockInfoFactory teamBlockInfoFactory, ITeamBlockScheduler teamBlockScheduler, ITeamBlockSteadyStateValidator teamBlockSteadyStateValidator, ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation, IWorkShiftMinMaxCalculator workShiftMinMaxCalculator, List<IWorkShiftFinderResult> advanceSchedulingResults, ITeamBlockMaxSeatChecker teamBlockMaxSeat)
	    {
		    _teamInfoFactory = teamInfoFactory;
		    _teamBlockInfoFactory = teamBlockInfoFactory;
		    _teamBlockScheduler = teamBlockScheduler;
	        _teamBlockSteadyStateValidator = teamBlockSteadyStateValidator;
		    _safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
            _workShiftMinMaxCalculator = workShiftMinMaxCalculator;
            _advanceSchedulingResults = advanceSchedulingResults;
            _teamBlockMaxSeat = teamBlockMaxSeat;
            _schedulingOptions = schedulingOptions;
	    }

	    public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

        
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public bool ScheduleSelected(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, ITeamSteadyStateHolder teamSteadyStateHolder,ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
		{
		    return ScheduleSelectedRefactored(allPersonMatrixList, selectedPeriod,selectedPersons, teamSteadyStateHolder,
		                               schedulePartModifyAndRollbackService);
		    //_teamBlockScheduler.DayScheduled += dayScheduled;
		    //if (schedulePartModifyAndRollbackService == null) return false;
		    //var dateOnlySkipList = new List<DateOnly>();
		    //foreach (var datePointer in selectedPeriod.DayCollection())
		    //{
		    //    if (dateOnlySkipList.Contains(datePointer)) continue;
		    //    var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
		    //    foreach (var selectedPerson in selectedPersons)
		    //    {
		    //        var teamInfo = _teamInfoFactory.CreateTeamInfo(selectedPerson, selectedPeriod, allPersonMatrixList);
		    //        if (teamInfo != null)
		    //            allTeamInfoListOnStartDate.Add(teamInfo);
		    //    }

		    //    foreach (var teamInfo in allTeamInfoListOnStartDate.GetRandom(allTeamInfoListOnStartDate.Count, true))
		    //    {

		    //        if (teamInfo == null) continue;
		    //        if (!teamSteadyStateHolder.IsSteadyState(teamInfo.GroupPerson))
		    //            continue;

		    //        bool singleAgentTeam = _schedulingOptions.GroupOnGroupPageForTeamBlockPer != null &&
		    //                               _schedulingOptions.GroupOnGroupPageForTeamBlockPer.Key == "SingleAgentTeam";
		    //        ITeamBlockInfo teamBlockInfo;
		    //        if (_schedulingOptions.UseTeamBlockPerOption)
		    //            teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, datePointer,
		    //                                                                                 _schedulingOptions
		    //                                                                                     .BlockFinderTypeForAdvanceScheduling, singleAgentTeam, allPersonMatrixList);
		    //        else
		    //            teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, datePointer,BlockFinderType.SingleDay, singleAgentTeam,allPersonMatrixList);
		    //        if (teamBlockInfo == null) continue;
		    //        if (TeamBlockScheduledDayChecker.IsDayScheduledInTeamBlock(teamBlockInfo, datePointer)) continue;


		    //        if (_teamBlockSteadyStateValidator.IsBlockInSteadyState(teamBlockInfo, _schedulingOptions))
		    //        {
		    //            schedulePartModifyAndRollbackService.ClearModificationCollection();
		    //            if (_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, datePointer, _schedulingOptions,
		    //                                                          selectedPeriod, selectedPersons))
		    //            {
		    //                var rollbackExecuted = false;
		    //                foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(datePointer))
		    //                {
		    //                    if (_cancelMe)
		    //                        break;

		    //                    if (!selectedPersons.Contains(matrix.Person)) continue;
		    //                    _workShiftMinMaxCalculator.ResetCache();
		    //                    if (!_workShiftMinMaxCalculator.IsPeriodInLegalState(matrix, _schedulingOptions))
		    //                    {
		    //                        var workShiftFinderResult = new WorkShiftFinderResult(teamInfo.GroupPerson, datePointer);
		    //                        workShiftFinderResult.AddFilterResults(new WorkShiftFilterResult(UserTexts.Resources.TeamBlockNotInLegalState, 0, 0));
		    //                        _advanceSchedulingResults.Add(workShiftFinderResult);

		    //                        _safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService,
		    //                                                                    _schedulingOptions);
		    //                        rollbackExecuted = true;
		    //                        break;
		    //                    }
		    //                }

		    //                if (!_teamBlockMaxSeat.CheckMaxSeat(datePointer, _schedulingOptions))
		    //                {
		    //                   _safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService,
		    //                                                                    _schedulingOptions);
		    //                    rollbackExecuted = true;
		    //                }

		    //                if (rollbackExecuted)
		    //                {
		    //                    //should skip the whole block
		    //                    dateOnlySkipList.AddRange(teamBlockInfo.BlockInfo.BlockPeriod.DayCollection());
		    //                    //break; Removed this to schedule all the remaining teams if this block failed.
		    //                }
		    //            }
		    //        }


		    //        if (_cancelMe)
		    //            break;
		    //    }
		//}

			//_teamBlockScheduler.DayScheduled -= dayScheduled;
		    //return true;
	    }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public bool ScheduleSelectedRefactored(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, ITeamSteadyStateHolder teamSteadyStateHolder, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
        {
            _teamBlockScheduler.DayScheduled += dayScheduled;
            if (schedulePartModifyAndRollbackService == null) return false;
            var dateOnlySkipList = new List<DateOnly>();
            foreach (var datePointer in selectedPeriod.DayCollection())
            {
                if (dateOnlySkipList.Contains(datePointer)) continue;

                var allTeamInfoListOnStartDate = getAllTeamInfoList(allPersonMatrixList, selectedPeriod, selectedPersons);

                runSchedulingForAllTeamInfoOnStartDate(allPersonMatrixList, selectedPeriod, selectedPersons,
                                                       teamSteadyStateHolder, schedulePartModifyAndRollbackService,
                                                       allTeamInfoListOnStartDate, datePointer, dateOnlySkipList);
            }

            _teamBlockScheduler.DayScheduled -= dayScheduled;
            return true;
        }

        private void runSchedulingForAllTeamInfoOnStartDate(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
                                     ITeamSteadyStateHolder teamSteadyStateHolder,
                                     ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
                                     HashSet<ITeamInfo> allTeamInfoListOnStartDate, DateOnly datePointer, List<DateOnly> dateOnlySkipList)
        {
            foreach (var teamInfo in allTeamInfoListOnStartDate.GetRandom(allTeamInfoListOnStartDate.Count, true))
            {
                var teamBlockInfo = getTeamBlockInfo(teamInfo,datePointer, allPersonMatrixList,teamSteadyStateHolder);
                if (teamBlockInfo == null) continue;

                schedulePartModifyAndRollbackService.ClearModificationCollection();
                if (_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, datePointer, _schedulingOptions,selectedPeriod, selectedPersons))
                    verfiyScheduledTeamBlock(selectedPersons, schedulePartModifyAndRollbackService, datePointer, dateOnlySkipList, teamBlockInfo, teamInfo);

                if (_cancelMe)
                    break;
            }
        }

        private ITeamBlockInfo getTeamBlockInfo(ITeamInfo teamInfo, DateOnly datePointer, IList<IScheduleMatrixPro> allPersonMatrixList, ITeamSteadyStateHolder teamSteadyStateHolder)
        {
            if (teamInfo == null) return null ;
            if (!teamSteadyStateHolder.IsSteadyState(teamInfo.GroupPerson)) return null;
            var teamBlockInfo = createTeamBlockInfo(allPersonMatrixList, datePointer, teamInfo);
            if (teamBlockInfo == null) return null;
            if (TeamBlockScheduledDayChecker.IsDayScheduledInTeamBlock(teamBlockInfo, datePointer))
                return null;
            if (!_teamBlockSteadyStateValidator.IsBlockInSteadyState(teamBlockInfo, _schedulingOptions))
                return null;
            return teamBlockInfo;
        }

        private void verfiyScheduledTeamBlock(IList<IPerson> selectedPersons,
                                              ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
                                              DateOnly datePointer, List<DateOnly> dateOnlySkipList, ITeamBlockInfo teamBlockInfo,
                                              ITeamInfo teamInfo)
        {
            foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(datePointer))
            {
                if (_cancelMe)
                    break;

                if (!selectedPersons.Contains(matrix.Person)) continue;
                _workShiftMinMaxCalculator.ResetCache();
                if (!_workShiftMinMaxCalculator.IsPeriodInLegalState(matrix, _schedulingOptions))
                {
                    var workShiftFinderResult = new WorkShiftFinderResult(teamInfo.GroupPerson, datePointer);
                    workShiftFinderResult.AddFilterResults(
                        new WorkShiftFilterResult(UserTexts.Resources.TeamBlockNotInLegalState, 0, 0));
                    _advanceSchedulingResults.Add(workShiftFinderResult);

                    executeRollback(schedulePartModifyAndRollbackService);
                    dateOnlySkipList.AddRange(teamBlockInfo.BlockInfo.BlockPeriod.DayCollection());
                    break;
                }
            }

            if (!_teamBlockMaxSeat.CheckMaxSeat(datePointer, _schedulingOptions))
            {
                executeRollback(schedulePartModifyAndRollbackService);
                dateOnlySkipList.AddRange(teamBlockInfo.BlockInfo.BlockPeriod.DayCollection());
            }

        }
        

        private void executeRollback(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
        {
            _safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService,
                                                        _schedulingOptions);
        }

        private ITeamBlockInfo createTeamBlockInfo(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnly datePointer, ITeamInfo teamInfo)
        {
            bool singleAgentTeam = _schedulingOptions.GroupOnGroupPageForTeamBlockPer != null &&
                                       _schedulingOptions.GroupOnGroupPageForTeamBlockPer.Key == "SingleAgentTeam";
            ITeamBlockInfo teamBlockInfo;
            if (_schedulingOptions.UseTeamBlockPerOption)
                teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, datePointer,
                                                                          _schedulingOptions
                                                                              .BlockFinderTypeForAdvanceScheduling,
                                                                          singleAgentTeam, allPersonMatrixList);
            else
                teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, datePointer, BlockFinderType.SingleDay,
                                                                          singleAgentTeam, allPersonMatrixList);
            return teamBlockInfo;
        }

        private HashSet<ITeamInfo> getAllTeamInfoList(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons)
        {
            var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
            foreach (var selectedPerson in selectedPersons)
            {
                var teamInfo = _teamInfoFactory.CreateTeamInfo(selectedPerson, selectedPeriod, allPersonMatrixList);
                if (teamInfo != null)
                    allTeamInfoListOnStartDate.Add(teamInfo);
            }
            return allTeamInfoListOnStartDate;
        }


        void dayScheduled(object sender, SchedulingServiceBaseEventArgs e)
	    {
		    OnDayScheduled(e);
	    }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs scheduleServiceBaseEventArgs)
		{
			EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
			if (temp != null)
			{
				temp(this, scheduleServiceBaseEventArgs);
			}
			_cancelMe = scheduleServiceBaseEventArgs.Cancel;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        public void RaiseEventForTest(object sender, SchedulingServiceBaseEventArgs e)
        {
            dayScheduled(sender, e);
        }
    }
}