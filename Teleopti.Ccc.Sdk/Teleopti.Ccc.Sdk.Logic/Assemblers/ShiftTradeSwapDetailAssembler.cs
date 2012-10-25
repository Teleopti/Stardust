using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class ShiftTradeSwapDetailAssembler : Assembler<IShiftTradeSwapDetail,ShiftTradeSwapDetailDto>
    {
        public IAssembler<IPerson, PersonDto> PersonAssembler { get; set; }
        public IAssembler<IScheduleDay,SchedulePartDto> SchedulePartAssembler { get; set; }

        public override ShiftTradeSwapDetailDto DomainEntityToDto(IShiftTradeSwapDetail entity)
        {
            ShiftTradeSwapDetailDto shiftTradeSwapDetailDto = new ShiftTradeSwapDetailDto();
            if (entity != null)
            {
                shiftTradeSwapDetailDto.Id = entity.Id;
                shiftTradeSwapDetailDto.PersonFrom = PersonAssembler.DomainEntityToDto(entity.PersonFrom);
                shiftTradeSwapDetailDto.PersonTo = PersonAssembler.DomainEntityToDto(entity.PersonTo);
				shiftTradeSwapDetailDto.DateFrom = new DateOnlyDto { DateTime = entity.DateFrom };
				shiftTradeSwapDetailDto.DateTo = new DateOnlyDto { DateTime = entity.DateTo };
                shiftTradeSwapDetailDto.SchedulePartFrom =
                    SchedulePartAssembler.DomainEntityToDto(entity.SchedulePartFrom);
                shiftTradeSwapDetailDto.SchedulePartTo =
                    SchedulePartAssembler.DomainEntityToDto(entity.SchedulePartTo);
                shiftTradeSwapDetailDto.ChecksumFrom = entity.ChecksumFrom;
                shiftTradeSwapDetailDto.ChecksumTo = entity.ChecksumTo;
            }
            return shiftTradeSwapDetailDto;
        }

        public override IShiftTradeSwapDetail DtoToDomainEntity(ShiftTradeSwapDetailDto dto)
        {
            IPerson personFrom = PersonAssembler.DtoToDomainEntity(dto.PersonFrom);
            IPerson personTo = PersonAssembler.DtoToDomainEntity(dto.PersonTo);
            IShiftTradeSwapDetail shiftTradeSwapDetail = new ShiftTradeSwapDetail(personFrom, personTo,
                                                                                  new DateOnly(dto.DateFrom.DateTime),
                                                                                  new DateOnly(dto.DateTo.DateTime));
            shiftTradeSwapDetail.ChecksumFrom = dto.ChecksumFrom;
            shiftTradeSwapDetail.ChecksumTo = dto.ChecksumTo;
            shiftTradeSwapDetail.SetId(dto.Id);

            return shiftTradeSwapDetail;
        }
    }
}