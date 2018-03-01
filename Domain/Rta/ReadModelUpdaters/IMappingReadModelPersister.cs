using System.Collections.Generic;
using Teleopti.Ccc.Domain.Rta.Service;

namespace Teleopti.Ccc.Domain.Rta.ReadModelUpdaters
{
	public interface IMappingReadModelPersister
	{
		void Invalidate();
		bool Invalid();
		void Persist(IEnumerable<Mapping> mappings);
	}
}