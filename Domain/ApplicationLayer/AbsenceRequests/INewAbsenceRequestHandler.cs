using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public interface INewAbsenceRequestHandler
	{
		void Handle(NewAbsenceRequestCreatedEvent @event);
	}
}