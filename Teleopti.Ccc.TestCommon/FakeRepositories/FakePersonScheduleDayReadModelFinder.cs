using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonScheduleDayReadModelFinder : IPersonScheduleDayReadModelFinder
	{
		private IPersonRepository _personRepository;
		private bool _isInitilized;
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly IAbsenceRepository _absenceRepository;
		private readonly ICurrentScenario _scenario;
		private IPersonAssignment _excludedAssignment;

		public FakePersonScheduleDayReadModelFinder(IPersonAssignmentRepository personAssignmentRepository,
			IPersonRepository personRepository, IPersonRequestRepository personRequestRepository, IPersonAbsenceRepository personAbsenceRepository, IAbsenceRepository absenceRepository, ICurrentScenario scenario)
			: this(personAssignmentRepository, personRepository, personAbsenceRepository, absenceRepository, scenario)
		{
			_personRequestRepository = personRequestRepository;
		}

		public FakePersonScheduleDayReadModelFinder(IPersonAssignmentRepository personAssignmentRepository, IPersonRepository personRepository, IPersonAbsenceRepository personAbsenceRepository, IAbsenceRepository absenceRepository, ICurrentScenario scenario)
		{
			_personAssignmentRepository = personAssignmentRepository;
			_personRepository = personRepository;
			_personAbsenceRepository = personAbsenceRepository;
			_absenceRepository = absenceRepository;
			_scenario = scenario;
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPerson(DateOnlyPeriod period, Guid personId)
		{
			throw new NotImplementedException();
		}

		public PersonScheduleDayReadModel ForPerson(DateOnly date, Guid personId)
		{
			var assignment = _personAssignmentRepository.LoadAll().SingleOrDefault(a => personId == a.Person.Id.Value && date == a.Date);
			if (assignment == null || assignment.Equals(_excludedAssignment))
				return null;
			var persons = _personRepository.LoadAll();

			if (assignment.DayOff() != null)
			{
				return PersonScheduleDayReadModelFactory.CreateDayOffPersonScheduleDayReadModel(assignment.Person, date);
			}
			PersonScheduleDayReadModel readModel;
			if (assignment.ShiftLayers.Any())
			{
				var simpleLayers = assignment.ShiftLayers.Select(shiftLayer => new SimpleLayer
				{
					Start = shiftLayer.Period.StartDateTime,
					End = shiftLayer.Period.EndDateTime,
					Description = shiftLayer.Payload.Name,
					Color = ColorTranslator.ToHtml(Color.FromArgb(shiftLayer.Payload.DisplayColor.ToArgb())),
					Minutes = (int) shiftLayer.Period.EndDateTime.Subtract(shiftLayer.Period.StartDateTime).TotalMinutes
				}).ToList();
				readModel = PersonScheduleDayReadModelFactory.CreatePersonScheduleDayReadModelWithSimpleShift(assignment.Person,
					date, simpleLayers);
			}
			else
			{
				readModel = PersonScheduleDayReadModelFactory.CreateSimplePersonScheduleDayReadModel(persons.First(p => p.Id.Value == personId), date);
			}
			
			return readModel;
		}

		public void Exclude(IPersonAssignment assignment)
		{
			_excludedAssignment = assignment;
		}

		public bool IsInitialized()
		{
			return _isInitilized;
		}

		public void SetIsInitialized(bool input)
		{
			_isInitilized = input;
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPeople(DateTimePeriod period, IEnumerable<Guid> personIds)
		{
			var schedules = new List<PersonScheduleDayReadModel>();
			foreach (var date in period.ToDateOnlyPeriod(TimeZoneInfo.Utc).DayCollection())
			{
				schedules.AddRange(ForPersons(date, personIds, Paging.Empty));
			}

			return schedules;
		}

		public IEnumerable<PersonScheduleDayReadModel> ForBulletinPersons(IEnumerable<string> shiftExchangeOfferIdList,
			Paging paging)
		{
			var personRequests = _personRequestRepository.LoadAll();
			var shiftExchangeOffers = personRequests.Select(x => x.Request).OfType<ShiftExchangeOffer>()
				.Where(x => shiftExchangeOfferIdList.Contains(x.ShiftExchangeOfferId));
			return shiftExchangeOffers.Select(x => new PersonScheduleDayReadModel
			{
				PersonId = x.Person.Id.Value
			});
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPersons(DateOnly date, IEnumerable<Guid> personIdList, Paging paging, TimeFilterInfo filterInfo = null,
			string timeSortOrder = "")
		{

			var assignments = _personAssignmentRepository.LoadAll()
				.Where(a => personIdList.Contains(a.Person.Id.Value) && date == a.Date)
				.Where(a => filterInfo == null || filterInfo.StartTimes == null ||!filterInfo.StartTimes.Any() || filterInfo.StartTimes.Any(period => period.Contains(a.Period.StartDateTime)))
				.Where(a => filterInfo == null || filterInfo.EndTimes == null || !filterInfo.EndTimes.Any() || filterInfo.EndTimes.Any(period => period.Contains(a.Period.EndDateTime))).ToList();
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
				var isFullDayAbsence = false;
				if (_personAbsenceRepository != null && _absenceRepository != null && _scenario != null)
				{

					var period = date.ToDateTimePeriod(TimeZoneInfo.Utc);
					var absence = _absenceRepository.LoadAll().FirstOrDefault();
					var scenario = _scenario.Current();
					isFullDayAbsence = _personAbsenceRepository.FindExact(assignment.Person, period, absence, scenario).Count > 0;
				}
				return PersonScheduleDayReadModelFactory.CreatePersonScheduleDayReadModelWithSimpleShift(assignment.Person, date,
					simpleLayers, isFullDayAbsence);
			});
		}

		public IEnumerable<PersonScheduleDayReadModel> ForPeople(DateTimePeriod period, IEnumerable<PersonInfoForShiftTradeFilter> personInfos)
		{
			throw new NotImplementedException();
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
