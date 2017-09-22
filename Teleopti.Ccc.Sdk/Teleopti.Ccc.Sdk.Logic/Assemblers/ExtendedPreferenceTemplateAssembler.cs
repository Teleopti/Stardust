using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class ExtendedPreferenceTemplateAssembler : Assembler<IExtendedPreferenceTemplate,ExtendedPreferenceTemplateDto> 
    {
        private readonly IPerson _person;
        private readonly IAssembler<IPreferenceRestrictionTemplate, ExtendedPreferenceTemplateDto> _assembler;

        public ExtendedPreferenceTemplateAssembler(IPerson person, IAssembler<IPreferenceRestrictionTemplate, ExtendedPreferenceTemplateDto> assembler)
        {
            _person = person;
            _assembler = assembler;
        }

        public override ExtendedPreferenceTemplateDto DomainEntityToDto(IExtendedPreferenceTemplate entity)
        {
            var extendedPreferenceTemplateDto = _assembler.DomainEntityToDto(entity.Restriction);
            extendedPreferenceTemplateDto.Id = entity.Id;
            extendedPreferenceTemplateDto.Name = entity.Name;
            extendedPreferenceTemplateDto.DisplayColor = new ColorDto(entity.DisplayColor);
 
            return extendedPreferenceTemplateDto;
        }

        public override IExtendedPreferenceTemplate DtoToDomainEntity(ExtendedPreferenceTemplateDto dto)
        {
            var name = dto.Name;
            Color color = dto.DisplayColor.ToColor();

            var restriction = _assembler.DtoToDomainEntity(dto);
            IExtendedPreferenceTemplate extendedPreferenceTemplate = new ExtendedPreferenceTemplate(_person, restriction, name, color);
            extendedPreferenceTemplate.SetId(dto.Id);

            return extendedPreferenceTemplate;
        }
    }
}
