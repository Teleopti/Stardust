using System;
using System.ServiceModel;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Sdk.ClientProxies;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Win.Payroll.Forms.PayrollExportPages;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast;
using log4net;

namespace Teleopti.Ccc.Win.Forecasting.Forms.ImportForecast
{
    public class SendCommandToSdk : ISendCommandToSdk
    {
        private readonly SdkAuthentication _sdkAuthentication;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SendCommandToSdk));

        public SendCommandToSdk(SdkAuthentication sdkAuthentication)
        {
            _sdkAuthentication = sdkAuthentication;
        }

        public CommandResultDto ExecuteCommand(CommandDto command)
        {
            _sdkAuthentication.SetSdkAuthenticationHeader();
            var sdkName = StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["Sdk"];
	        var proxy = Proxy.GetProxy(sdkName);
            try
            {
                proxy.Open();
                var result = proxy.ExecuteCommand(command);
                return result;
            }
            catch (TimeoutException timeoutException)
            {
                Logger.Error(string.Concat(command.GetType(), " can't reach Sdk due to a timeout."), timeoutException);
            }
            catch (CommunicationException exception)
            {
                Logger.Error(string.Concat(command.GetType(), " can't reach Sdk."), exception);
            }
            catch (Exception exception)
            {
                Logger.Error(string.Concat(command.GetType(), " notification error."), exception);
            }
            finally
            {
                ((IDisposable)proxy).Dispose();
            }
            return new CommandResultDto();
        }
    }
}