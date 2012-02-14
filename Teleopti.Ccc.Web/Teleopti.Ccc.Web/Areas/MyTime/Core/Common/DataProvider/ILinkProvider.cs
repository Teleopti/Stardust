using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface ILinkProvider
	{
		string TextRequestLink(Guid value);
	}
}