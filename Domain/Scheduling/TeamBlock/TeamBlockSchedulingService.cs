using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
	    private readonly ISchedulingOptions _schedulingOptions;
	    private bool _cancelMe;
        private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
        private readonly ITeamBlockMaxSeatChecker  _teamBlockMaxSeat;
        private readonly IValidatedTeamBlockInfoExtractor  _validatedTeamBlockExtractor;
	    private readonly ITeamMatrixChecker _teamMatrixChecker;
	    private SchedulingServiceBaseEventArgs _progressEvent;

	    public TeamBlockSchedulingService
		    (ISchedulingOptions schedulingOptions, ITeamInfoFactory teamInfoFactory, ITeamBlockScheduler teamBlockScheduler,
			    ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			    IWorkShiftMinMaxCalculator workShiftMinMaxCalculator, ITeamBlockMaxSeatChecker teamBlockMaxSeat,
			    IValidatedTeamBlockInfoExtractor validatedTeamBlockExtractor,
			ITeamMatrixChecker teamMatrixChecker)
	    {
		    _teamInfoFactory = teamInfoFactory;
		    _teamBlockScheduler = teamBlockScheduler;
		    _safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
		    _workShiftMinMaxCalculator = workShiftMinMaxCalculator;
		    _teamBlockMaxSeat = teamBlockMaxSeat;
		    _validatedTeamBlockExtractor = validatedTeamBlockExtractor;
		    _teamMatrixChecker = teamMatrixChecker;
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
		    _teamBlockScheduler.DayScheduled += dayScheduled;
		    if (schedulePartModifyAndRollbackService == null)
		    {
				_progressEvent = null;
				return workShiftFinderResultHolder;    
		    }

		    var dateOnlySkipList = new List<DateOnly>();
		    foreach (var datePointer in selectedPeriod.DayCollection())
		    {
			    if (_cancelMe)
				    break;

			    if (_progressEvent != null && _progressEvent.UserCancel)
				    break;

			    if (dateOnlySkipList.Contains(datePointer))
				    continue;

			    var allTeamInfoListOnStartDate = getAllTeamInfoList(allPersonMatrixList, selectedPeriod, selectedPersons);

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
			                                           resourceCalculateDelayer, schedulingResultStateHolder);
		    }

		    _progressEvent = null;
		    _teamBlockScheduler.DayScheduled -= dayScheduled;
			return workShiftFinderResultHolder;
	    }

	    private void runSchedulingForAllTeamInfoOnStartDate(IList<IScheduleMatrixPro> allPersonMatrixList, IList<IPerson> selectedPersons, DateOnlyPeriod selectedPeriod,
                                     ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
                                     IEnumerable<ITeamInfo> allTeamInfoListOnStartDate, DateOnly datePointer, List<DateOnly> dateOnlySkipList,
									 IResourceCalculateDelayer resourceCalculateDelayer,
									 ISchedulingResultStateHolder schedulingResultStateHolder)
        {
            foreach (var teamInfo in allTeamInfoListOnStartDate.GetRandom(allTeamInfoListOnStartDate.Count(), true))
            {
				var teamBlockInfo = _validatedTeamBlockExtractor.GetTeamBlockInfo(teamInfo, datePointer, allPersonMatrixList, _schedulingOptions, selectedPeriod);
                if (teamBlockInfo == null) continue;

                schedulePartModifyAndRollbackService.ClearModificationCollection();
	            if (_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, datePointer, _schedulingOptions,
	                                                          schedulePartModifyAndRollbackService,
	                                                         resourceCalculateDelayer, schedulingResultStateHolder, new ShiftNudgeDirective()))
		            verifyScheduledTeamBlock(selectedPersons, schedulePartModifyAndRollbackService, datePointer,
		                                     dateOnlySkipList, teamBlockInfo);
				else
				{
					OnDayScheduledFailed();
					continue;
				}
                if (_cancelMe)
                    break;

	            if (_progressEvent != null && _progressEvent.UserCancel)
		            break;
            }
        }

        private void verifyScheduledTeamBlock(IList<IPerson> selectedPersons,
                                              ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
                                              DateOnly datePointer, List<DateOnly> dateOnlySkipList, ITeamBlockInfo teamBlockInfo)
        {
            foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(datePointer))
            {
                if (_cancelMe)
                    break;

	            if (_progressEvent != null && _progressEvent.UserCancel)
		            break;

                if (!selectedPersons.Contains(matrix.Person)) continue;
                _workShiftMinMaxCalculator.ResetCache();
                if (!_workShiftMinMaxCalculator.IsPeriodInLegalState(matrix, _schedulingOptions))
                {
                    executeRollback(schedulePartModifyAndRollbackService);
                    dateOnlySkipList.AddRange(teamBlockInfo.BlockInfo.BlockPeriod.DayCollection());
                    break;
                }
            }
	        foreach (var dateOnly in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
			  {
				  if (!_teamBlockMaxSeat.CheckMaxSeat(dateOnly, _schedulingOptions))
				  {
					  executeRollback(schedulePartModifyAndRollbackService);
					  dateOnlySkipList.AddRange(teamBlockInfo.BlockInfo.BlockPeriod.DayCollection());
					  break;
				  }
		        
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

	        foreach (var teamInfo in allTeamInfoListOnStartDate)
	        {
		        foreach (var groupMember in teamInfo.GroupMembers)
		        {
			        if(!selectedPersons.Contains(groupMember))
						teamInfo.LockMember(groupMember);
		        }
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

			if (_progressEvent != null && _progressEvent.UserCancel)
				return;

			_progressEvent = scheduleServiceBaseEventArgs;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        public void RaiseEventForTest(object sender, SchedulingServiceBaseEventArgs e)
        {
            dayScheduled(sender, e);
        }

		public void OnDayScheduledFailed()
		{
			var e = new SchedulingServiceFailedEventArgs();

			var temp = DayScheduled;
			if (temp != null)
			{
				temp(this, e);
			}
			_cancelMe = e.Cancel;

			if (_progressEvent != null && _progressEvent.UserCancel)
				return;

			_progressEvent = e;
		}
    }
}