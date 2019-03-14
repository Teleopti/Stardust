using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ExternalMeetingRepository : Repository<ExternalMeeting>, IExternalMeetingRepository
	{
		public ExternalMeetingRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork, null, null)
		{
		}
	}
}