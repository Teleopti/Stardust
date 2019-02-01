using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IInsightsReportRepository: IRepository<IInsightsReport>
	{
		IList<IInsightsReport> GetAllValidReports();
	}
}