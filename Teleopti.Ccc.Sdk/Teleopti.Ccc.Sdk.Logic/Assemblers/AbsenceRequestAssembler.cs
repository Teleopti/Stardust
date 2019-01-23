using System;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class AbsenceRequestAssembler : Assembler<IAbsenceRequest,AbsenceRequestDto>
    {
        private readonly IUserCultureProvider _cultureInfoProvider;
        private readonly IAssembler<IAbsence, AbsenceDto> _absenceAssembler;
        private readonly IAssembler<DateTimePeriod, DateTimePeriodDto> _dateTimePeriodAssembler;

        public AbsenceRequestAssembler(IUserCultureProvider cultureInfoProvider, IAssembler<IAbsence,AbsenceDto> absenceAssembler, IAssembler<DateTimePeriod,DateTimePeriodDto> dateTimePeriodAssembler)
        {
            _cultureInfoProvider = cultureInfoProvider;
            _absenceAssembler = absenceAssembler;
            _dateTimePeriodAssembler = dateTimePeriodAssembler;
        }

        public override AbsenceRequestDto DomainEntityToDto(IAbsenceRequest entity)
        {
            AbsenceRequestDto absenceRequestDto = new AbsenceRequestDto();
            if (entity != null)
            {
                absenceRequestDto.Id = entity.Id;
                absenceRequestDto.Period = _dateTimePeriodAssembler.DomainEntityToDto(entity.Period);
                absenceRequestDto.Absence = _absenceAssembler.DomainEntityToDto(entity.Absence);
                absenceRequestDto.Details = entity.GetDetails(_cultureInfoProvider.Culture);
            }
            return absenceRequestDto;
        }

        public override IAbsenceRequest DtoToDomainEntity(AbsenceRequestDto dto)
        {
            EnsureInjectionForDtoToDo();
            IAbsence absence = _absenceAssembler.DtoToDomainEntity(dto.Absence);

            DateTimePeriod period = _dateTimePeriodAssembler.DtoToDomainEntity(dto.Period);
            IAbsenceRequest absenceRequest = new AbsenceRequest(absence, period);

            return absenceRequest;
        }

        private void EnsureInjectionForDtoToDo()
        {
            if (_absenceAssembler == null)
                throw new InvalidOperationException("You'll need to provide a absence repository");
        }
    }

    public interface IUserCultureProvider
    {
        CultureInfo Culture { get; }
    }

    public class UserCultureProvider : IUserCultureProvider
    {
        public CultureInfo Culture
        {
            get
            {
                return
                    TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
            }
        }
    }
}