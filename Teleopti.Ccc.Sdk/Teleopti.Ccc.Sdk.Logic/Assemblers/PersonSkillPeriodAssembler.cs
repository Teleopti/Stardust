using System;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PersonSkillPeriodAssembler : Assembler<IPersonPeriod,PersonSkillPeriodDto>
    {
        public override PersonSkillPeriodDto DomainEntityToDto(IPersonPeriod entity)
        {
            var dto = new PersonSkillPeriodDto
	            {
		            Id = entity.Id,
		            PersonId = entity.Parent.Id.GetValueOrDefault(Guid.Empty),
		            DateFrom = new DateOnlyDto {DateTime = entity.StartDate},
		            DateTo = new DateOnlyDto {DateTime = entity.EndDate()}
	            };

	        foreach (var personSkill in entity.PersonSkillCollection)
            {
                dto.SkillCollection.Add(personSkillDoToDto(personSkill));
            }

            return dto;
        }

        public override IPersonPeriod DtoToDomainEntity(PersonSkillPeriodDto dto)
        {
            throw new NotSupportedException("This function is not supported yet.");
        }

        private static Guid personSkillDoToDto(IPersonSkill entity)
        {
            return entity.Skill.Id.GetValueOrDefault(Guid.Empty);
        }
    }
}
