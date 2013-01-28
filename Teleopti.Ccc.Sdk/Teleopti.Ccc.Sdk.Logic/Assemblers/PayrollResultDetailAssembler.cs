﻿using System;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PayrollResultDetailAssembler : Assembler<IPayrollResultDetail, PayrollResultDetailDto>
    {
        public override PayrollResultDetailDto DomainEntityToDto(IPayrollResultDetail entity)
        {
            var detail = new PayrollResultDetailDto();
            detail.DetailLevel = entity.DetailLevel;
            detail.ExceptionMessage = entity.ExceptionMessage;
            detail.ExceptionStackTrace = entity.ExceptionStackTrace;
            detail.Message = entity.Message;
            detail.Timestamp = entity.Timestamp;
            return detail;
        }

        public override IPayrollResultDetail DtoToDomainEntity(PayrollResultDetailDto dto)
        {
			var detail = new PayrollResultDetail(dto.DetailLevel, dto.Message, dto.Timestamp, new Exception(dto.ExceptionMessage));
            return detail;
        }
    }
}