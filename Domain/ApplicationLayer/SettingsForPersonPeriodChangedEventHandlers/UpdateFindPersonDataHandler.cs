using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.SettingsForPersonPeriodChangedEventHandlers
{
	public class UpdateFindPersonDataHandler : IHandleEvent<SettingsForPersonPeriodChangedEvent>, IRunOnHangfire
	{
		private readonly IPersonFinderReadOnlyRepository _personFinderReadOnlyRepository;

		public UpdateFindPersonDataHandler(IPersonFinderReadOnlyRepository personFinderReadOnlyRepository)
		{
			_personFinderReadOnlyRepository = personFinderReadOnlyRepository;
		}
		
		[UnitOfWork]
		public virtual void Handle(SettingsForPersonPeriodChangedEvent @event)
		{
			_personFinderReadOnlyRepository.UpdateFindPersonData(@event.IdCollection.ToArray());
		}
	}
}
