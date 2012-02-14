using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IDenormalizerQueueRepository
	{
		IEnumerable<DenormalizerQueueItem> DequeueDenormalizerMessages(IBusinessUnit businessUnit);
	}
}