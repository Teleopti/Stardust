using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{

    public interface ITeamBlockSchedulingService
    {
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
		bool ScheduleSelected(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, TeamSteadyStateHolder teamSteadyStateHolder);
    }

    public class TeamBlockSchedulingService : ITeamBlockSchedulingService
    {
	    private readonly ITeamInfoFactory _teamInfoFactory;
	    private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
	    private readonly ITeamBlockScheduler _teamBlockScheduler;
        private readonly ISchedulingOptions _schedulingOptions;
	    private bool _cancelMe;

	    public TeamBlockSchedulingService
		    (
		    ISchedulingOptions schedulingOptions,
		    ITeamInfoFactory teamInfoFactory,
		    ITeamBlockInfoFactory teamBlockInfoFactory,
		    ITeamBlockScheduler teamBlockScheduler
		    )
	    {
		    _teamInfoFactory = teamInfoFactory;
		    _teamBlockInfoFactory = teamBlockInfoFactory;
		    _teamBlockScheduler = teamBlockScheduler;
		    _schedulingOptions = schedulingOptions;
	    }

	    public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

	    public bool ScheduleSelected(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, TeamSteadyStateHolder teamSteadyStateHolder)
	    {
			_teamBlockScheduler.DayScheduled += dayScheduled;
		    foreach (var datePointer in selectedPeriod.DayCollection())
		    {
				var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			    foreach (var selectedPerson in selectedPersons)
			    {
				    allTeamInfoListOnStartDate.Add(_teamInfoFactory.CreateTeamInfo(selectedPerson, datePointer, allPersonMatrixList));
			    }

				foreach (var teamInfo in allTeamInfoListOnStartDate.GetRandom(allTeamInfoListOnStartDate.Count, true))
				{
					if (!teamSteadyStateHolder.IsSteadyState(teamInfo.GroupPerson))
						continue;

					ITeamBlockInfo teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, datePointer,
					                                                                         _schedulingOptions
						                                                                         .BlockFinderTypeForAdvanceScheduling);
                    if (isTeamBlockScheduled(teamBlockInfo)) continue;

                    if (!_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, datePointer, _schedulingOptions, selectedPeriod,selectedPersons))
                        continue;

				

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

        private bool isTeamBlockScheduled(ITeamBlockInfo teamBlockInfo)
        {
            foreach (var day in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
                foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(day))
                    if (!matrix.GetScheduleDayByKey(day).DaySchedulePart().IsScheduled())
                        return false;
            return true;
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
    }
}