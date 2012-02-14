using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.Sdk.WcfService.CommandHandler
{
    public interface IHandleCommand<TCommand>
    {
       CommandResultDto Handle(TCommand command);
    }
}
