using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider
{
	public interface IReportsProvider
	{
		IEnumerable<IApplicationFunction> GetReports();
	}
}