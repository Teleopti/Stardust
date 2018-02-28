using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public interface IMappingReadModelPersister
	{
		void Invalidate();
		bool Invalid();
		void Persist(IEnumerable<Mapping> mappings);
	}
}