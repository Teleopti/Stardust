using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PayrollResultAssembler : Assembler<IPayrollResult, PayrollResultDto>
    {
        private readonly IPayrollResultRepository _payrollResultRepository;
        private readonly IAssembler<IPayrollResultDetail, PayrollResultDetailDto> _resultDetailAssembler;

        public PayrollResultAssembler(IPayrollResultRepository payrollResultRepository, IAssembler<IPayrollResultDetail, PayrollResultDetailDto> resultDetailAssembler)
        {
            _payrollResultRepository = payrollResultRepository;
            _resultDetailAssembler = resultDetailAssembler;
        }

        public override PayrollResultDto DomainEntityToDto(IPayrollResult entity)
        {
            var resultDto = new PayrollResultDto();
            resultDto.Id = entity.Id;
            resultDto.Timestamp = entity.Timestamp;
            resultDto.HasError = entity.HasError();
            resultDto.FinishedOk =  entity.FinishedOk;
            resultDto.IsWorking =  entity.IsWorking();

			var details = _resultDetailAssembler.DomainEntitiesToDtos(entity.Details);
        	foreach (var detailDto in details)
        	{
        		resultDto.Details.Add(detailDto);
        	}
            
            return resultDto;
        }

        public override IPayrollResult DtoToDomainEntity(PayrollResultDto dto)
        {
            IPayrollResult result = null;
            if (dto.Id.HasValue)
            {
                result = _payrollResultRepository.Get(dto.Id.Value);
            }
            return result;
        }
    }
}