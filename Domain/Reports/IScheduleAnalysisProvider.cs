using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Reports
{
	public interface IScheduleAnalysisProvider
	{
		List<IApplicationFunction> GetScheduleAnalysisApplicationFunctions(IList<IApplicationFunction> applicationFunctions);
	}
}