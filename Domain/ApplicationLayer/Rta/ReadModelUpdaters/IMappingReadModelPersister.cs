using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public interface IMappingReadModelPersister
	{
		void Invalidate();
		void Add(Mapping mapping);
	}
}