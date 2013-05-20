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
        private readonly IBlockSteadyStateValidator _blockSteadyStateValidator;
	    private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
	    private readonly ISchedulingOptions _schedulingOptions;
	    private bool _cancelMe;
        private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
        private readonly List<IWorkShiftFinderResult> _advanceSchedulingResults;

        public TeamBlockSchedulingService
		    (ISchedulingOptions schedulingOptions, ITeamInfoFactory teamInfoFactory, ITeamBlockInfoFactory teamBlockInfoFactory, ITeamBlockScheduler teamBlockScheduler, IBlockSteadyStateValidator blockSteadyStateValidator, ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation, IWorkShiftMinMaxCalculator workShiftMinMaxCalculator, List<IWorkShiftFinderResult> advanceSchedulingResults)
	    {
		    _teamInfoFactory = teamInfoFactory;
		    _teamBlockInfoFactory = teamBlockInfoFactory;
		    _teamBlockScheduler = teamBlockScheduler;
	        _blockSteadyStateValidator = blockSteadyStateValidator;
		    _safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
            _workShiftMinMaxCalculator = workShiftMinMaxCalculator;
            _advanceSchedulingResults = advanceSchedulingResults;
            _schedulingOptions = schedulingOptions;
	    }

	    public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

        
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public bool ScheduleSelected(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, ITeamSteadyStateHolder teamSteadyStateHolder,ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
	    {
			_teamBlockScheduler.DayScheduled += dayScheduled;
		    if (schedulePartModifyAndRollbackService == null) return false;
		    var dateOnlySkipList = new List<DateOnly>();
		    //foreach (var datePointer in selectedPeriod.DayCollection())
            foreach (var datePointer in selectedPeriod.DayCollection())
            {
                if (dateOnlySkipList.Contains(datePointer)) continue;
                var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			    foreach (var selectedPerson in selectedPersons)
			    {
                    allTeamInfoListOnStartDate.Add(_teamInfoFactory.CreateTeamInfo(selectedPerson, selectedPeriod, allPersonMatrixList));
			    }

				foreach (var teamInfo in allTeamInfoListOnStartDate.GetRandom(allTeamInfoListOnStartDate.Count, true))
				{

				    if (teamInfo == null) continue;
                    if (!teamSteadyStateHolder.IsSteadyState(teamInfo.GroupPerson))
						continue;

                    bool singleAgentTeam = _schedulingOptions.GroupOnGroupPageForTeamBlockPer != null &&
                                           _schedulingOptions.GroupOnGroupPageForTeamBlockPer.Key == "SingleAgentTeam";
				    ITeamBlockInfo teamBlockInfo;
                    if (_schedulingOptions.UseTeamBlockPerOption)
                        teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, datePointer,
					                                                                         _schedulingOptions
                                                                                                 .BlockFinderTypeForAdvanceScheduling, singleAgentTeam, allPersonMatrixList);
                    else
                        teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, datePointer,BlockFinderType.SingleDay, singleAgentTeam,allPersonMatrixList);
				    if (teamBlockInfo == null) continue;
                    if (TeamBlockScheduledDayChecker.IsDayScheduledInTeamBlock(teamBlockInfo, datePointer)) continue;

					
                    if (_blockSteadyStateValidator.IsBlockInSteadyState(teamBlockInfo, _schedulingOptions))
                    {
                        schedulePartModifyAndRollbackService.ClearModificationCollection();
                        if (_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, datePointer, _schedulingOptions,
                                                                      selectedPeriod, selectedPersons))
                        {
                            var rollbackExecuted = false;
                            foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(datePointer))
                            {
                                if (!selectedPersons.Contains(matrix.Person)) continue;
                                _workShiftMinMaxCalculator.ResetCache();
                                if (!_workShiftMinMaxCalculator.IsPeriodInLegalState(matrix, _schedulingOptions))
                                {
                                    var workShiftFinderResult = new WorkShiftFinderResult(teamInfo.GroupPerson, datePointer);
                                    workShiftFinderResult.AddFilterResults(new WorkShiftFilterResult(UserTexts.Resources.TeamBlockNotInLegalState, 0, 0));
                                    _advanceSchedulingResults.Add(workShiftFinderResult);

                                    _safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService,
                                                                                _schedulingOptions);
                                    rollbackExecuted = true;
                                    break;
                                }
                            }
                            if (rollbackExecuted)
                            {
                                //should skip the whole block
                                dateOnlySkipList.AddRange(teamBlockInfo.BlockInfo.BlockPeriod.DayCollection());
                                //break; Removed this to schedule all the remaining teams if this block failed.
                            }
                                
                        }

                    }
                     
					if (_cancelMe)
						break;
				}
		    }

			_teamBlockScheduler.DayScheduled -= dayScheduled;
		    return true;
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