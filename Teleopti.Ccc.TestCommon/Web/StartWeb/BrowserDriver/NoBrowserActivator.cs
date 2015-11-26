using System;

namespace Teleopti.Ccc.TestCommon.Web.StartWeb.BrowserDriver
{
	public class NoBrowserActivator : IBrowserActivator
	{
		private readonly IBrowserInteractions _browserInteractions;

		public NoBrowserActivator()
		{
			_browserInteractions = new NoBrowserInteractions();
		}

		public void SetTimeout(TimeSpan timeout)
		{
		}

		public void Start(TimeSpan timeout, TimeSpan retry)
		{
		}

		public IBrowserInteractions GetInteractions()
		{
			return _browserInteractions;
		}

		public void Close()
		{
		}
	}
}