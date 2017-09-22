using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class StudentAvailabilityAssembler : ScheduleDataAssembler<IStudentAvailabilityDay, StudentAvailabilityDayDto>
    {
        public IAssembler<IPerson, PersonDto> PersonAssembler { get; set; }

        public override StudentAvailabilityDayDto DomainEntityToDto(IStudentAvailabilityDay entity)
        {
            var dto = new StudentAvailabilityDayDto();
			dto.RestrictionDate = new DateOnlyDto { DateTime = entity.RestrictionDate.Date };
            dto.Person = PersonAssembler.DomainEntityToDto(entity.Person);
			if (entity.NotAvailable) return dto;

            foreach (var availabilityRestriction in entity.RestrictionCollection)
            {
                var restrictionDto = new StudentAvailabilityRestrictionDto();
                restrictionDto.Id = availabilityRestriction.Id;
                restrictionDto.StartTimeLimitation = new TimeLimitationDto
                                                         {
                                                             MinTime =
                                                                 availabilityRestriction.StartTimeLimitation.StartTime,
                                                             MaxTime =
                                                                 availabilityRestriction.StartTimeLimitation.EndTime
                                                         };
                restrictionDto.EndTimeLimitation = new TimeLimitationDto
                                                       {
                                                           MinTime =
                                                               availabilityRestriction.EndTimeLimitation.StartTime,
                                                           MaxTime =
                                                               availabilityRestriction.EndTimeLimitation.EndTime
                                                       };
                restrictionDto.WorkTimeLimitation = new TimeLimitationDto
                                                        {
                                                            MinTime =
                                                                availabilityRestriction.WorkTimeLimitation.StartTime,
                                                            MaxTime =
                                                                availabilityRestriction.WorkTimeLimitation.EndTime
                                                        };
                restrictionDto.LimitationEndTimeString = availabilityRestriction.EndTimeLimitation.EndTimeString;
                restrictionDto.LimitationStartTimeString = availabilityRestriction.StartTimeLimitation.StartTimeString;
                
                dto.StudentAvailabilityRestrictions.Add(restrictionDto);
            }
            return dto;
        }

        protected override void EnsureInjectionForDtoToDo()
        {
        }

        protected override IStudentAvailabilityDay DtoToDomainEntityAfterValidation(StudentAvailabilityDayDto dto)
        {
            IList<IStudentAvailabilityRestriction> restrictions = new List<IStudentAvailabilityRestriction>();

            if (dto.StudentAvailabilityRestrictions != null && dto.StudentAvailabilityRestrictions.Count > 0)
            {
                foreach (var restriction in dto.StudentAvailabilityRestrictions)
                {
                    IStudentAvailabilityRestriction studentAvailabilityRestriction =
                        new StudentAvailabilityRestriction();
                    if (dto.Id.HasValue) studentAvailabilityRestriction.SetId(dto.Id.Value);
                    studentAvailabilityRestriction.StartTimeLimitation = new StartTimeLimitation(restriction.StartTimeLimitation.MinTime, restriction.StartTimeLimitation.MaxTime);
                    studentAvailabilityRestriction.EndTimeLimitation = new EndTimeLimitation(restriction.EndTimeLimitation.MinTime, restriction.EndTimeLimitation.MaxTime);
                    studentAvailabilityRestriction.WorkTimeLimitation = new WorkTimeLimitation(restriction.WorkTimeLimitation.MinTime, restriction.WorkTimeLimitation.MaxTime);
                    restrictions.Add(studentAvailabilityRestriction);
                }
            }
            IPerson person = PersonAssembler.DtoToDomainEntity(dto.Person);
            IStudentAvailabilityDay entity = new StudentAvailabilityDay(person, dto.RestrictionDate.ToDateOnly(), restrictions);
            entity.NotAvailable = dto.NotAvailable;
            return entity;
        }
    }
}
