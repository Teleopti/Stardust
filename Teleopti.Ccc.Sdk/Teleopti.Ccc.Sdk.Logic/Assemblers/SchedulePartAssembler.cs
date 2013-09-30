using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class SchedulePartAssembler : Assembler<IScheduleDay, SchedulePartDto>, ISchedulePartAssembler
    {
        private readonly IScheduleDataAssembler<IPersonAssignment, PersonAssignmentDto> _personAssignmentAssembler;
        private readonly IScheduleDataAssembler<IPersonAbsence, PersonAbsenceDto> _personAbsenceAssembler;
        private readonly ICurrentScenario _scenarioRepository;
				private readonly IScheduleDataAssembler<IPersonAssignment, PersonDayOffDto> _personDayOffAssembler;
        private readonly IScheduleDataAssembler<IPersonMeeting, PersonMeetingDto> _personMeetingAssembler;
        private readonly IScheduleDataAssembler<IPreferenceDay, PreferenceRestrictionDto> _prefRestrictionAssembler;
        private readonly IScheduleDataAssembler<IStudentAvailabilityDay, StudentAvailabilityDayDto> _studRestrictionAssembler;
        private readonly IProjectedLayerAssembler _projectedLayerAssembler;
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
    	private readonly ISdkProjectionServiceFactory _sdkProjectionServiceFactory;
	    private readonly IScheduleTagAssembler _scheduleTagAssembler;
	    public IPersonRepository PersonRepository { get; set; }
        public IScheduleRepository ScheduleRepository { get; set; }

    	public string SpecialProjection { get; set; }
    	public TimeZoneInfo TimeZone { get; set; }

		public SchedulePartAssembler(ICurrentScenario scenarioRepository,
                                    IScheduleDataAssembler<IPersonAssignment, PersonAssignmentDto> personAssignmentAssembler,
                                    IScheduleDataAssembler<IPersonAbsence, PersonAbsenceDto> personAbsenceAssembler,
																		IScheduleDataAssembler<IPersonAssignment, PersonDayOffDto> personDayOffAssembler,
                                    IScheduleDataAssembler<IPersonMeeting, PersonMeetingDto> personMeetingAssembler,
                                    IScheduleDataAssembler<IPreferenceDay, PreferenceRestrictionDto> preferenceRestrictionAssembler,
                                    IScheduleDataAssembler<IStudentAvailabilityDay, StudentAvailabilityDayDto> studRestrictionAssembler,
												IProjectedLayerAssembler projectedLayerAssembler,
												IAssembler<DateTimePeriod,DateTimePeriodDto> dateTimePeriodAssembler,
												ISdkProjectionServiceFactory sdkProjectionServiceFactory,
									IScheduleTagAssembler scheduleTagAssembler)
        {
            _scenarioRepository = scenarioRepository;
            _personDayOffAssembler = personDayOffAssembler;
            _personAssignmentAssembler = personAssignmentAssembler;
            _personAbsenceAssembler = personAbsenceAssembler;
            _personMeetingAssembler = personMeetingAssembler;
            _prefRestrictionAssembler = preferenceRestrictionAssembler;
            _studRestrictionAssembler = studRestrictionAssembler;
            _projectedLayerAssembler = projectedLayerAssembler;
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
    		_sdkProjectionServiceFactory = sdkProjectionServiceFactory;
			_scheduleTagAssembler = scheduleTagAssembler;
			SpecialProjection = string.Empty;
        }

        public override SchedulePartDto DomainEntityToDto(IScheduleDay entity)
        {
            SchedulePartDto part = fetchSchedulePart(entity);
            if (entity != null)
            {
	            var assignment = entity.PersonAssignment();
                fillProjection(part, entity);
                fillPersonAssignment(part, assignment);
                fillPersonAbsence(part, entity.PersonAbsenceCollection());
                fillPersonDayOff(part, assignment);
                fillPersonMeeting(part, entity.PersonMeetingCollection());
                FillPreference(part, entity.PersistableScheduleDataCollection().OfType<IPreferenceDay>());
                FillStudentAvailability(part, entity.PersistableScheduleDataCollection().OfType<IStudentAvailabilityDay>());
	            addScheduleTag(part, entity.ScheduleTag());
            }
            return part;
        }

				//isn't used anymore (?)
				public override IScheduleDay DtoToDomainEntity(SchedulePartDto dto)
        {
            ensureInjectionForDtoToDo();

            IScheduleDay part = fetchSchedulePart(dto);
            fillPersonAssignment(part, dto.PersonAssignmentCollection);
            fillPersonAbsence(part, dto.PersonAbsenceCollection);
            //Meetings cannot be added from dto as they are generated automatically!
            //FillPreference(part, dto.PreferenceRestriction);
            FillStudentAvailability(part, dto.StudentAvailabilityDay);
            return part;
        }

        private void ensureInjectionForDtoToDo()
        {
            if (PersonRepository == null)
                throw new InvalidOperationException("You'll need to provide a person repository");
            if (ScheduleRepository == null)
                throw new InvalidOperationException("You'll need to provide a schedule repository");
        }

        private IScheduleDay fetchSchedulePart(SchedulePartDto schedulePartDto)
        {
            IPerson person = PersonRepository.Load(schedulePartDto.PersonId);
            DateTimePeriod period = partPeriod(schedulePartDto);

	        var skaVaraPersonernaIListansFullaSchemaPeriod =
		        period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());
            IScheduleDictionary scheduleDictionary =
                ScheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(new[] { person }), new ScheduleDictionaryLoadOptions(true, false),
                                                                     skaVaraPersonernaIListansFullaSchemaPeriod,
                                                                     _scenarioRepository.Current());
            //vad ska hända här?
            //TimeZoneInfo timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById(schedulePartDto.TimeZoneId));
            //scheduleDictionary.SetTimeZone(timeZoneInfo);
            DateOnly date = schedulePartDto.Date.ToDateOnly();
            return scheduleDictionary[person].ScheduledDay(date);
        }

        private SchedulePartDto fetchSchedulePart(IScheduleDay schedulePart)
        {
            SchedulePartDto retDto = new SchedulePartDto();
            if (schedulePart != null)
            {
                retDto.PersonId = schedulePart.Person.Id.Value;
				retDto.Date = new DateOnlyDto { DateTime = schedulePart.DateOnlyAsPeriod.DateOnly };
                retDto.TimeZoneId = schedulePart.Person.PermissionInformation.DefaultTimeZone().Id;

                DateTimePeriodDto dateOnlyPeriodDto = _dateTimePeriodAssembler.DomainEntityToDto(schedulePart.DateOnlyAsPeriod.Period());
                retDto.LocalPeriod = dateOnlyPeriodDto;
            }
            return retDto;
        }

        private static DateTimePeriod partPeriod(SchedulePartDto schedulePartDto)
        {
            //todo: tidszon
            DateTime dateMedFelTidszon = TimeZoneHelper.ConvertToUtc(schedulePartDto.Date.DateTime);
            return new DateTimePeriod(dateMedFelTidszon, dateMedFelTidszon.AddDays(1));
        }

        private void fillPersonAbsence(IScheduleDay part, IEnumerable<PersonAbsenceDto> absenceDtos)
        {
            _personAbsenceAssembler.Person = part.Person;
            _personAbsenceAssembler.DefaultScenario = _scenarioRepository.Current();

            ClearAbsenceBelongingToThisPart(part);

            _personAbsenceAssembler.DtosToDomainEntities(absenceDtos).ForEach(part.Add);
        }

        private static void ClearAbsenceBelongingToThisPart(IScheduleDay part)
        {
            var view = part.SignificantPart();
            switch (view)
            {
                case SchedulePartView.FullDayAbsence:
                case SchedulePartView.ContractDayOff:
                    part.DeleteFullDayAbsence(part);
                    break;
                case SchedulePartView.Absence:
                    part.DeleteAbsence(false);
                    break;
                default:
                    part.DeleteAbsence(true);
                    break;
            }
        }

        private void fillPersonAssignment(IScheduleDay part, IEnumerable<PersonAssignmentDto> assignmentDtos)
        {
            _personAssignmentAssembler.Person = part.Person;
            _personAssignmentAssembler.DefaultScenario = _scenarioRepository.Current();
	        _personAssignmentAssembler.PartDate = part.DateOnlyAsPeriod.DateOnly;
            part.Clear<IPersonAssignment>();
            var filteredAssignmentDtos = filterEmptyAssignment(assignmentDtos);
            if (filteredAssignmentDtos != Enumerable.Empty<PersonAssignmentDto>())
                _personAssignmentAssembler.DtosToDomainEntities(filteredAssignmentDtos).ForEach(part.Add);
        }

        private static IEnumerable<PersonAssignmentDto> filterEmptyAssignment(IEnumerable<PersonAssignmentDto> assignmentDtos)
        {
            return assignmentDtos.Where(
                    personAssignmentDto =>
                    hasMainShift(personAssignmentDto) ||
                    hasOvertimeShifts(personAssignmentDto) ||
                    hasPersonShifts(personAssignmentDto)).ToList();
        }

        private static bool hasPersonShifts(PersonAssignmentDto personAssignmentDto)
        {
            return personAssignmentDto.PersonalShiftCollection.Count != 0 &&
                   personAssignmentDto.PersonalShiftCollection.All(s => s.LayerCollection.Count != 0);
        }

        private static bool hasOvertimeShifts(PersonAssignmentDto personAssignmentDto)
        {
            return personAssignmentDto.OvertimeShiftCollection.Count != 0 &&
                   personAssignmentDto.OvertimeShiftCollection.All(s => s.LayerCollection.Count != 0);
        }

        private static bool hasMainShift(PersonAssignmentDto personAssignmentDto)
        {
            return personAssignmentDto.MainShift != null &&
                   personAssignmentDto.MainShift.LayerCollection.Count != 0;
        }


        private void fillPersonAbsence(SchedulePartDto part, IEnumerable<IPersonAbsence> absences)
        {
            part.PersonAbsenceCollection.Clear();
            _personAbsenceAssembler.DomainEntitiesToDtos(absences).ForEach(part.PersonAbsenceCollection.Add);
        }

        private void fillPersonMeeting(SchedulePartDto part, IEnumerable<IPersonMeeting> meetings)
        {
            part.PersonMeetingCollection.Clear();
            _personMeetingAssembler.DomainEntitiesToDtos(meetings).ForEach(part.PersonMeetingCollection.Add);
        }

        private void fillPersonAssignment(SchedulePartDto part, IPersonAssignment assignment)
        {
            part.PersonAssignmentCollection.Clear();
					if (assignment != null)
					{
						part.PersonAssignmentCollection.Add(_personAssignmentAssembler.DomainEntityToDto(assignment));
					}
        }

        private void fillPersonDayOff(SchedulePartDto part, IPersonAssignment dayOff)
        {
            part.PersonDayOff = null;
					if (dayOff != null)
					{
						part.PersonDayOff = _personDayOffAssembler.DomainEntityToDto(dayOff);
					}
        }

        private void fillProjection(SchedulePartDto part, IScheduleDay scheduleDay)
        {
        	var proj = _sdkProjectionServiceFactory
											.CreateProjectionService(scheduleDay, SpecialProjection, TimeZone)
											.CreateProjection();
            part.ProjectedLayerCollection.Clear();
            part.ContractTime = DateTime.MinValue.Add(proj.ContractTime());
	        part.WorkTime = DateTime.MinValue.Add(proj.WorkTime());
	        part.PaidTime = DateTime.MinValue.Add(proj.PaidTime());

            if (proj.IsSatisfiedBy(VisualLayerCollectionSpecification.OneAbsenceLayer))
                part.IsFullDayAbsence = true;

            _projectedLayerAssembler.SetCurrentProjection(proj);
            _projectedLayerAssembler.DomainEntitiesToDtos(proj).ForEach(
                part.ProjectedLayerCollection.Add);
        }

    	private void FillPreference(SchedulePartDto part, IEnumerable<IPreferenceDay> preferences)
        {
            // We assume that the part is ONE day and we can only have ONE preference per day
            part.PreferenceRestriction = null;
            if (!preferences.IsEmpty())
                part.PreferenceRestriction = _prefRestrictionAssembler.DomainEntityToDto(preferences.First());
        }

        //private void FillPreference(IScheduleDay part, PreferenceRestrictionDto preference)
        //{
        //    // We assume that the part is ONE day and we can only have ONE preference per day
        //    part.Clear<IPreferenceDay>();
        //    if (preference != null)
        //    {
        //        //_prefRestrictionAssembler.Person = part.Person;
        //        //_prefRestrictionAssembler.DefaultScenario = _defaultScenario;    
        //        //_prefRestrictionAssembler.PartDate = partDate;
        //       part.Add(_prefRestrictionAssembler.DtoToDomainEntity(preference));
        //    }
        //}

        private void FillStudentAvailability(SchedulePartDto part, IEnumerable<IStudentAvailabilityDay> studentAvailabilityDays)
        {
            // We assume that the part is ONE day and we can only have ONE StudentAvailability per day
            part.StudentAvailabilityDay = null;
            if (!studentAvailabilityDays.IsEmpty())
                part.StudentAvailabilityDay = _studRestrictionAssembler.DomainEntityToDto(studentAvailabilityDays.First());
        }

        private void FillStudentAvailability(IScheduleDay part, StudentAvailabilityDayDto studentAvailabilityDay)
        {
            // We assume that the part is ONE day and we can only have ONE StudentAvailability per day
            part.Clear<IStudentAvailabilityDay>();
            if (studentAvailabilityDay != null)
            {
                //_studRestrictionAssembler.Person = part.Person;
                //_studRestrictionAssembler.DefaultScenario = _defaultScenario;
                //_studRestrictionAssembler.PartDate = partDate;
                part.Add(_studRestrictionAssembler.DtoToDomainEntity(studentAvailabilityDay));
            }
        }

		private void addScheduleTag(SchedulePartDto part, IScheduleTag scheduleTag)
		{
			part.ScheduleTag = _scheduleTagAssembler.DomainEntityToDto(scheduleTag);
		}
    }
}