using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public interface IFixedStaffSchedulingService
    {
        event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
        IList<IWorkShiftFinderResult> FinderResults { get; }
        void ClearFinderResults();
		bool DoTheScheduling(IList<IScheduleDay> selectedParts, ISchedulingOptions schedulingOptions, bool useOccupancyAdjustment, bool breakIfPersonCannotSchedule, ISchedulePartModifyAndRollbackService rollbackService);
    }

    public class FixedStaffSchedulingService : IFixedStaffSchedulingService
    {
	    private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
	    private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
	    private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
	    private readonly IScheduleService _scheduleService;
    	private readonly IDaysOffSchedulingService _daysOffSchedulingService;
    	private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
	    private readonly IPersonSkillProvider _personSkillProvider;

	    private readonly Random _random = new Random((int)DateTime.Now.TimeOfDay.Ticks);
        private readonly HashSet<IWorkShiftFinderResult> _finderResults = new HashSet<IWorkShiftFinderResult>();

        public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4")]
		public FixedStaffSchedulingService(
            ISchedulingResultStateHolder schedulingResultStateHolder,
			IDayOffsInPeriodCalculator dayOffsInPeriodCalculator, 
            IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IScheduleService scheduleService, 
			IDaysOffSchedulingService daysOffSchedulingService, 
			IResourceOptimizationHelper resourceOptimizationHelper,
			IPersonSkillProvider personSkillProvider)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_dayOffsInPeriodCalculator = dayOffsInPeriodCalculator;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_scheduleService = scheduleService;
			_daysOffSchedulingService = daysOffSchedulingService;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_personSkillProvider = personSkillProvider;
			_daysOffSchedulingService.DayScheduled += schedulerDayScheduled;
		}

		void schedulerDayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			OnDayScheduled(e);
		}

    	public IList<IWorkShiftFinderResult> FinderResults
	    {
            get
            {
                var ret = new List<IWorkShiftFinderResult>(_scheduleService.FinderResults);
                ret.AddRange(new List<IWorkShiftFinderResult>(_finderResults));

                return ret;
            }
	    }

        public void ClearFinderResults()
        {
            _finderResults.Clear();
            _scheduleService.ClearFinderResults();
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public bool DoTheScheduling(IList<IScheduleDay> selectedParts, ISchedulingOptions schedulingOptions, bool useOccupancyAdjustment, bool breakIfPersonCannotSchedule, ISchedulePartModifyAndRollbackService rollbackService)
        {
        	var result = true;
			var resourceCalculateDelayer = new ResourceCalculateDelayer(
				_resourceOptimizationHelper, 
				schedulingOptions.ResourceCalculateFrequency,
				useOccupancyAdjustment,
				schedulingOptions.ConsiderShortBreaks);

			var personDateDictionary = (from p in selectedParts
			                            group p.DateOnlyAsPeriod.DateOnly by p.Person
			                            into g
			                            select new {g.Key, Values = g.ToList()}).ToDictionary(k => k.Key, v => v.Values);

				var dates = GetAllDates(personDateDictionary);
				var initialPersons = personDateDictionary.Keys;
		    foreach (DateOnly date in dates)
		    {
		        var persons = initialPersons.ToList();
		        IPerson person = GetRandomPerson(persons.ToArray());

		        while (person != null)
		        {
		            IScheduleDay schedulePart = _schedulingResultStateHolder.Schedules[person].ScheduledDay(date);
		            if (!schedulePart.IsScheduled())
		            {
		                var virtualSchedulePeriod = person.VirtualSchedulePeriod(date);
		                if (!virtualSchedulePeriod.IsValid)
		                {
		                    persons.Remove(person);
		                    person = GetRandomPerson(persons.ToArray());
		                    continue;
		                }

		                if (HasCorrectNumberOfDaysOff(virtualSchedulePeriod, date))
		                {
		                    if (personDateDictionary[person].Contains(date))
		                    {
		                        var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(
		                            schedulePart, schedulingOptions);

		                        bool schedulePersonOnDayResult = _scheduleService.SchedulePersonOnDay(schedulePart,
		                                                                                              schedulingOptions,
		                                                                                              effectiveRestriction,
		                                                                                              resourceCalculateDelayer,
		                                                                                              null, rollbackService);

		                        result = result && schedulePersonOnDayResult;
		                        if (!result && breakIfPersonCannotSchedule)
		                            return false;

								var eventArgs = new SchedulingServiceSuccessfulEventArgs(schedulePart); 
								OnDayScheduled(eventArgs);
		                        if (eventArgs.Cancel) return result;
		                    }
		                }
		            }

		            persons.Remove(person);
		            person = GetRandomPerson(persons.ToArray());
		        }
		    }
		    return result;
		}

        public IPerson GetRandomPerson(IList<IPerson> persons)
        {
            IPerson ret = null;
            if (persons.Count > 0)
            {
                int index;
                lock (_random)
                {
                    index = _random.Next(0, persons.Count);
                }
                ret = persons[index];
            }
            return ret;
        }

		public bool HasCorrectNumberOfDaysOff(IVirtualSchedulePeriod schedulePeriod, DateOnly dateOnly)
		{
			int targetDaysOff;
			IList<IScheduleDay> dayOffsNow = new List<IScheduleDay>();
			var result = _dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(schedulePeriod, out targetDaysOff, out dayOffsNow);

			if (!result)
				AddFinderResult(schedulePeriod.Person, dateOnly,
										string.Format(CultureInfo.InvariantCulture, UserTexts.Resources.WrongNumberOfDayOffsInSchedulePeriod, targetDaysOff, dayOffsNow.Count));
			return result;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual IList<DateOnly> GetAllDates(Dictionary<IPerson, List<DateOnly>> selectedParts)
        {
            var ret = new List<DateOnly>();
            foreach (var dateCollection in selectedParts.Values)
            {
				foreach (var dateOnly in dateCollection)
	            {
					if (!ret.Contains(dateOnly))
						ret.Add(dateOnly);
	            }
            }
            ret.Sort(new DateSorter());
            return ret;
        }

        protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs scheduleServiceBaseEventArgs)
        {
            EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
            if (temp != null)
            {
                temp(this, scheduleServiceBaseEventArgs);
            }
        }

        protected void AddFinderResult(IPerson person, DateOnly scheduleDateOnly, string message)
        {
            IWorkShiftFinderResult finderResult = new WorkShiftFinderResult(person, scheduleDateOnly);
            finderResult.AddFilterResults(new WorkShiftFilterResult(message, 0, 0));
            _finderResults.Add(finderResult);
        }

		internal class DateSorter : IComparer<DateOnly>
		{
			public int Compare(DateOnly x, DateOnly y)
			{
				return x.Date.CompareTo(y.Date);
			}
		}
    }
}
