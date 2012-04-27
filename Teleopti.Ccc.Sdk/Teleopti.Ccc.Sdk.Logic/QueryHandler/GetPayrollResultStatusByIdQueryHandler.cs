using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
    public class GetPayrollResultStatusByIdQueryHandler : IHandleQuery<GetPayrollResultStatusByIdQueryDto, ICollection<PayrollResultDto>>
    {
        private readonly IAssembler<IPayrollResult, PayrollResultDto> _assembler;
        private readonly IPayrollResultRepository _resultRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public GetPayrollResultStatusByIdQueryHandler(IAssembler<IPayrollResult,PayrollResultDto> assembler, IPayrollResultRepository resultRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _assembler = assembler;
            _resultRepository = resultRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public ICollection<PayrollResultDto> Handle(GetPayrollResultStatusByIdQueryDto query)
        {
            using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var result = _resultRepository.Load(query.PayrollResultId);
                return new [] {_assembler.DomainEntityToDto(result)};
            }
        }
    }
}
