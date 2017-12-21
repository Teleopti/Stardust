using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public interface IExternalPerformancePersister
	{
		void Persist(ExternalPerformanceInfoProcessResult result);
	}
}
