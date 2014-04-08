using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public interface IStudentSchedulingService
    {
        event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
        IList<IWorkShiftFinderResult> FinderResults { get; }
        void ClearFinderResults();
        bool DoTheScheduling(IList<IScheduleDay> selectedParts, ISchedulingOptions schedulingOptions, bool useOccupancyAdjustment, bool breakIfPersonCannotSchedule, ISchedulePartModifyAndRollbackService rollbackService);

    }

    /// <summary>
	/// Utilities for scheduling students
	/// </summary>
	/// /// 
	/// <remarks>
	///  Created by: Ola
	///  Created date: 2008-10-29    
	/// /// </remarks>
	public class StudentSchedulingService : IStudentSchedulingService
    {
	    private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
	    private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
	    private readonly IScheduleService _scheduleService;
    	private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
	    private readonly IPersonSkillProvider _personSkillProvider;

	    private readonly Random _random = new Random((int)DateTime.Now.TimeOfDay.Ticks);
        private readonly HashSet<IWorkShiftFinderResult> _finderResults = new HashSet<IWorkShiftFinderResult>();

        public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public StudentSchedulingService( ISchedulingResultStateHolder schedulingResultStateHolder,
            IEffectiveRestrictionCreator effectiveRestrictionCreator, IScheduleService scheduleService,
			IResourceOptimizationHelper resourceOptimizationHelper, IPersonSkillProvider personSkillProvider)
		{
		    _schedulingResultStateHolder = schedulingResultStateHolder;
		    _effectiveRestrictionCreator = effectiveRestrictionCreator;
		    _scheduleService = scheduleService;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_personSkillProvider = personSkillProvider;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public bool DoTheScheduling(IList<IScheduleDay> selectedParts, ISchedulingOptions schedulingOptions, bool useOccupancyAdjustment, bool breakIfPersonCannotSchedule,
			ISchedulePartModifyAndRollbackService rollbackService)
        {
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1,
																		useOccupancyAdjustment,
																		schedulingOptions.ConsiderShortBreaks);
			
			var extractor = new ScheduleProjectionExtractor(_personSkillProvider, _schedulingResultStateHolder.Skills.Min(s => s.DefaultResolution));
			var resources = extractor.CreateRelevantProjectionList(_schedulingResultStateHolder.Schedules);
	        using (new ResourceCalculationContext<IResourceCalculationDataContainerWithSingleOperation>(resources))
	        {
		        schedulingOptions.OnlyShiftsWhenUnderstaffed = true;
		        doTheSchedulingLoop(selectedParts, schedulingOptions, breakIfPersonCannotSchedule, true,
		                            resourceCalculateDelayer, rollbackService);

		        schedulingOptions.OnlyShiftsWhenUnderstaffed = false;
		        doTheSchedulingLoop(selectedParts, schedulingOptions, breakIfPersonCannotSchedule, true,
		                            resourceCalculateDelayer, rollbackService);

		        schedulingOptions.OnlyShiftsWhenUnderstaffed = true;

		        return doTheSchedulingLoop(selectedParts, schedulingOptions, breakIfPersonCannotSchedule, false,
		                                   resourceCalculateDelayer, rollbackService);
	        }
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
        protected void OnDayScheduled(SchedulingServiceBaseEventArgs scheduleServiceBaseEventArgs)
        {
            EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
            if (temp != null)
            {
                temp(this, scheduleServiceBaseEventArgs);
            }
        }

        private bool doTheSchedulingLoop(IList<IScheduleDay> selectedParts, ISchedulingOptions schedulingOptions,
			bool breakIfPersonCannotSchedule, bool excludeStudentsWithEnoughHours, IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulePartModifyAndRollbackService rollbackService)
        {
			bool everyPersonScheduled = true;
			bool tempOnlyShiftsWhenUnderstaffed =
                schedulingOptions.OnlyShiftsWhenUnderstaffed;

			// all list off Days and person that we don't want to try again, the person(s> could not be scheduled on that day
			//IDictionary<ISkillDay, IList<IPerson>> daysAndPersonsToExclude = new Dictionary<ISkillDay, IList<IPerson>>();

			// all list off Days that we don't want to try again, no person could be scheduled on that day
			IList<ISkillDay> daysToExclude = new List<ISkillDay>();

			// get the day
            ISkillDay theDay = FindMostUnderstaffedSkillDay(GetAllDates(selectedParts), daysToExclude);
			if (theDay == null)
				return false;
			var dateOnly = theDay.CurrentDate;

			// all person with the skill
            IList<IPerson> persons = FilterPersonsOnSkill(dateOnly, GetAllPersons(selectedParts, excludeStudentsWithEnoughHours, dateOnly), theDay.Skill);
			// after a day is scheduled it is removed from the list
			while (theDay != null && selectedParts.Count > 0)
			{
				IPerson person;
				if (persons.Count > 0)
				{
					bool schedulePersonOnDayResult = false;
					person = GetRandomPerson(persons);

					IVirtualSchedulePeriod virtualSchedulePeriod = person.VirtualSchedulePeriod(dateOnly);
					TimeSpan minTimeSchedulePeriod = virtualSchedulePeriod.MinTimeSchedulePeriod;
					if (minTimeSchedulePeriod == new TimeSpan(0))
					{
						// if the MinTimeSchedulePeriod is not set we shall never overstaff
                        schedulingOptions.OnlyShiftsWhenUnderstaffed = true;
					}
                    IScheduleDay part = GetSchedulePartOnDateAndPerson(selectedParts,person, dateOnly);
					// if success remove the part
					if (part != null)
					{
						var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(part, schedulingOptions);
						schedulePersonOnDayResult = _scheduleService.SchedulePersonOnDay(part, schedulingOptions, effectiveRestriction, resourceCalculateDelayer, null, rollbackService);
						everyPersonScheduled = everyPersonScheduled && schedulePersonOnDayResult;

					}
					//reset for next person
                    schedulingOptions.
						OnlyShiftsWhenUnderstaffed = tempOnlyShiftsWhenUnderstaffed;
					if (schedulePersonOnDayResult)
					{
                        selectedParts.Remove(part);
                        theDay = FindMostUnderstaffedSkillDay(GetAllDates(selectedParts), daysToExclude);
						if (theDay != null)
						{
							dateOnly = theDay.CurrentDate;
                            persons = FilterPersonsOnSkill(dateOnly, GetAllPersons(selectedParts, excludeStudentsWithEnoughHours, dateOnly), theDay.Skill);
						}
					}
					else
					{
						if (breakIfPersonCannotSchedule)
							return false;

						persons.Remove(person);
					}

					var eventArgs = new SchedulingServiceSuccessfulEventArgs(part);
					OnDayScheduled(eventArgs);
					if (eventArgs.Cancel) return everyPersonScheduled;
				}
				else
				{
					//no one could be scheduled
					daysToExclude.Add(theDay);
                    theDay = FindMostUnderstaffedSkillDay(GetAllDates(selectedParts), daysToExclude);
					while (theDay != null)
					{
						dateOnly = theDay.CurrentDate;
                        persons = FilterPersonsOnSkill(dateOnly, GetAllPersons(selectedParts, excludeStudentsWithEnoughHours, dateOnly), theDay.Skill);
						if (persons.Count > 0)
						{
							break;
						}

						daysToExclude.Add(theDay);
                        theDay = FindMostUnderstaffedSkillDay(GetAllDates(selectedParts), daysToExclude);
					}
				}
			}
			return everyPersonScheduled;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public ICollection<IPerson> GetAllPersons(IList<IScheduleDay> selectedParts, bool excludeStudentsWithEnoughHours, DateOnly onDate)
        {
            var ret = new HashSet<IPerson>();
            foreach (var part in selectedParts)
            {
                ret.Add(part.Person);
            }
            if (excludeStudentsWithEnoughHours)
            {
                IList<IPerson> personsToRemove = new List<IPerson>();
                foreach (var person in ret)
                {
                    TimeSpan scheduledSoFar = TimeSpan.Zero;
                    IVirtualSchedulePeriod virtualSchedulePeriod = person.VirtualSchedulePeriod(onDate);
                    TimeSpan minTimeSchedulePeriod = virtualSchedulePeriod.MinTimeSchedulePeriod;
                    if (minTimeSchedulePeriod > new TimeSpan(0))
                    {
                        DateOnlyPeriod dateOnlyPeriod = virtualSchedulePeriod.DateOnlyPeriod;

                        foreach (var localDate in dateOnlyPeriod.DayCollection())
                        {
                            IScheduleDay schedulePart = _schedulingResultStateHolder.Schedules[person].ScheduledDay(localDate);
                            IProjectionService projSvc = schedulePart.ProjectionService();
                            IVisualLayerCollection res = projSvc.CreateProjection();
                            scheduledSoFar = scheduledSoFar.Add(res.ContractTime());
                        }

                        if (scheduledSoFar >= minTimeSchedulePeriod)
                            personsToRemove.Add(person);
                    }
                }

                foreach (var person in personsToRemove)
                {
                    ret.Remove(person);
                }
            }
            return ret;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual IList<DateOnly> GetAllDates(IList<IScheduleDay> selectedParts)
        {
            IList<DateOnly> ret = new List<DateOnly>();
            foreach (IScheduleDay part in selectedParts)
            {
                var dateOnly = part.DateOnlyAsPeriod.DateOnly;
                if (!ret.Contains(part.DateOnlyAsPeriod.DateOnly))
                    ret.Add(dateOnly);
            }
            return ret;
        }

        public virtual IScheduleDay GetSchedulePartOnDateAndPerson(IList<IScheduleDay> selectedParts, IPerson thePerson, DateOnly theDate)
		{
			return selectedParts.FirstOrDefault(part => part.DateOnlyAsPeriod.DateOnly.Equals(theDate) && part.Person.Equals(thePerson));	 
		}

		public ISkillDay FindMostUnderstaffedSkillDay(IList<DateOnly> theDates, IList<ISkillDay> skillDaysExcluded)
		{
			ISkillDay retSkillDay = null;
			double highestValue = double.MinValue;
			
			IList<ISkillDay> skillDays = _schedulingResultStateHolder.SkillDaysOnDateOnly(theDates);

			foreach (ISkillDay skillDay in skillDays)
			{
				TimeSpan forecasted = skillDay.ForecastedIncomingDemand;
				if (forecasted > new TimeSpan())
				{
					var res = new TimeSpan();
					foreach (ISkillStaffPeriod skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
					{
						res = res.Add(TimeSpan.FromMinutes(skillStaffPeriod.Payload.CalculatedResource * skillStaffPeriod.Period.ElapsedTime().TotalMinutes));
					}
					double val = forecasted.TotalMinutes - res.TotalMinutes;
					if (val > highestValue && !skillDaysExcluded.Contains(skillDay))
					{
						highestValue = val;
						retSkillDay = skillDay;
					}
				}
			}
			if (retSkillDay == null)
			{
				System.Diagnostics.Debug.Print(" Hittade ingen mest underbemannad skilldag bland " + skillDays.Count + " skilldagar");
			}
			return retSkillDay;
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public static IList<IPerson> FilterPersonsOnSkill(DateOnly onDate, IEnumerable<IPerson> persons, ISkill filterOnSkill)
		{
			IList<IPerson> ret = new List<IPerson>();
			var period = new DateOnlyPeriod(onDate, onDate.AddDays(1));
			foreach (IPerson person in persons)
			{
				bool found = false;
				foreach (IPersonPeriod personPeriod in person.PersonPeriods(period))
				{
					foreach (IPersonSkill skill in personPeriod.PersonSkillCollection)
					{
						if (skill.Skill.Equals(filterOnSkill) && !ret.Contains(person))
						{
							ret.Add(person);
							found = true;
							break;
						}
					}
					if (found)
						break;
				}
			}
			return ret;
		}
	}
}
