using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class ShiftTradeRequestAssembler : Assembler<IShiftTradeRequest,ShiftTradeRequestDto>
    {
        private readonly IUserCultureProvider _cultureInfoProvider;
        private readonly IPersonRequestCheckAuthorization _authorization;
        private readonly IShiftTradeRequestStatusChecker _shiftTradeRequestStatusChecker;
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;

		public ShiftTradeRequestAssembler(IUserCultureProvider cultureInfoProvider, IPersonRequestCheckAuthorization authorization, IAssembler<DateTimePeriod, DateTimePeriodDto> dateTimePeriodAssembler, IBatchShiftTradeRequestStatusChecker shiftTradeRequestStatusChecker)
        {
            _cultureInfoProvider = cultureInfoProvider;
            _authorization = authorization;
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
			_shiftTradeRequestStatusChecker = shiftTradeRequestStatusChecker;
        }

        public override ShiftTradeRequestDto DomainEntityToDto(IShiftTradeRequest entity)
        {
            EnsureInjectionDoToDto();
            ShiftTradeRequestDto shiftTradeRequestDto = new ShiftTradeRequestDto();
            if (entity != null)
            {
                shiftTradeRequestDto.Id = entity.Id;
                shiftTradeRequestDto.Period = _dateTimePeriodAssembler.DomainEntityToDto(entity.Period);
                shiftTradeRequestDto.ShiftTradeStatus = (ShiftTradeStatusDto)entity.GetShiftTradeStatus(_shiftTradeRequestStatusChecker);
                shiftTradeRequestDto.TypeDescription = entity.RequestTypeDescription;
                shiftTradeRequestDto.Details = entity.GetDetails(_cultureInfoProvider.Culture);
            }
            return shiftTradeRequestDto;
        }

        private void EnsureInjectionDoToDto()
        {
            if (_shiftTradeRequestStatusChecker==null)
                throw new InvalidOperationException("A shift trade request status checker must be set for assembling of Dto.");
        }

        public override IShiftTradeRequest DtoToDomainEntity(ShiftTradeRequestDto dto)
        {
            IShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>());
            shiftTradeRequest.SetId(dto.Id);
            shiftTradeRequest.SetShiftTradeStatus((ShiftTradeStatus)dto.ShiftTradeStatus,_authorization);

            return shiftTradeRequest;
        }
    }
}