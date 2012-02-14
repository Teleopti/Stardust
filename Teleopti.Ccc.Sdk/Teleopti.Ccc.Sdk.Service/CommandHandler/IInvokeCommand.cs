using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.Sdk.WcfService.CommandHandler
{
    public interface IInvokeCommand
    {
        CommandResultDto Invoke(CommandDto command);
    }
}