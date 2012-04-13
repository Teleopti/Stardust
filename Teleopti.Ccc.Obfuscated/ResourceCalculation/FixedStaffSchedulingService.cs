using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{
    public interface IFixedStaffSchedulingService
    {
        event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
        IList<IWorkShiftFinderResult> FinderResults { get; }
        void ClearFinderResults();
        void DayOffScheduling(IList<IScheduleMatrixPro> matrixList, IList<IScheduleMatrixPro> matrixListAll, ISchedulePartModifyAndRollbackService rollbackService);
        bool DoTheScheduling(IList<IScheduleDay> selectedParts, ISchedulingOptions schedulingOptions, bool useOccupancyAdjustment, bool breakIfPersonCannotSchedule);
    }

    public class FixedStaffSchedulingService : IFixedStaffSchedulingService
    {
	    private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
	    private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
	    private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
	    private readonly IScheduleService _scheduleService;
    	private readonly IAbsencePreferenceScheduler _absencePreferenceScheduler;
    	private readonly IDayOffScheduler _dayOffScheduler;
    	private readonly IResourceOptimizationHelper _resourceOptimizationHelper;

    	private readonly Random _random = new Random((int)DateTime.Now.TimeOfDay.Ticks);
        private readonly HashSet<IWorkShiftFinderResult> _finderResults = new HashSet<IWorkShiftFinderResult>();

        public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public FixedStaffSchedulingService(
            ISchedulingResultStateHolder schedulingResultStateHolder,
			IDayOffsInPeriodCalculator dayOffsInPeriodCalculator, 
            IEffectiveRestrictionCreator effectiveRestrictionCreator,
			IScheduleService scheduleService, 
			IAbsencePreferenceScheduler absencePreferenceScheduler, 
            IDayOffScheduler dayOffScheduler,
			IResourceOptimizationHelper resourceOptimizationHelper)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_dayOffsInPeriodCalculator = dayOffsInPeriodCalculator;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_scheduleService = scheduleService;
			if (absencePreferenceScheduler == null)
				throw new ArgumentNullException("absencePreferenceScheduler");

			_absencePreferenceScheduler = absencePreferenceScheduler;
			if (dayOffScheduler == null)
				throw new ArgumentNullException("dayOffScheduler");
			_dayOffScheduler = dayOffScheduler;
			_resourceOptimizationHelper = resourceOptimizationHelper;

			_absencePreferenceScheduler.DayScheduled += schedulerDayScheduled;
			_dayOffScheduler.DayScheduled += schedulerDayScheduled;
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

        public void DayOffScheduling(IList<IScheduleMatrixPro> matrixList, IList<IScheduleMatrixPro> matrixListAll, ISchedulePartModifyAndRollbackService rollbackService)
        {
            _absencePreferenceScheduler.AddPreferredAbsence(matrixList);
            _dayOffScheduler.DayOffScheduling(matrixList, matrixListAll, rollbackService);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public bool DoTheScheduling(IList<IScheduleDay> selectedParts, ISchedulingOptions schedulingOptions, bool useOccupancyAdjustment, bool breakIfPersonCannotSchedule)
        {
        	var result = true;
			var resourceCalculateDelayer = new ResourceCalculateDelayer(
				_resourceOptimizationHelper, 
				schedulingOptions.ResourceCalculateFrequency,
				useOccupancyAdjustment,
				schedulingOptions.ConsiderShortBreaks);

			var dates = GetAllDates(selectedParts);
			foreach (DateOnly date in dates)
			{
			    DateOnly theDate = date;
                var persons = GetAllPersons(selectedParts);
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


                            var thePerson = person;
                            var exists = selectedParts.FirstOrDefault(part => part.DateOnlyAsPeriod.DateOnly.Equals(theDate) && part.Person.Equals(thePerson));

                            if (exists != null)
                            {
                                var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(
                                schedulePart, schedulingOptions);

                            bool schedulePersonOnDayResult =
								_scheduleService.SchedulePersonOnDay(schedulePart, schedulingOptions, useOccupancyAdjustment, effectiveRestriction, resourceCalculateDelayer);

                                result = result && schedulePersonOnDayResult;
                                if (!result && breakIfPersonCannotSchedule)
                                    return false;

                                var eventArgs = new SchedulingServiceBaseEventArgs(schedulePart);
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public virtual ICollection<IPerson> GetAllPersons(IList<IScheduleDay> selectedParts)
        {
            var ret = new HashSet<IPerson>();
            foreach (var part in selectedParts)
            {
                ret.Add(part.Person);
            }
            return ret;
        }

		public bool HasCorrectNumberOfDaysOff(IVirtualSchedulePeriod schedulePeriod, DateOnly dateOnly)
		{
			int targetDaysOff;
			int dayOffsNow;
			var result = _dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(schedulePeriod, out targetDaysOff, out dayOffsNow);

			if (!result)
				AddFinderResult(schedulePeriod.Person, dateOnly,
										string.Format(CultureInfo.InvariantCulture, UserTexts.Resources.WrongNumberOfDayOffsInSchedulePeriod, targetDaysOff, dayOffsNow));
			return result;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual IList<DateOnly> GetAllDates(IList<IScheduleDay> selectedParts)
        {
            var ret = new List<DateOnly>();
            foreach (IScheduleDay part in selectedParts)
            {
                var dateOnly = part.DateOnlyAsPeriod.DateOnly;
                if (!ret.Contains(part.DateOnlyAsPeriod.DateOnly))
                    ret.Add(dateOnly);
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
