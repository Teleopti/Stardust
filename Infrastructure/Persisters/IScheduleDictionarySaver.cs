using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public interface IScheduleDictionarySaver
	{
		IScheduleDictionaryPersisterResult MarkForPersist(IUnitOfWork unitOfWork, IScheduleRepository scheduleRepository, IDifferenceCollection<IPersistableScheduleData> scheduleChange);
	}
}