using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
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

	    public TeamBlockSchedulingService
		    (
		    ISchedulingOptions schedulingOptions,
		    ITeamInfoFactory teamInfoFactory,
		    ITeamBlockInfoFactory teamBlockInfoFactory,
		    ITeamBlockScheduler teamBlockScheduler,
            IBlockSteadyStateValidator blockSteadyStateValidator,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation
		    )
	    {
		    _teamInfoFactory = teamInfoFactory;
		    _teamBlockInfoFactory = teamBlockInfoFactory;
		    _teamBlockScheduler = teamBlockScheduler;
	        _blockSteadyStateValidator = blockSteadyStateValidator;
		    _safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
		    _schedulingOptions = schedulingOptions;
	    }

	    public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public bool ScheduleSelected(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, ITeamSteadyStateHolder teamSteadyStateHolder,ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
	    {
			_teamBlockScheduler.DayScheduled += dayScheduled;
		    if (schedulePartModifyAndRollbackService == null) return false;
		    foreach (var datePointer in selectedPeriod.DayCollection())
		    {
				var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			    foreach (var selectedPerson in selectedPersons)
			    {
                    allTeamInfoListOnStartDate.Add(_teamInfoFactory.CreateTeamInfo(selectedPerson, selectedPeriod, allPersonMatrixList));
			    }

				foreach (var teamInfo in allTeamInfoListOnStartDate.GetRandom(allTeamInfoListOnStartDate.Count, true))
				{
					if (!teamSteadyStateHolder.IsSteadyState(teamInfo.GroupPerson))
						continue;

                    bool singleAgentTeam = _schedulingOptions.GroupOnGroupPageForLevelingPer != null &&
                                           _schedulingOptions.GroupOnGroupPageForLevelingPer.Key == "SingleAgentTeam";
				    ITeamBlockInfo teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, datePointer,
					                                                                         _schedulingOptions
                                                                                                 .BlockFinderTypeForAdvanceScheduling, singleAgentTeam);
				    if (teamBlockInfo == null) continue;
                    if (TeamBlockScheduledDayChecker.IsDayScheduledInTeamBlock(teamBlockInfo, datePointer)) continue;

					
                    if (_blockSteadyStateValidator.IsBlockInSteadyState(teamBlockInfo, _schedulingOptions))
                    {
                        schedulePartModifyAndRollbackService.ClearModificationCollection();
                        if (!_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, datePointer, _schedulingOptions,
                                                                      selectedPeriod, selectedPersons))
                        {
                            _safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService,
                                                                        _schedulingOptions);
                            continue;
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

        //private static bool isTeamBlockScheduled(ITeamBlockInfo teamBlockInfo, DateOnly dateOnly )
        //{
        //    IScheduleRange rangeForPerson = null;
        //    foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroup())
        //    {
        //        rangeForPerson = matrix.SchedulingStateHolder.Schedules[matrix.Person];
        //        break;
        //    }
        //    if (rangeForPerson == null) return false;

        //        IScheduleDay scheduleDay = rangeForPerson.ScheduledDay(dateOnly);
        //        if (!scheduleDay.IsScheduled())
        //            return false;


        //    return true;
        //}

      

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