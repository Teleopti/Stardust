using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	[DisabledBy(Toggles.People_ImprovePersonAccountAccuracy_74914)]
	public class PersonEmploymentChangedEventEmptyHandler  : IHandleEvent<PersonEmploymentChangedEvent>, IRunOnHangfire
	{
		public virtual void Handle(PersonEmploymentChangedEvent @event)
		{
			
		}
	}

	[EnabledBy(Toggles.People_ImprovePersonAccountAccuracy_74914)]
	public class PersonEmploymentChangedEventHandler  : IHandleEvent<PersonEmploymentChangedEvent>, IRunOnHangfire
	{
		private readonly CalculatePersonAccount _calculatePersonAccount;

		public PersonEmploymentChangedEventHandler(CalculatePersonAccount calculatePersonAccount)
		{
			_calculatePersonAccount = calculatePersonAccount;
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(PersonEmploymentChangedEvent @event)
		{
			_calculatePersonAccount.Calculate(@event);
		}
	}
}
