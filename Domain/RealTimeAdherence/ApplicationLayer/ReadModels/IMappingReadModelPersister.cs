using System.Collections.Generic;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels
{
	public interface IMappingReadModelPersister
	{
		void Invalidate();
		bool Invalid();
		void Persist(IEnumerable<Mapping> mappings);
	}
}