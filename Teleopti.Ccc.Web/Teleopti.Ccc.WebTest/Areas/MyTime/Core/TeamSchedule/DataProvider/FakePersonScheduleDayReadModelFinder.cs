using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	class FakePersonScheduleDayReadModelFinder : IPersonScheduleDayReadModelFinder
	{
		private readonly IPersonRepository _personRepository;

		private readonly IPersonAssignmentRepository _personAssignmentRepository;

		public FakePersonScheduleDayReadModelFinder(IPersonAssignmentRepository personAssignmentRepository, IPersonRepository personRepository)
		{
			_personAssignmentRepository = personAssignmentRepository;
			_personRepository = personRepository;
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPerson(DateOnly startDate, DateOnly endDate, Guid personId)
		{
			throw new NotImplementedException();
		}

		public PersonScheduleDayReadModel ForPerson(DateOnly date, Guid personId)
		{
			var assignment = _personAssignmentRepository.LoadAll().SingleOrDefault(a => personId == a.Person.Id.Value && date == a.Date);
			var persons = _personRepository.LoadAll();
			
			if (assignment == default(object))
			{
				return PersonScheduleDayReadModelFactory.CreateSimplePersonScheduleDayReadModel(persons.First(p => p.Id.Value == personId), date);
			}

			if (assignment.DayOff() != null)
			{
				return PersonScheduleDayReadModelFactory.CreateDayOffPersonScheduleDayReadModel(assignment.Person, date);
			}

			var simpleLayers = assignment.ShiftLayers.Select(shiftLayer => new SimpleLayer
			{
				Start = shiftLayer.Period.StartDateTime,
				End = shiftLayer.Period.EndDateTime,
				Description = shiftLayer.Payload.Name
			}).ToList();

			return PersonScheduleDayReadModelFactory.CreatePersonScheduleDayReadModelWithSimpleShift(assignment.Person, date, simpleLayers);
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPeople(DateTimePeriod period, IEnumerable<Guid> personIds)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPeople(DateTime scheduleDate, DateTimePeriod period, IEnumerable<Guid> personIds, Paging paging)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<PersonScheduleDayReadModel> ForBulletinPersons(IEnumerable<string> shiftExchangeOfferIdList, Paging paging)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPersons(DateOnly date, IEnumerable<Guid> personIdList, Paging paging, TimeFilterInfo filterInfo = null,
			string timeSortOrder = "")
		{

			var assignments = _personAssignmentRepository.LoadAll()
				.Where(a => personIdList.Contains(a.Person.Id.Value) && date == a.Date)
				.Where(
					a =>
						filterInfo == null || filterInfo.StartTimes == null || !filterInfo.StartTimes.Any() ||
						filterInfo.StartTimes.Any(period => period.Contains(a.Period.StartDateTime)))
				.Where(
					a =>
						filterInfo == null || filterInfo.EndTimes == null || !filterInfo.EndTimes.Any() ||
						filterInfo.EndTimes.Any(period => period.Contains(a.Period.EndDateTime))).ToList();
			var persons = _personRepository.LoadAll();


			if (filterInfo != null && timeFilterHasValue(filterInfo))
			{
				var nonEmptyDayPersonSchedules = assignments.Select(a =>
				{
					if (a.DayOff() != null)
					{
						return PersonScheduleDayReadModelFactory.CreateDayOffPersonScheduleDayReadModel(a.Person, date);
					}
					var simpleLayers = a.ShiftLayers.Select(shiftLayer => new SimpleLayer
					{
						Start = shiftLayer.Period.StartDateTime,
						End = shiftLayer.Period.EndDateTime,
						Description = shiftLayer.Payload.Name
					}).ToList();

					return PersonScheduleDayReadModelFactory.CreatePersonScheduleDayReadModelWithSimpleShift(a.Person, date,
						simpleLayers);
				});

				if (!filterInfo.IsEmptyDay) return nonEmptyDayPersonSchedules;

				var emptyDayPersonSchedules = personIdList.Except(assignments.Select(x => x.Person.Id.Value))
					.Select(pid => PersonScheduleDayReadModelFactory.CreateSimplePersonScheduleDayReadModel(
						persons.First(p => p.Id.Value == pid),
						date));

				return nonEmptyDayPersonSchedules.Concat(emptyDayPersonSchedules);
			}
			return personIdList.Select(pid =>
			{
				var assignment = assignments.FirstOrDefault(a => a.Person.Id.Value == pid);
				if (assignment == default(object))
				{
					return PersonScheduleDayReadModelFactory.CreateSimplePersonScheduleDayReadModel(
						persons.First(p => p.Id.Value == pid),
						date);
				}

				if (assignment.DayOff() != null)
				{
					return PersonScheduleDayReadModelFactory.CreateDayOffPersonScheduleDayReadModel(assignment.Person, date);
				}

				var simpleLayers = assignment.ShiftLayers.Select(shiftLayer => new SimpleLayer
				{
					Start = shiftLayer.Period.StartDateTime,
					End = shiftLayer.Period.EndDateTime,
					Description = shiftLayer.Payload.Name
				}).ToList();

				return PersonScheduleDayReadModelFactory.CreatePersonScheduleDayReadModelWithSimpleShift(assignment.Person, date,
					simpleLayers);
			});
		}

		private bool timeFilterHasValue(TimeFilterInfo filter)
		{
			if (filter.IsWorkingDay)
			{
				if ((filter.StartTimes != null && filter.StartTimes.Any()) ||
					(filter.EndTimes != null && filter.EndTimes.Any()))
				{
					return true;
				}
			}

			return !(filter.IsWorkingDay && filter.IsDayOff && filter.IsEmptyDay);
		}
	}
}
