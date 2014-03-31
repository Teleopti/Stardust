using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider
{
	public interface IReportsProvider
	{
		IEnumerable<IApplicationFunction> GetReports();
	}
}