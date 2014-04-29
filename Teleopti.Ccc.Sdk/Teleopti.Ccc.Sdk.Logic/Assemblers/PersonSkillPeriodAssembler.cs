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
                var skillDoToDto = personSkillDoToDto(personSkill);
                dto.SkillCollection.Add(skillDoToDto.SkillId);
                dto.PersonSkillCollection.Add(skillDoToDto);
            }

            return dto;
        }

        public override IPersonPeriod DtoToDomainEntity(PersonSkillPeriodDto dto)
        {
            throw new NotSupportedException("This function is not supported yet.");
        }

        private static PersonSkillDto personSkillDoToDto(IPersonSkill entity)
        {
            return new PersonSkillDto
            {
                Active = entity.Active,
                Proficiency = entity.SkillPercentage.Value,
                SkillId = entity.Skill.Id.GetValueOrDefault()
            };
        }
    }
}
