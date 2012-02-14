using System;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PersonSkillPeriodAssembler : Assembler<IPersonPeriod,PersonSkillPeriodDto>
    {
        public override PersonSkillPeriodDto DomainEntityToDto(IPersonPeriod entity)
        {
            PersonSkillPeriodDto dto = new PersonSkillPeriodDto();
            dto.Id = entity.Id;
            dto.PersonId = entity.Parent.Id.GetValueOrDefault(Guid.Empty);
            dto.DateFrom = new DateOnlyDto(entity.StartDate);
            dto.DateTo = new DateOnlyDto(entity.EndDate());

            foreach (IPersonSkill personSkill in entity.PersonSkillCollection)
            {
                dto.SkillCollection.Add(PersonSkillDoToDto(personSkill));
            }

            return dto;
        }

        public override IPersonPeriod DtoToDomainEntity(PersonSkillPeriodDto dto)
        {
            throw new NotSupportedException("This function is not supported yet.");
        }

        private static Guid PersonSkillDoToDto(IPersonSkill entity)
        {
            return entity.Skill.Id.GetValueOrDefault(Guid.Empty);
        }
    }
}
