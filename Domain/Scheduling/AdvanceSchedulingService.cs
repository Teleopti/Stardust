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
        bool Execute(IDictionary<string, IWorkShiftFinderResult> workShiftFinderResultList, IList<IScheduleMatrixPro> allPersonMatrixList, IList<IScheduleMatrixPro> selectedPersonMatrixList,TeamSteadyStateHolder teamSteadyStateHolder );
		bool Execute3(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, TeamSteadyStateHolder teamSteadyStateHolder);
    }
    public class AdvanceSchedulingService : IAdvanceSchedulingService
    {
        private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
	    private readonly ITeamInfoCreator _teamInfoCreator;
	    private readonly IDynamicBlockFinder _dynamicBlockFinder;
        private readonly IRestrictionAggregator _restrictionAggregator;
        private readonly IWorkShiftFilterService _workShiftFilterService;
        private readonly ITeamScheduling _teamScheduling;
        private readonly ISchedulingOptions _schedulingOptions;
    	private readonly IWorkShiftSelector _workShiftSelector;
        private readonly IGroupPersonBuilderBasedOnContractTime _groupPersonBuilderBasedOnContractTime;
        private readonly ISkillDayPeriodIntervalDataGenerator _skillDayPeriodIntervalDataGenerator;
	    private bool _cancelMe;


	    public AdvanceSchedulingService(ISkillDayPeriodIntervalDataGenerator skillDayPeriodIntervalDataGenerator,
            IDynamicBlockFinder dynamicBlockFinder, 
            IRestrictionAggregator restrictionAggregator,
            IWorkShiftFilterService workShiftFilterService,
            ITeamScheduling teamScheduling,
            ISchedulingOptions schedulingOptions,
			IWorkShiftSelector workShiftSelector,
            IGroupPersonBuilderBasedOnContractTime groupPersonBuilderBasedOnContractTime ,
            IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization,
			ITeamInfoCreator teamInfoCreator
            )
        {
            _groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
		    _teamInfoCreator = teamInfoCreator;
		    _dynamicBlockFinder = dynamicBlockFinder;
            _restrictionAggregator = restrictionAggregator;
            _workShiftFilterService = workShiftFilterService;
            _teamScheduling = teamScheduling;
            _schedulingOptions = schedulingOptions;
        	_workShiftSelector = workShiftSelector;
            _groupPersonBuilderBasedOnContractTime = groupPersonBuilderBasedOnContractTime;
            _skillDayPeriodIntervalDataGenerator = skillDayPeriodIntervalDataGenerator;
        }


		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")
	    ]
	    public bool Execute(IDictionary<string, IWorkShiftFinderResult> workShiftFinderResultList,
	                        IList<IScheduleMatrixPro> allPersonMatrixList,
	                        IList<IScheduleMatrixPro> selectedPersonMatrixList, TeamSteadyStateHolder teamSteadyStateHolder)
	    {
            //_teamScheduling.DayScheduled += dayScheduled;
            //List<DateOnly> dayOff, effectiveDays, unLockedDays;

            //var startDate = retrieveStartDate(_schedulingOptions.BlockFinderTypeForAdvanceScheduling, selectedPersonMatrixList,
            //                                  out dayOff, out effectiveDays, out unLockedDays);

            //var selectedPerson = selectedPersonMatrixList.Select(scheduleMatrixPro => scheduleMatrixPro.Person).Distinct().ToList();

            //while (startDate != DateOnly.MinValue)
            //{
            //    //call class that return the teamblock dates for a given date (problem if team members don't have same days off)
            //    var dateOnlyList = _dynamicBlockFinder.ExtractBlockDays(startDate, new GroupPerson(new List<IPerson>( ),new DateOnly(),"",Guid.NewGuid()  ) );

            //    var allGroupPersonListOnStartDate = new HashSet<IGroupPerson>();

            //    foreach (var person in selectedPerson)
            //    {
            //        allGroupPersonListOnStartDate.Add(_groupPersonBuilderForOptimization.BuildGroupPerson(person, startDate));
            //    }

            //    foreach (var fullGroupPerson in allGroupPersonListOnStartDate.GetRandom(allGroupPersonListOnStartDate.Count, true))
            //    {
            //        if (!teamSteadyStateHolder.IsSteadyState(fullGroupPerson)) 
            //            continue;

            //        var groupPersonList = _groupPersonBuilderBasedOnContractTime.SplitTeams(fullGroupPerson, startDate);
				    
            //        foreach (var groupPerson in groupPersonList)
            //        {
            //            var groupMatrixList = getScheduleMatrixProList(groupPerson, startDate, allPersonMatrixList);
            //            var restriction = _restrictionAggregator.Aggregate(dateOnlyList, groupPerson, groupMatrixList,
            //                                                               _schedulingOptions);

            //            // (should we cover for max seats here?) ????
            //            var shifts = _workShiftFilterService.Filter(startDate, groupPerson, groupMatrixList, restriction,
            //                                                        _schedulingOptions);
            //            if (shifts == null || shifts.Count <= 0)
            //                continue;

            //            var activityInternalData = _skillDayPeriodIntervalDataGenerator.Generate(fullGroupPerson, dateOnlyList);
            //            var bestShiftProjectionCache = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
            //                                                                                         _schedulingOptions
            //                                                                                             .WorkShiftLengthHintOption,
            //                                                                                         _schedulingOptions
            //                                                                                             .UseMinimumPersons,
            //                                                                                         _schedulingOptions
            //                                                                                             .UseMaximumPersons);

            //            _teamScheduling.Execute(startDate, dateOnlyList, groupMatrixList, groupPerson,
            //                                    bestShiftProjectionCache, unLockedDays, selectedPerson);
            //            if (_cancelMe)
            //                break;
            //        }


            //        if (_cancelMe)
            //            break;
            //    }
            //    if (_cancelMe)
            //        break;
            //    startDate = getNextDate(dateOnlyList.OrderByDescending(x => x.Date).First(), effectiveDays);

            //}

            //_teamScheduling.DayScheduled -= dayScheduled;
		    return true;
	    }

	    public bool Execute3(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IList<IPerson> selectedPersons, TeamSteadyStateHolder teamSteadyStateHolder)
	    {
		    foreach (var datePointer in selectedPeriod.DayCollection())
		    {
				var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			    foreach (var selectedPerson in selectedPersons)
			    {
				    allTeamInfoListOnStartDate.Add(_teamInfoCreator.CreateTeamInfo(selectedPerson, datePointer, allPersonMatrixList));
			    }

				foreach (var teamInfo in allTeamInfoListOnStartDate.GetRandom(allTeamInfoListOnStartDate.Count, true))
				{
					//Create new class that returns a TeamBlockInfo
					BlockInfo blockInfo = _dynamicBlockFinder.ExtractBlockInfo(datePointer, teamInfo,
					                                                           _schedulingOptions.BlockFinderTypeForAdvanceScheduling);
					ITeamBlockInfo teamBlockInfo = new TeamBlockInfo(teamInfo, blockInfo);

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

		    return true;
	    }


	    public bool Execute2(IDictionary<string, IWorkShiftFinderResult> workShiftFinderResultList,
                    IList<IScheduleMatrixPro> allPersonMatrixList,
                    IList<IScheduleMatrixPro> selectedPersonMatrixList, TeamSteadyStateHolder teamSteadyStateHolder)
        {
            _teamScheduling.DayScheduled += dayScheduled;
            List<DateOnly> dayOff, effectiveDays, unLockedDays;

            var startDate = retrieveStartDate(_schedulingOptions.BlockFinderTypeForAdvanceScheduling, selectedPersonMatrixList,
                                              out dayOff, out effectiveDays, out unLockedDays);

            var selectedPerson = selectedPersonMatrixList.Select(scheduleMatrixPro => scheduleMatrixPro.Person).Distinct().ToList();

            while (startDate != DateOnly.MinValue)
            {
                //call class that return the teamblock dates for a given date (problem if team members don't have same days off)
                
                var allGroupPersonListOnStartDate = new HashSet<IGroupPerson>();

                foreach (var person in selectedPerson)
                {
                    allGroupPersonListOnStartDate.Add(_groupPersonBuilderForOptimization.BuildGroupPerson(person, startDate));
                }

                foreach (var fullGroupPerson in allGroupPersonListOnStartDate.GetRandom(allGroupPersonListOnStartDate.Count, true))
                {
                    
                    if (!teamSteadyStateHolder.IsSteadyState(fullGroupPerson))
                        continue;
                    var dateOnlyList = _dynamicBlockFinder.ExtractBlockDays(startDate, fullGroupPerson);

                    if (dateOnlyList.Count == 0) continue;

                    var groupPersonList = _groupPersonBuilderBasedOnContractTime.SplitTeams(fullGroupPerson, startDate );

                    foreach (var groupPerson in groupPersonList)
                    {
                        var groupMatrixList = getScheduleMatrixProList(groupPerson, startDate, allPersonMatrixList);
                        var restriction = _restrictionAggregator.Aggregate(dateOnlyList, groupPerson, groupMatrixList,
                                                                            _schedulingOptions);

                        // (should we cover for max seats here?) ????
                        var shifts = _workShiftFilterService.Filter(startDate, groupPerson, groupMatrixList, restriction,
                                                                    _schedulingOptions);
                        if (shifts == null || shifts.Count <= 0)
                            continue;

                        var activityInternalData = _skillDayPeriodIntervalDataGenerator.Generate(fullGroupPerson, dateOnlyList);
                        var bestShiftProjectionCache = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
                                                                                                        _schedulingOptions
                                                                                                            .WorkShiftLengthHintOption,
                                                                                                        _schedulingOptions
                                                                                                            .UseMinimumPersons,
                                                                                                        _schedulingOptions
                                                                                                            .UseMaximumPersons);

                            _teamScheduling.Execute(dateOnlyList, groupMatrixList, groupPerson,
                                                bestShiftProjectionCache, unLockedDays, selectedPerson);
                        if (_cancelMe)
                            break;
                    }

                    if (_cancelMe)
                        break;
                }
                if (_cancelMe)
                    break;
                startDate = getNextDate(startDate, effectiveDays);

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

		private static DateOnly retrieveStartDate(BlockFinderType blockType, IList<IScheduleMatrixPro> matrixList, out  List<DateOnly> dayOff, out  List<DateOnly> effectiveDays, out  List<DateOnly> unLockedDays)
		{
			var startDate = DateOnly.MinValue;
			dayOff = new List<DateOnly>();
			effectiveDays = new List<DateOnly>();
			unLockedDays = new List<DateOnly>();

			if (matrixList != null)
			{
				for (var i = 0; i < matrixList.Count; i++)
				{
					int i1 = i;
					var openMatrixList = matrixList.Where(x => x.Person.Equals(matrixList[i1].Person));
					foreach (var scheduleMatrixPro in openMatrixList)
					{
						foreach (var scheduleDayPro in scheduleMatrixPro.EffectivePeriodDays.OrderBy(x => x.Day))
						{
							var daySignificantPart = scheduleDayPro.DaySchedulePart().SignificantPart();

							if (startDate == DateOnly.MinValue &&
								(daySignificantPart != SchedulePartView.DayOff &&
								 daySignificantPart != SchedulePartView.ContractDayOff &&
								 daySignificantPart != SchedulePartView.FullDayAbsence))
							{
								startDate = scheduleDayPro.Day;
							}

							if (daySignificantPart == SchedulePartView.DayOff)
								dayOff.Add(scheduleDayPro.Day);

							effectiveDays.Add(scheduleDayPro.Day);

							if (scheduleMatrixPro.UnlockedDays.Contains(scheduleDayPro))
								unLockedDays.Add(scheduleDayPro.Day);
						}
					}
				}
			}

			if (blockType == BlockFinderType.SingleDay)
				return unLockedDays.FirstOrDefault();

			return startDate;
		}

		private static DateOnly getNextDate(DateOnly dateOnly, List<DateOnly> effectiveDays)
		{
            dateOnly = dateOnly.AddDays(1);
            return effectiveDays.Contains(dateOnly) ? dateOnly : DateOnly.MinValue;
		}

        private static List<IScheduleMatrixPro> getScheduleMatrixProList(IGroupPerson groupPerson, DateOnly startDate, IEnumerable<IScheduleMatrixPro> matrixList)
        {
            var person = groupPerson;
            var date = startDate;
            var groupMatrixList =
                matrixList.Where(x => person.GroupMembers.Contains(x.Person) && x.SchedulePeriod.DateOnlyPeriod.Contains(date))
                    .ToList();
            return groupMatrixList;
        }

    }
}