using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IStudentSchedulingService
	{
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
		bool DoTheScheduling(IList<IScheduleDay> selectedParts, SchedulingOptions schedulingOptions, bool breakIfPersonCannotSchedule, ISchedulePartModifyAndRollbackService rollbackService);
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
		private readonly IResourceCalculation _resourceOptimizationHelper;
		private readonly IUserTimeZone _userTimeZone;

		private readonly Random _random = new Random((int)DateTime.Now.TimeOfDay.Ticks);

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public StudentSchedulingService(ISchedulingResultStateHolder schedulingResultStateHolder,
				IEffectiveRestrictionCreator effectiveRestrictionCreator, IScheduleService scheduleService,
			IResourceCalculation resourceOptimizationHelper,
			IUserTimeZone userTimeZone)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_scheduleService = scheduleService;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_userTimeZone = userTimeZone;
		}

		public bool DoTheScheduling(IList<IScheduleDay> selectedParts, SchedulingOptions schedulingOptions,
			bool breakIfPersonCannotSchedule,
			ISchedulePartModifyAndRollbackService rollbackService)
		{
			var skills = _schedulingResultStateHolder.Skills;
			if (skills.Length == 0) return false;
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper,
				schedulingOptions.ConsiderShortBreaks, _schedulingResultStateHolder, _userTimeZone);

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

		private CancelSignal onDayScheduled(SchedulingServiceBaseEventArgs args)
		{
			EventHandler<SchedulingServiceBaseEventArgs> handler = DayScheduled;
			if (handler != null)
			{
				handler(this, args);
				if (args.Cancel) return new CancelSignal { ShouldCancel = true };
			}
			return new CancelSignal();
		}

		private bool doTheSchedulingLoop(IList<IScheduleDay> selectedParts, SchedulingOptions schedulingOptions,
		 bool breakIfPersonCannotSchedule, bool excludeStudentsWithEnoughHours, IResourceCalculateDelayer resourceCalculateDelayer,
		 ISchedulePartModifyAndRollbackService rollbackService)
		{
			var everyPersonScheduled = true;
			var cancel = false;
			var tempOnlyShiftsWhenUnderstaffed =
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
			IList<IPerson> persons = FilterPersonsOnSkill.Filter(dateOnly, GetAllPersons(selectedParts, excludeStudentsWithEnoughHours, dateOnly), theDay.Skill);
			// after a day is scheduled it is removed from the list
			while (theDay != null && selectedParts.Count > 0)
			{
				if (persons.Count > 0)
				{
					bool schedulePersonOnDayResult = false;
					IPerson person = GetRandomPerson(persons);

					IVirtualSchedulePeriod virtualSchedulePeriod = person.VirtualSchedulePeriod(dateOnly);
					TimeSpan minTimeSchedulePeriod = virtualSchedulePeriod.MinTimeSchedulePeriod;
					if (minTimeSchedulePeriod == new TimeSpan(0))
					{
						// if the MinTimeSchedulePeriod is not set we shall never overstaff
						schedulingOptions.OnlyShiftsWhenUnderstaffed = true;
					}
					IScheduleDay part = GetSchedulePartOnDateAndPerson(selectedParts, person, dateOnly);
					// if success remove the part
					if (part != null)
					{
						var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(part, schedulingOptions);
						schedulePersonOnDayResult = _scheduleService.SchedulePersonOnDay(part, schedulingOptions, effectiveRestriction, resourceCalculateDelayer, rollbackService);
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
							persons = FilterPersonsOnSkill.Filter(dateOnly, GetAllPersons(selectedParts, excludeStudentsWithEnoughHours, dateOnly), theDay.Skill);
						}
					}
					else
					{
						if (breakIfPersonCannotSchedule)
							return false;

						persons.Remove(person);
					}

					var eventArgs = new SchedulingServiceSuccessfulEventArgs(part, () => cancel = true);
					var progressResult = onDayScheduled(eventArgs);
					if (cancel || progressResult.ShouldCancel) return everyPersonScheduled;
				}
				else
				{
					//no one could be scheduled
					daysToExclude.Add(theDay);
					theDay = FindMostUnderstaffedSkillDay(GetAllDates(selectedParts), daysToExclude);
					while (theDay != null)
					{
						dateOnly = theDay.CurrentDate;
						persons = FilterPersonsOnSkill.Filter(dateOnly, GetAllPersons(selectedParts, excludeStudentsWithEnoughHours, dateOnly), theDay.Skill);
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

						foreach (var schedulePart in _schedulingResultStateHolder.Schedules[person].ScheduledDayCollection(dateOnlyPeriod))
						{
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

			var skillDays = _schedulingResultStateHolder.SkillDaysOnDateOnly(theDates);

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
				System.Diagnostics.Debug.Print(" Hittade ingen mest underbemannad skilldag bland " + skillDays.Count() + " skilldagar");
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


	}
}
