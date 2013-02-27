using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
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
        private readonly IRestrictionAggregator _restrictionAggregator;
        private readonly IWorkShiftFilterService _workShiftFilterService;
        private readonly ITeamScheduling _teamScheduling;
        private readonly ISchedulingOptions _schedulingOptions;
    	private readonly IWorkShiftSelector _workShiftSelector;
        private readonly ISkillDayPeriodIntervalDataGenerator _skillDayPeriodIntervalDataGenerator;
	    private bool _cancelMe;

	    public AdvanceSchedulingService(ISkillDayPeriodIntervalDataGenerator skillDayPeriodIntervalDataGenerator,
            IRestrictionAggregator restrictionAggregator,
            IWorkShiftFilterService workShiftFilterService,
            ITeamScheduling teamScheduling,
            ISchedulingOptions schedulingOptions,
			IWorkShiftSelector workShiftSelector,
			ITeamInfoCreator teamInfoCreator,
			ITeamBlockInfoFactory teamBlockInfoFactory
            )
        {
		    _teamInfoCreator = teamInfoCreator;
		    _teamBlockInfoFactory = teamBlockInfoFactory;
            _restrictionAggregator = restrictionAggregator;
            _workShiftFilterService = workShiftFilterService;
            _teamScheduling = teamScheduling;
            _schedulingOptions = schedulingOptions;
        	_workShiftSelector = workShiftSelector;
            _skillDayPeriodIntervalDataGenerator = skillDayPeriodIntervalDataGenerator;
        }

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

	    public bool Execute3(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, TeamSteadyStateHolder teamSteadyStateHolder)
	    {
		    _teamScheduling.DayScheduled += dayScheduled;
		    foreach (var datePointer in selectedPeriod.DayCollection())
		    {
				var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			    foreach (var selectedPerson in selectedPersons)
			    {
				    allTeamInfoListOnStartDate.Add(_teamInfoCreator.CreateTeamInfo(selectedPerson, datePointer, allPersonMatrixList));
			    }

				foreach (var teamInfo in allTeamInfoListOnStartDate.GetRandom(allTeamInfoListOnStartDate.Count, true))
				{

					ITeamBlockInfo teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, datePointer,
					                                                                         _schedulingOptions
						                                                                         .BlockFinderTypeForAdvanceScheduling);
					if (teamBlockInfo == null)
						continue;

					//if teamBlockInfo is fully scheduled, continue;

					//change signature
					var restriction = _restrictionAggregator.Aggregate(teamBlockInfo.BlockInfo.BlockPeriod.DayCollection(),
					                                                   teamBlockInfo.TeamInfo.GroupPerson,
					                                                   teamBlockInfo.TeamInfo.MatrixesForGroup.ToList(),
					                                                   _schedulingOptions);

					// (should we cover for max seats here?) ????
					//change signature
					var shifts = _workShiftFilterService.Filter(datePointer, teamBlockInfo.TeamInfo.GroupPerson,
					                                            teamBlockInfo.TeamInfo.MatrixesForGroup.ToList(), restriction,
					                                            _schedulingOptions);
					if (shifts == null || shifts.Count <= 0)
						continue;

					//change signature
					var activityInternalData = _skillDayPeriodIntervalDataGenerator.Generate(teamBlockInfo.TeamInfo.GroupPerson,
					                                                                         teamBlockInfo.BlockInfo.BlockPeriod
					                                                                                      .DayCollection());

					var bestShiftProjectionCache = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
																								 _schedulingOptions
																									 .WorkShiftLengthHintOption,
																								 _schedulingOptions
																									 .UseMinimumPersons,
																								 _schedulingOptions
																									 .UseMaximumPersons);
					//implement
					_teamScheduling.Execute(teamBlockInfo, bestShiftProjectionCache);

					if (_cancelMe)
						break;
				}
		    }

			_teamScheduling.DayScheduled -= dayScheduled;
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
    }
}