using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence.States.Infrastructure
{
	public interface IMappingReadModelPersister
	{
		void Invalidate();
		bool Invalid();
		void Persist(IEnumerable<Mapping> mappings);
	}
}