using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public interface IScheduleDictionaryBatchPersister
	{
		void Persist(IScheduleDictionary scheduleDictionary);
	}
}