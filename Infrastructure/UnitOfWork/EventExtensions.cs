using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public static class EventExtensions
	{
		public static void SetMessageDetail(this IRaptorDomainMessageInfo @event)
		{
			var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity);

			setValuesFromIdentity(@event,identity);
		}

		public static void SetMessageDetail(this IEvent @event)
		{
			var domainEvents = @event as IRaptorDomainMessageInfo;
            if (domainEvents!=null)
                domainEvents.SetMessageDetail();
		}

		private static void setValuesFromIdentity(IRaptorDomainMessageInfo message, ITeleoptiIdentity identity)
		{
			message.BusinessUnitId = identity.BusinessUnit.Id.GetValueOrDefault();
			message.Datasource = identity.DataSource.Application.Name;
			message.Timestamp = DateTime.UtcNow;
		}
	}
}