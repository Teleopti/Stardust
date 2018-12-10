using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class SchedulePartAssembler : Assembler<IScheduleDay, SchedulePartDto>, ISchedulePartAssembler
    {
        private readonly IScheduleDataAssembler<IPersonAssignment, PersonAssignmentDto> _personAssignmentAssembler;
        private readonly IScheduleDataAssembler<IPersonAbsence, PersonAbsenceDto> _personAbsenceAssembler;
				private readonly IScheduleDataAssembler<IPersonAssignment, PersonDayOffDto> _personDayOffAssembler;
        private readonly IScheduleDataAssembler<IPersonMeeting, PersonMeetingDto> _personMeetingAssembler;
        private readonly IProjectedLayerAssembler _projectedLayerAssembler;
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;
    	private readonly ISdkProjectionServiceFactory _sdkProjectionServiceFactory;
	    private readonly IScheduleTagAssembler _scheduleTagAssembler;
	    public IPersonRepository PersonRepository { get; set; }
        public IScheduleStorage ScheduleStorage { get; set; }

    	public string SpecialProjection { get; set; }
    	public TimeZoneInfo TimeZone { get; set; }

		public SchedulePartAssembler(IScheduleDataAssembler<IPersonAssignment, PersonAssignmentDto> personAssignmentAssembler,
                                    IScheduleDataAssembler<IPersonAbsence, PersonAbsenceDto> personAbsenceAssembler,
																		IScheduleDataAssembler<IPersonAssignment, PersonDayOffDto> personDayOffAssembler,
                                    IScheduleDataAssembler<IPersonMeeting, PersonMeetingDto> personMeetingAssembler,
												IProjectedLayerAssembler projectedLayerAssembler,
												IAssembler<DateTimePeriod,DateTimePeriodDto> dateTimePeriodAssembler,
												ISdkProjectionServiceFactory sdkProjectionServiceFactory,
									IScheduleTagAssembler scheduleTagAssembler)
        {
            _personDayOffAssembler = personDayOffAssembler;
            _personAssignmentAssembler = personAssignmentAssembler;
            _personAbsenceAssembler = personAbsenceAssembler;
            _personMeetingAssembler = personMeetingAssembler;
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
	            addScheduleTag(part, entity.ScheduleTag());
            }
            return part;
        }
		
	    public override IScheduleDay DtoToDomainEntity(SchedulePartDto dto)
	    {
		    throw new NotSupportedException();
	    }
		
        private SchedulePartDto fetchSchedulePart(IScheduleDay schedulePart)
        {
            SchedulePartDto retDto = new SchedulePartDto();
            if (schedulePart != null)
            {
                retDto.PersonId = schedulePart.Person.Id.Value;
				retDto.Date = new DateOnlyDto { DateTime = schedulePart.DateOnlyAsPeriod.DateOnly.Date };
                retDto.TimeZoneId = schedulePart.Person.PermissionInformation.DefaultTimeZone().Id;

                DateTimePeriodDto dateOnlyPeriodDto = _dateTimePeriodAssembler.DomainEntityToDto(schedulePart.DateOnlyAsPeriod.Period());
                retDto.LocalPeriod = dateOnlyPeriodDto;
            }
            return retDto;
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
	        if (assignment == null) return;
	        
			var domainEntityToDto = _personAssignmentAssembler.DomainEntityToDto(assignment);
	        if (domainEntityToDto != null)
		        part.PersonAssignmentCollection.Add(domainEntityToDto);
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

            _projectedLayerAssembler.SetCurrentProjection(proj, scheduleDay.Person);
            _projectedLayerAssembler.DomainEntitiesToDtos(proj).ForEach(
                part.ProjectedLayerCollection.Add);
        }

		private void addScheduleTag(SchedulePartDto part, IScheduleTag scheduleTag)
		{
			part.ScheduleTag = _scheduleTagAssembler.DomainEntityToDto(scheduleTag);
		}
    }
}