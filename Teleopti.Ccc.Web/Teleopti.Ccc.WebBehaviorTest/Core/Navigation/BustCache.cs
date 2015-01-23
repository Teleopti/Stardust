using System;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Navigation
{
	public class BustCache : INavigationInterceptor
	{
		public void Before(GotoArgs args)
		{
			var url = args.Uri.ToString();
			if (url.Contains("?"))
				url = url.Replace("?", string.Format("?{0}&", Guid.NewGuid()));
			else if (url.Contains("#"))
				url = url.Replace("#", string.Format("?{0}#", Guid.NewGuid()));
			else
				url = string.Concat(url, string.Format("?{0}", Guid.NewGuid()));
			args.Uri = new Uri(url);
		}

		public void After(GotoArgs args)
		{
		}
	}
}