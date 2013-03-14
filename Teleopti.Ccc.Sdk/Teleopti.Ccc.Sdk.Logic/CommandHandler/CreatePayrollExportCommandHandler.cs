﻿using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class CreatePayrollExportCommandHandler : IHandleCommand<CreatePayrollExportCommandDto>
    {
        private readonly IPayrollResultFactory _payrollResultFactory;

        public CreatePayrollExportCommandHandler(IPayrollResultFactory payrollResultFactory)
        {
            _payrollResultFactory = payrollResultFactory;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(CreatePayrollExportCommandDto command)
        {
            var id = _payrollResultFactory.RunPayrollOnBus(command.PayrollExportDto);
            command.Result = new CommandResultDto() { AffectedId = id, AffectedItems = 1};
        }
    }
}
