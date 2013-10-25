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
		bool ScheduleSelected(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService);
    }

    public class TeamBlockSchedulingService : ITeamBlockSchedulingService
    {
	    private readonly ITeamInfoFactory _teamInfoFactory;
	    private readonly ITeamBlockScheduler _teamBlockScheduler;
	    private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
	    private readonly ISchedulingOptions _schedulingOptions;
	    private bool _cancelMe;
        private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
        private readonly List<IWorkShiftFinderResult> _advanceSchedulingResults;
        private readonly ITeamBlockMaxSeatChecker  _teamBlockMaxSeat;
        private readonly IValidatedTeamBlockInfoExtractor  _validatedTeamBlockExtractor;

        public TeamBlockSchedulingService
		    (ISchedulingOptions schedulingOptions, ITeamInfoFactory teamInfoFactory, ITeamBlockScheduler teamBlockScheduler,  ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation, IWorkShiftMinMaxCalculator workShiftMinMaxCalculator, List<IWorkShiftFinderResult> advanceSchedulingResults, ITeamBlockMaxSeatChecker teamBlockMaxSeat, IValidatedTeamBlockInfoExtractor validatedTeamBlockExtractor)
	    {
		    _teamInfoFactory = teamInfoFactory;
		    _teamBlockScheduler = teamBlockScheduler;
		    _safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
            _workShiftMinMaxCalculator = workShiftMinMaxCalculator;
            _advanceSchedulingResults = advanceSchedulingResults;
            _teamBlockMaxSeat = teamBlockMaxSeat;
            _validatedTeamBlockExtractor = validatedTeamBlockExtractor;
            _schedulingOptions = schedulingOptions;
	    }

	    public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public bool ScheduleSelected(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
        {
            _teamBlockScheduler.DayScheduled += dayScheduled;
            if (schedulePartModifyAndRollbackService == null) return false;
            var dateOnlySkipList = new List<DateOnly>();
            foreach (var datePointer in selectedPeriod.DayCollection())
            {
                if (dateOnlySkipList.Contains(datePointer)) continue;

                var allTeamInfoListOnStartDate = getAllTeamInfoList(allPersonMatrixList, selectedPeriod, selectedPersons);

                runSchedulingForAllTeamInfoOnStartDate(allPersonMatrixList, selectedPeriod, selectedPersons, schedulePartModifyAndRollbackService,
                                                       allTeamInfoListOnStartDate, datePointer, dateOnlySkipList);
            }

            _teamBlockScheduler.DayScheduled -= dayScheduled;
            return true;
        }

        private void runSchedulingForAllTeamInfoOnStartDate(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons,
                                     ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
                                     HashSet<ITeamInfo> allTeamInfoListOnStartDate, DateOnly datePointer, List<DateOnly> dateOnlySkipList)
        {
            foreach (var teamInfo in allTeamInfoListOnStartDate.GetRandom(allTeamInfoListOnStartDate.Count, true))
            {
                var teamBlockInfo = _validatedTeamBlockExtractor.GetTeamBlockInfo(teamInfo,datePointer, allPersonMatrixList,_schedulingOptions );
                if (teamBlockInfo == null) continue;

                schedulePartModifyAndRollbackService.ClearModificationCollection();
                if (_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, datePointer, _schedulingOptions,selectedPeriod, selectedPersons))
                    verfiyScheduledTeamBlock(selectedPersons, schedulePartModifyAndRollbackService, datePointer, dateOnlySkipList, teamBlockInfo, teamInfo);

                if (_cancelMe)
                    break;
            }
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