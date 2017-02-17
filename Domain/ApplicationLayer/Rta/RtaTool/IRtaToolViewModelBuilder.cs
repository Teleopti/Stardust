using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.RtaTool
{
	public interface IRtaToolViewModelBuilder
	{
		IEnumerable<RtaToolViewModel> Build();
	}
}