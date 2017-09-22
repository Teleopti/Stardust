using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
    public class GetPayrollResultStatusByIdQueryHandler : IHandleQuery<GetPayrollResultStatusByIdQueryDto, ICollection<PayrollResultDto>>
    {
        private readonly IAssembler<IPayrollResult, PayrollResultDto> _assembler;
        private readonly IPayrollResultRepository _resultRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

        public GetPayrollResultStatusByIdQueryHandler(IAssembler<IPayrollResult,PayrollResultDto> assembler, IPayrollResultRepository resultRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
        {
            _assembler = assembler;
            _resultRepository = resultRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ICollection<PayrollResultDto> Handle(GetPayrollResultStatusByIdQueryDto query)
        {
            using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
                var result = _resultRepository.Load(query.PayrollResultId);
                return new [] {_assembler.DomainEntityToDto(result)};
            }
        }
    }
}
