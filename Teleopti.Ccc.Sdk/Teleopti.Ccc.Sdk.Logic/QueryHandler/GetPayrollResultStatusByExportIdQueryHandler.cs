﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
    public class GetPayrollResultStatusByExportIdQueryHandler: IHandleQuery<GetPayrollResultStatusByExportIdQueryDto, ICollection<PayrollResultDto>>
    {
        private readonly IAssembler<IPayrollResult, PayrollResultDto> _assembler;
        private readonly IPayrollResultRepository _resultRepository;
        private readonly IPayrollExportRepository _exportRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public GetPayrollResultStatusByExportIdQueryHandler(IAssembler<IPayrollResult, PayrollResultDto> assembler, IPayrollResultRepository resultRepository, IPayrollExportRepository exportRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _assembler = assembler;
            _resultRepository = resultRepository;
            _exportRepository = exportRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ICollection<PayrollResultDto> Handle(GetPayrollResultStatusByExportIdQueryDto query)
        {
            using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var export = _exportRepository.Load(query.PayrollExportId);
                var results = _resultRepository.GetPayrollResultsByPayrollExport(export);
                return _assembler.DomainEntitiesToDtos(results).ToList();
            }
        }
    }
}
