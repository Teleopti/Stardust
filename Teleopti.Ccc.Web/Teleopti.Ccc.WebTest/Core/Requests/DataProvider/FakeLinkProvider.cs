using System;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	public class FakeLinkProvider : ILinkProvider
	{
		public string RequestDetailLink(Guid value)
		{
			return value.ToString();
		}
	}
}