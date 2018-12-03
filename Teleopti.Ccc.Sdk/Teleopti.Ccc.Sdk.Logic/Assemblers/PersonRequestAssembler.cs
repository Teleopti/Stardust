using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PersonRequestAssembler : Assembler<IPersonRequest,PersonRequestDto>
    {
        private readonly IAssembler<IRequest, TextRequestDto> _textRequestAssembler;
        private readonly IAssembler<IAbsenceRequest, AbsenceRequestDto> _absenceRequestAssembler;
        private readonly IAssembler<IShiftTradeRequest, ShiftTradeRequestDto> _shiftTradeRequestAssembler;
        private readonly IAssembler<IShiftTradeSwapDetail, ShiftTradeSwapDetailDto> _shiftTradeSwapDetailAssembler;
        private readonly IAssembler<IPerson, PersonDto> _personAssembler;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IBatchShiftTradeRequestStatusChecker _shiftTradeRequestStatusChecker;
		private readonly IUserTimeZone _userTimeZone;
		
        public PersonRequestAssembler(IAssembler<IRequest, TextRequestDto> textRequestAssembler, IAssembler<IAbsenceRequest, AbsenceRequestDto> absenceRequestAssembler, IAssembler<IShiftTradeRequest,ShiftTradeRequestDto> shiftTradeRequestAssembler, IAssembler<IShiftTradeSwapDetail, ShiftTradeSwapDetailDto> shiftTradeSwapDetailAssembler, IAssembler<IPerson, PersonDto> personAssembler, IPersonRequestRepository personRequestRepository, IBatchShiftTradeRequestStatusChecker shiftTradeRequestStatusChecker, IUserTimeZone userTimeZone)
        {
            _textRequestAssembler = textRequestAssembler;
            _personAssembler = personAssembler;
			_personRequestRepository = personRequestRepository;
			_shiftTradeRequestStatusChecker = shiftTradeRequestStatusChecker;
			_userTimeZone = userTimeZone;
			_absenceRequestAssembler = absenceRequestAssembler;
            _shiftTradeRequestAssembler = shiftTradeRequestAssembler;
            _shiftTradeSwapDetailAssembler = shiftTradeSwapDetailAssembler;
        }

        public override System.Collections.Generic.IEnumerable<PersonRequestDto> DomainEntitiesToDtos(System.Collections.Generic.IEnumerable<IPersonRequest> entityCollection)
        {
            _shiftTradeRequestStatusChecker.StartBatch(entityCollection);
            
            var result = base.DomainEntitiesToDtos(entityCollection).ToList();
            
            _shiftTradeRequestStatusChecker.EndBatch();

            return result;
        }

        public override PersonRequestDto DomainEntityToDto(IPersonRequest entity)
        {
            var personRequestDto = new PersonRequestDto();
            personRequestDto.Id = entity.Id;
            personRequestDto.Message = entity.GetMessage(new NormalizeText());
            personRequestDto.Subject = entity.GetSubject(new NormalizeText());

            var statusId = PersonRequest.GetUnderlyingStateId(entity);
	        if (statusId == 4) statusId = 1;
			personRequestDto.RequestStatus = (RequestStatusDto)statusId;

			var timeZone = _userTimeZone.TimeZone();
            personRequestDto.RequestedDate = entity.RequestedDate;
			personRequestDto.CreatedDate = entity.CreatedOn.HasValue ? TimeZoneHelper.ConvertFromUtc(entity.CreatedOn.Value, timeZone) : DateTime.MinValue;
            personRequestDto.UpdatedOn =  entity.UpdatedOn.HasValue ? TimeZoneHelper.ConvertFromUtc(entity.UpdatedOn.Value, timeZone) : DateTime.MinValue;
            personRequestDto.RequestedDateLocal = TimeZoneHelper.ConvertFromUtc(entity.RequestedDate, timeZone);
            personRequestDto.Person = _personAssembler.DomainEntityToDto(entity.Person);
            personRequestDto.CanDelete = entity.IsEditable;
            personRequestDto.Request = null;
            personRequestDto.DenyReason = entity.DenyReason;
            var request = entity.Request;

            var shiftTradeRequest = request as IShiftTradeRequest;
            var absenceRequest = request as IAbsenceRequest;
            if (absenceRequest!=null)
            {
                personRequestDto.Request = _absenceRequestAssembler.DomainEntityToDto(absenceRequest);
            }
            else if (shiftTradeRequest!=null)
            {
                var shiftTradeRequestDto = _shiftTradeRequestAssembler.DomainEntityToDto(shiftTradeRequest);
                foreach (var shiftTradeSwapDetail in shiftTradeRequest.ShiftTradeSwapDetails)
                {
                    shiftTradeRequestDto.ShiftTradeSwapDetails.Add(_shiftTradeSwapDetailAssembler.DomainEntityToDto(shiftTradeSwapDetail));
                }
                personRequestDto.Request = shiftTradeRequestDto;
                if (isNotWaitingForInitiator(shiftTradeRequestDto))
                {
                    personRequestDto.CanDelete = false;
                }
            }
            else if (request is TextRequest)
            {
                personRequestDto.Request = _textRequestAssembler.DomainEntityToDto(request);
            }
            
            return personRequestDto;
        }

        private static bool isNotWaitingForInitiator(ShiftTradeRequestDto shiftTradeRequestDto)
        {
            return shiftTradeRequestDto.ShiftTradeStatus != ShiftTradeStatusDto.OkByMe && 
                   shiftTradeRequestDto.ShiftTradeStatus != ShiftTradeStatusDto.Referred;
        }

        public override IPersonRequest DtoToDomainEntity(PersonRequestDto dto)
        {
            var personRequestDo = dto.Id.HasValue ? _personRequestRepository.Load(dto.Id.Value) : null;

            if (personRequestDo == null)
            {
                var person = _personAssembler.DtoToDomainEntity(dto.Person);
                personRequestDo = new PersonRequest(person);
            }

            // Person Request Properties
            personRequestDo.Subject = dto.Subject;
            personRequestDo.TrySetMessage(dto.Message);

            //get new or modified Request part
            if (dto.Request is TextRequestDto)
                personRequestDo.Request = _textRequestAssembler.DtoToDomainEntity((TextRequestDto) dto.Request);

            else if (dto.Request is AbsenceRequestDto)
                personRequestDo.Request = _absenceRequestAssembler.DtoToDomainEntity((AbsenceRequestDto) dto.Request);

            else if (dto.Request is ShiftTradeRequestDto)
            {
                var shiftTradeRequestDto = (ShiftTradeRequestDto)dto.Request;
                var shiftTradeRequest =
                    _shiftTradeRequestAssembler.DtoToDomainEntity(shiftTradeRequestDto);
                shiftTradeRequest.ClearShiftTradeSwapDetails();
                foreach (var shiftTradeSwapDetailDto in shiftTradeRequestDto.ShiftTradeSwapDetails)
                {
                    shiftTradeRequest.AddShiftTradeSwapDetail(
                        _shiftTradeSwapDetailAssembler.DtoToDomainEntity(shiftTradeSwapDetailDto));
                }
                personRequestDo.Request = shiftTradeRequest;
            }

            return personRequestDo;
        }
    }
}