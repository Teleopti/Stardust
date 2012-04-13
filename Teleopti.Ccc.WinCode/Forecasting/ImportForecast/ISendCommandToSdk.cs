using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.WinCode.Forecasting.ImportForecast
{
    public interface ISendCommandToSdk
    {
        CommandResultDto ExecuteCommand(CommandDto command);
    }
}