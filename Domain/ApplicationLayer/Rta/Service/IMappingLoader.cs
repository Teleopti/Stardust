using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IMappingLoader
	{
		IEnumerable<Mapping> Load();
	}
}