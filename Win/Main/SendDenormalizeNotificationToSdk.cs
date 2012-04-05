using System.Threading;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast;

namespace Teleopti.Ccc.Win.Main
{
	public class SendDenormalizeNotificationToSdk : ISendDenormalizeNotification
	{
	    private readonly ISendCommandToSdk _sendCommandToSdk;

	    public SendDenormalizeNotificationToSdk(ISendCommandToSdk sendCommandToSdk)
	    {
	        _sendCommandToSdk = sendCommandToSdk;
	    }

	    public void Notify()
		{
		    if (DenormalizerContext.IsInActiveContext) return;

		    ThreadPool.QueueUserWorkItem(sendToSdk);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		private void sendToSdk(object state)
		{
		    _sendCommandToSdk.ExecuteCommand(new DenormalizeNotificationCommandDto());
	    }
	}
}