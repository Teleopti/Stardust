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

        public CommandResultDto Handle(CreatePayrollExportCommandDto command)
        {
            var id = _payrollResultFactory.RunPayrollOnBus(command.PayrollExportDto);
            return new CommandResultDto() { AffectedId = id, AffectedItems = 1};
        }
    }
}
