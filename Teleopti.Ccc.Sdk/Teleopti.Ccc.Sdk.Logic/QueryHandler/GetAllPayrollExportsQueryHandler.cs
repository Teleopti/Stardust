using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
    public class GetAllPayrollExportsQueryHandler : IHandleQuery<GetAllPayrollExportsQueryDto,ICollection<PayrollExportDto>>
    {
        private readonly IAssembler<IPayrollExport, PayrollExportDto> _assembler;
        private readonly IPayrollExportRepository _exportRepository;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

        public GetAllPayrollExportsQueryHandler(IAssembler<IPayrollExport,PayrollExportDto> assembler, IPayrollExportRepository exportRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory)
        {
            _assembler = assembler;
            _exportRepository = exportRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public ICollection<PayrollExportDto> Handle(GetAllPayrollExportsQueryDto query)
        {
            using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
                var exports = _exportRepository.LoadAll();
                return _assembler.DomainEntitiesToDtos(exports).ToList();
            }
        }
    }
}
