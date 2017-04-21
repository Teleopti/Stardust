using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Reports
{
	public interface IReportNavigationModel
	{
		IEnumerable<IMatrixFunctionGroup> PermittedCategorizedReportFunctions { get; }
		IEnumerable<IApplicationFunction> PermittedCustomReportFunctions { get; }
		IEnumerable<IApplicationFunction> PermittedRealTimeReportFunctions { get; }
		IEnumerable<IApplicationFunction> PermittedReportFunctions { get; }
	}
}