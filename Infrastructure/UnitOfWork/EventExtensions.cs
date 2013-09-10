using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public static class EventExtensions
	{
		public static void SetMessageDetail(this IRaptorDomainMessageInfo @event, ICurrentIdentity currentIdentity)
		{
			var identity = currentIdentity.Current();

			setValuesFromIdentity(@event,identity);
		}

        public static void SetMessageDetail(this IEvent @event, ICurrentIdentity currentIdentity)
		{
			var domainEvents = @event as IRaptorDomainMessageInfo;
            if (domainEvents!=null)
                domainEvents.SetMessageDetail(currentIdentity);
		}

		private static void setValuesFromIdentity(IRaptorDomainMessageInfo message, ITeleoptiIdentity identity)
		{
			message.BusinessUnitId = identity.BusinessUnit.Id.GetValueOrDefault();
			message.Datasource = identity.DataSource.Application.Name;
			message.Timestamp = DateTime.UtcNow;
		}
	}
}