using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{

    public interface IAdvanceSchedulingService
    {
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
        bool Execute(IDictionary<string, IWorkShiftFinderResult> workShiftFinderResultList, IList<IScheduleMatrixPro> allPersonMatrixList, IList<IScheduleMatrixPro> selectedPersonMatrixList);
    }
    public class AdvanceSchedulingService : IAdvanceSchedulingService
    {
        private readonly IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
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
            IGroupPersonBuilderForOptimization groupPersonBuilderForOptimization 
            )
        {
            _groupPersonBuilderForOptimization = groupPersonBuilderForOptimization;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public bool Execute(IDictionary<string, IWorkShiftFinderResult> workShiftFinderResultList, IList<IScheduleMatrixPro> allPersonMatrixList, IList<IScheduleMatrixPro> selectedPersonMatrixList)
        {
			_teamScheduling.DayScheduled += dayScheduled;
            List<DateOnly> dayOff,effectiveDays,unLockedDays;

			var startDate = retrieveStartDate(selectedPersonMatrixList, out dayOff, out effectiveDays, out unLockedDays);

            var selectedPerson = selectedPersonMatrixList.Select(scheduleMatrixPro => scheduleMatrixPro.Person).Distinct().ToList();
            
            while (startDate != DateOnly.MinValue )
            {
                //call class that return the teamblock dates for a given date (problem if team members don't have same days off)
                var dateOnlyList = _dynamicBlockFinder.ExtractBlockDays( startDate );
                var allGroupPersonListOnStartDate = new HashSet<IGroupPerson>();
                
                foreach (var person in selectedPerson)
                {
					if(_schedulingOptions.GroupOnGroupPageForLevelingPer.Key.Equals("SingleAgentTeam"))
						allGroupPersonListOnStartDate.Add(_groupPersonBuilderForOptimization.BuildSingleGroupPerson(person, startDate));
					else
						allGroupPersonListOnStartDate.Add(_groupPersonBuilderForOptimization.BuildGroupPerson(person, startDate));
                }
                
                foreach (var fullGroupPerson in allGroupPersonListOnStartDate.GetRandom(allGroupPersonListOnStartDate.Count, true))
                {
                    var groupPersonList = _groupPersonBuilderBasedOnContractTime.SplitTeams(fullGroupPerson, startDate);
                    foreach (var groupPerson in groupPersonList)
                    {
                        var groupMatrixList = getScheduleMatrixProList(groupPerson, startDate, allPersonMatrixList);
                        //call class that returns the aggregated restrictions for the teamblock (is team member personal skills needed for this?)
                        var restriction = _restrictionAggregator.Aggregate(dateOnlyList, groupPerson,groupMatrixList, _schedulingOptions);
						if (restriction == null) continue;

                        //call class that returns the aggregated intraday dist based on teamblock dates ???? consider the priority and understaffing
                        var activityInternalData = _skillDayPeriodIntervalDataGenerator.Generate(fullGroupPerson, dateOnlyList);

                        //call class that returns a filtered list of valid workshifts, this class will probably consists of a lot of subclasses 
                        // (should we cover for max seats here?) ????
                        var shifts = _workShiftFilterService.Filter(startDate, groupPerson, groupMatrixList, restriction, _schedulingOptions);

                        if (shifts != null && shifts.Count > 0)
                        {
                            //call class that returns the workshift to use based on valid workshifts, the aggregated intraday dist and other things we need ???
	                        IShiftProjectionCache bestShiftProjectionCache;
	                        if (shifts.Count == 1)
		                        bestShiftProjectionCache = shifts.First();
							else
								bestShiftProjectionCache = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
                                                                                        _schedulingOptions.WorkShiftLengthHintOption,
                                                                                        _schedulingOptions.UseMinimumPersons,
                                                                                        _schedulingOptions.UseMaximumPersons);

                            //call class that schedules given date with given workshift on the complete team
                            //call class that schedules the unscheduled days for the teamblock using the same start time from the given shift, 
                            //this class will handle steady state as well as individual
                            _teamScheduling.Execute(startDate, dateOnlyList, groupMatrixList, groupPerson, restriction, bestShiftProjectionCache, unLockedDays,selectedPerson );
							if (_cancelMe)
								break;
                        }
						if (_cancelMe)
							break;
                    }
					if (_cancelMe)
						break;
                }
				if (_cancelMe)
					break;
                startDate = getNextDate(dateOnlyList.OrderByDescending(x => x.Date).First(), effectiveDays,dayOff,unLockedDays );
                
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

		private static DateOnly retrieveStartDate(IList<IScheduleMatrixPro> matrixList, out  List<DateOnly> dayOff, out  List<DateOnly> effectiveDays, out  List<DateOnly> unLockedDays)
		{
			var startDate = DateOnly.MinValue;
			dayOff = new List<DateOnly>();
			effectiveDays = new List<DateOnly>();
			unLockedDays = new List<DateOnly>();

			if (matrixList != null)
			{
				for (var i = 0; i < matrixList.Count; i++)
				{
					var openMatrixList = matrixList.Where(x => x.Person.Equals(matrixList[i].Person));
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
			return startDate;
		}

		private static DateOnly getNextDate(DateOnly dateOnly, List<DateOnly> effectiveDays, List<DateOnly> daysOff, List<DateOnly> unLockedDays)
		{
			dateOnly = dateOnly.AddDays(1);
			while (daysOff.Contains(dateOnly))
				dateOnly = dateOnly.AddDays(1);
			return effectiveDays.Contains(dateOnly) && unLockedDays.Contains(dateOnly) ? dateOnly : DateOnly.MinValue;
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