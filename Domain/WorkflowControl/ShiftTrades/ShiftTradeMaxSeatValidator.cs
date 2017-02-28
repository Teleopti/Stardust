﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class ShiftTradeMaxSeatValidator : IShiftTradeMaxSeatValidator
	{
		private readonly ICurrentScenario _currentScenario;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonRepository _personRepository;

		public ShiftTradeMaxSeatValidator(ICurrentScenario currentScenario, IScheduleStorage scheduleStorage, IPersonRepository personRepository)
		{
			_currentScenario = currentScenario;
			_scheduleStorage = scheduleStorage;
			_personRepository = personRepository;
		}

		public bool Validate (ISite site, IScheduleDay scheduleDayIncoming, IScheduleDay scheduleDayOutgoing,
			List<IVisualLayer> incomingActivitiesRequiringSeat, IList<ISeatUsageForInterval> seatUsageOnEachIntervalDic, TimeZoneInfo timeZoneInfo)
		{
			var schedulePeriod = scheduleDayIncoming.Period;
			var scheduleDateOnlyPeriod = schedulePeriod.ToDateOnlyPeriod(timeZoneInfo);
			var personList = getPeopleForSiteOnDate(site, scheduleDateOnlyPeriod);
			var personProvider = new PersonProvider(personList)
			{
				DoLoadByPerson = false
			};

#pragma warning disable 618
			var schedulesThatOverlap = _scheduleStorage.FindSchedulesForPersons(new ScheduleDateTimePeriod(schedulePeriod), _currentScenario.Current(),
#pragma warning restore 618
				personProvider, new ScheduleDictionaryLoadOptions(false, false), personList);
			return
				personList.Any(
					person => personScheduleCausesMaxSeatViolation(site, scheduleDayOutgoing, schedulesThatOverlap,
					person, scheduleDateOnlyPeriod, seatUsageOnEachIntervalDic, incomingActivitiesRequiringSeat));
		}

		private static bool personScheduleCausesMaxSeatViolation(ISite site, IScheduleDay scheduleDayOutgoing,
			IScheduleDictionary schedulesThatOverlap, IPerson person, DateOnlyPeriod scheduleDateOnlyPeriod,
			IList<ISeatUsageForInterval> seatUsageOnEachIntervalDic, IEnumerable<IVisualLayer> incomingActivitiesRequiringSeat)
		{
			seatUsageOnEachIntervalDic.ForEach(interval => interval.SeatUsage = 0);

			IScheduleRange scheduleRangeForPerson;
			if (!schedulesThatOverlap.TryGetValue(person, out scheduleRangeForPerson))
			{
				return false;
			}

			var scheduleDayCollection = scheduleRangeForPerson
				.ScheduledDayCollection(scheduleDateOnlyPeriod)
				.Where(scheduleDay => !scheduleDayMatchesScheduleDayToBeTraded(scheduleDayOutgoing, scheduleDay));

			return activitiesOnDayAlreadyMatchOrExceedMaximumSeats(site, scheduleDayCollection,
				seatUsageOnEachIntervalDic, incomingActivitiesRequiringSeat);
		}

		private List<IPerson> getPeopleForSiteOnDate(ISite siteTo, DateOnlyPeriod dateOnlyPeriod)
		{
			var personList = new List<IPerson>();

			foreach (var team in siteTo.TeamCollection)
			{
				personList.AddRange(_personRepository.FindPeopleBelongTeam(team, dateOnlyPeriod));
			}
			return personList;
		}

		private static bool scheduleDayMatchesScheduleDayToBeTraded(IScheduleDay scheduleDayOutgoing, IScheduleDay scheduleDay)
		{
			return scheduleDay.Person == scheduleDayOutgoing.Person && scheduleDay.Period == scheduleDayOutgoing.Period;
		}

		private static bool activitiesOnDayAlreadyMatchOrExceedMaximumSeats(ISite site,
			IEnumerable<IScheduleDay> scheduleDayCollection, IList<ISeatUsageForInterval> seatUsageOnEachIntervalDic,
			IEnumerable<IVisualLayer> incomingActivitiesRequiringSeat)
		{
			var activitiesTocheck = new List<IVisualLayer>();

			var activities = scheduleDayCollection.Select(getActivitiesRequiringSeat);
			foreach (var activitiesRequiringSeat in activities.Where(a => a != null))
			{
				activitiesTocheck.AddRange(activitiesRequiringSeat);
			}

			activitiesTocheck.AddRange(incomingActivitiesRequiringSeat);

			foreach (var activity in activitiesTocheck)
			{
				var intervalsThatContainActivity = seatUsageOnEachIntervalDic
					.Where(interval => activity.Period.StartDateTime < interval.IntervalEnd &&
									   activity.Period.EndDateTime > interval.IntervalStart);

				foreach (var interval in intervalsThatContainActivity)
				{
					interval.SeatUsage++;
					if (interval.SeatUsage > site.MaxSeats)
					{
						return true;
					}
				}
			}

			return false;
		}

		private static IEnumerable<IVisualLayer> getActivitiesRequiringSeat(IScheduleDay scheduleDay)
		{
			var personAssignment = scheduleDay.PersonAssignment();
			var projection = personAssignment?.ProjectionService().CreateProjection();
			var activitiesRequiringSeat = projection?.Where(layer =>
			{
				var activityLayer = layer.Payload as IActivity;
				return activityLayer != null && activityLayer.RequiresSeat;
			});
			return activitiesRequiringSeat;
		}
	}
}