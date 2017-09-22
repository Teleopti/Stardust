using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IScheduleStorageFactory
	{
		IScheduleStorage Create(IUnitOfWork unitOfWork);
	}
}