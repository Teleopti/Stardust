using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IScheduleStorageFactory
	{
		IScheduleStorage Create(IUnitOfWork unitOfWork);
	}
}