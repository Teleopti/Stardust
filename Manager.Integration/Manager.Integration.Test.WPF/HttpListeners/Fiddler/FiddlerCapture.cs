using System;
using Fiddler;

namespace Manager.Integration.Test.WPF.HttpListeners.Fiddler
{
	public class FiddlerCapture : IDisposable
	{
		public EventHandler<FiddlerCaptureInformation> NewDataCapturedEventHandler;

		public FiddlerCapture(FiddlerCaptureUrlConfiguration fiddlerCaptureUrlConfiguration)
		{
			if (fiddlerCaptureUrlConfiguration == null)
			{
				throw new ArgumentNullException("fiddlerCaptureUrlConfiguration");
			}

			FiddlerCaptureUrlConfiguration = fiddlerCaptureUrlConfiguration;
		}

		public FiddlerCaptureUrlConfiguration FiddlerCaptureUrlConfiguration { get; private set; }

		public void Dispose()
		{
			Stop();
		}

		private void InvokeNewDataCapturedEventHandler(FiddlerCaptureInformation fiddlerCaptureInformation)
		{
			if (NewDataCapturedEventHandler != null)
			{
				NewDataCapturedEventHandler(this, fiddlerCaptureInformation);
			}
		}

		public void Start()
		{
			FiddlerApplication.AfterSessionComplete += FiddlerApplicationOnAfterSessionComplete;

			FiddlerApplication.Startup(8888, true, true, true);
		}

		public void Stop()
		{
			FiddlerApplication.AfterSessionComplete -= FiddlerApplicationOnAfterSessionComplete;

			if (FiddlerApplication.IsStarted())
			{
				FiddlerApplication.Shutdown();
			}
		}

		private void FiddlerApplicationOnAfterSessionComplete(Session oSession)
		{
			// Ignore HTTPS connect requests
			if (oSession.RequestMethod == "CONNECT")
			{
				return;
			}

			if (oSession.hostname.Contains("localhost"))
			{
				InvokeNewDataCapturedEventHandler(new FiddlerCaptureInformation
				{
					Uri = oSession.fullUrl
				});

			}
		}
	}
}