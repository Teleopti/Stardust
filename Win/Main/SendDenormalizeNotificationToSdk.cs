using System;
using System.ServiceModel;
using System.Threading;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.ClientProxies;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Win.Payroll.Forms.PayrollExportPages;
using log4net;

namespace Teleopti.Ccc.Win.Main
{
	public class SendDenormalizeNotificationToSdk : ISendDenormalizeNotification
	{
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SendDenormalizeNotificationToSdk));

		public void Notify()
		{
		    if (DenormalizerContext.IsInActiveContext) return;

		    ThreadPool.QueueUserWorkItem(sendToSdk);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private static void sendToSdk(object state)
	    {
	        var proxy = new Proxy();
            try
            {
                var sdkAuthentication = new SdkAuthentication();
                sdkAuthentication.SetSdkAuthenticationHeader();

                proxy.Open();
                proxy.ExecuteCommand(new DenormalizeNotificationCommandDto());
            }
            catch (TimeoutException timeoutException)
            {
                Logger.Error("Denormalization message can't reach Sdk due to a timeout.", timeoutException);
            }
            catch (CommunicationException exception)
            {
                Logger.Error("Denormalization message can't reach Sdk.", exception);
            }
            catch (Exception exception)
            {
                Logger.Error("Denormalization notification error.", exception);
            }
	        finally
	        {
	            ((IDisposable) proxy).Dispose();
	        }
	    }
	}
}