using System;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface IReportUrl 
	{
		string Build(string foreignId, Guid businessId);
	}
}