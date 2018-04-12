using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
	[DisabledBy(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
	public class PersonScheduleDayReadModelUpdaterHangfire :
		IHandleEvent<ProjectionChangedEvent>,
		IHandleEvent<ProjectionChangedEventForPersonScheduleDay>,
		IRunOnHangfire,
		IPersonScheduleDayReadModelUpdaterHangfire
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

	[RemoveMeWithToggle(Toggles.ResourcePlanner_SpeedUpEvents_75415)]
	public interface IPersonScheduleDayReadModelUpdaterHangfire
	{
		void Handle(ProjectionChangedEvent @event);
		void Handle(ProjectionChangedEventForPersonScheduleDay @event);
	}
}