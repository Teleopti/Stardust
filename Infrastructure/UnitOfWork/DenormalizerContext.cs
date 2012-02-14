using System;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public class DenormalizerContext : IDisposable
	{
		private readonly ISendDenormalizeNotification _sendDenormalizeNotification;

		public DenormalizerContext(ISendDenormalizeNotification sendDenormalizeNotification)
		{
			_sendDenormalizeNotification = sendDenormalizeNotification;
			IsInActiveContext = true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "InActive")]
		[ThreadStatic]
		public static bool IsInActiveContext;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
		public void Dispose()
		{
			IsInActiveContext = false;
			_sendDenormalizeNotification.Notify();
		}
	}
}
