using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	public class PersonScheduleDayReadModelUpdaterHangfire :
		IHandleEvent<ProjectionChangedEvent>,
		IHandleEvent<ProjectionChangedEventForPersonScheduleDay>,
		IRunOnHangfire
	{
		private readonly PersonScheduleDayReadModelUpdaterPersister _personScheduleDayReadModelUpdaterPersister;
		
		public PersonScheduleDayReadModelUpdaterHangfire(PersonScheduleDayReadModelUpdaterPersister personScheduleDayReadModelUpdaterPersister)
		{
			_personScheduleDayReadModelUpdaterPersister = personScheduleDayReadModelUpdaterPersister;
		}

		[UnitOfWork]
		public virtual void Handle(ProjectionChangedEvent @event)
		{
			_personScheduleDayReadModelUpdaterPersister.Execute(@event);
		}

		[UnitOfWork]
		public virtual void Handle(ProjectionChangedEventForPersonScheduleDay @event)
		{
			_personScheduleDayReadModelUpdaterPersister.Execute(@event);
		}
	}
}