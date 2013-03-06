using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{

    public interface IAdvanceSchedulingService
    {
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
		bool Execute3(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, TeamSteadyStateHolder teamSteadyStateHolder);
    }

    public class AdvanceSchedulingService : IAdvanceSchedulingService
    {
	    private readonly ITeamInfoCreator _teamInfoCreator;
	    private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
	    private readonly ITeamBlockScheduler _teamBlockScheduler;
        private readonly ISchedulingOptions _schedulingOptions;
	    private bool _cancelMe;

	    public AdvanceSchedulingService
		    (
		    ISchedulingOptions schedulingOptions,
		    ITeamInfoCreator teamInfoCreator,
		    ITeamBlockInfoFactory teamBlockInfoFactory,
		    ITeamBlockScheduler teamBlockScheduler
		    )
	    {
		    _teamInfoCreator = teamInfoCreator;
		    _teamBlockInfoFactory = teamBlockInfoFactory;
		    _teamBlockScheduler = teamBlockScheduler;
		    _schedulingOptions = schedulingOptions;
	    }

	    public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

	    public bool Execute3(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, TeamSteadyStateHolder teamSteadyStateHolder)
	    {
			_teamBlockScheduler.DayScheduled += dayScheduled;
		    foreach (var datePointer in selectedPeriod.DayCollection())
		    {
				var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			    foreach (var selectedPerson in selectedPersons)
			    {
				    allTeamInfoListOnStartDate.Add(_teamInfoCreator.CreateTeamInfo(selectedPerson, datePointer, allPersonMatrixList));
			    }

				foreach (var teamInfo in allTeamInfoListOnStartDate.GetRandom(allTeamInfoListOnStartDate.Count, true))
				{
					if (!teamSteadyStateHolder.IsSteadyState(teamInfo.GroupPerson))
						continue;

					ITeamBlockInfo teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, datePointer,
					                                                                         _schedulingOptions
						                                                                         .BlockFinderTypeForAdvanceScheduling);

					if (!_teamBlockScheduler.ScheduleTeamBlock(teamBlockInfo, datePointer, _schedulingOptions)) 
						continue;

					if (_cancelMe)
						break;
				}
		    }

			_teamBlockScheduler.DayScheduled -= dayScheduled;
		    return true;
	    }


		//extract to class
	    


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
    }
}