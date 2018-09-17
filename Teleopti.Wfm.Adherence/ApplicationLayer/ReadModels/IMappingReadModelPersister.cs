using System.Collections.Generic;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels
{
	public interface IMappingReadModelPersister
	{
		void Invalidate();
		bool Invalid();
		void Persist(IEnumerable<Mapping> mappings);
	}
}