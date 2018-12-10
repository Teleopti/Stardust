using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class TextRequestAssembler : Assembler<IRequest,TextRequestDto>
    {
        private readonly IUserCultureProvider _cultureInfoProvider;
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;

        public TextRequestAssembler(IUserCultureProvider cultureInfoProvider, IAssembler<DateTimePeriod,DateTimePeriodDto> dateTimePeriodAssembler)
        {
            _cultureInfoProvider = cultureInfoProvider;
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
        }

        public override TextRequestDto DomainEntityToDto(IRequest entity)
        {
            TextRequestDto textRequestDto = new TextRequestDto();
            TextRequest textRequest = entity as TextRequest;
            if (textRequest != null)
            {
                textRequestDto.Id = textRequest.Id;
                textRequestDto.Period = _dateTimePeriodAssembler.DomainEntityToDto(textRequest.Period);
                textRequestDto.Details = entity.GetDetails(_cultureInfoProvider.Culture);
            }
            return textRequestDto;
        }

        public override IRequest DtoToDomainEntity(TextRequestDto dto)
        {
            DateTimePeriod period = _dateTimePeriodAssembler.DtoToDomainEntity(dto.Period);
            TextRequest textRequest = new TextRequest(period);
            return textRequest;
        }
    }
}