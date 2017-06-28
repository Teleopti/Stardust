using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	public class PublishInitializeReadModelEventHandler : 
		IHandleEvent<PublishInitializeReadModelEvent>,
		IRunOnHangfire
	{
		private readonly IDataSourceScope _dataSourceScope;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEventPublisher _eventPublisher;
		private readonly Func<ICurrentUnitOfWork, IBusinessUnitRepository> _businessUnitRepository;

		public PublishInitializeReadModelEventHandler(IDataSourceScope dataSourceScope, ICurrentUnitOfWorkFactory unitOfWorkFactory, IEventPublisher eventPublisher, Func<ICurrentUnitOfWork, IBusinessUnitRepository> businessUnitRepository)
		{
			_dataSourceScope = dataSourceScope;
			_unitOfWorkFactory = unitOfWorkFactory;
			_eventPublisher = eventPublisher;
			_businessUnitRepository = businessUnitRepository;
		}

		public void Handle(PublishInitializeReadModelEvent @event)
		{
			_eventPublisher.Publish(@event.Tenants.SelectMany(tenant => createEventForTenant(tenant,
				@event.InitialLoadScheduleProjectionStartDate, @event.InitialLoadScheduleProjectionEndDate)).ToArray());
		}

		private IEnumerable<IEvent> createEventForTenant(string tenantName, int start, int end)
		{
			using (_dataSourceScope.OnThisThreadUse(tenantName))
			{
				using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					var businessUnits = _businessUnitRepository(new ThisUnitOfWork(uow)).LoadAll();
					foreach (var businessUnit in businessUnits)
					{
						yield return new InitialLoadScheduleProjectionEvent
						{
							LogOnDatasource = tenantName,
							LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault(),
							StartDays = start,
							EndDays = end
						};
					}
				}
			}
		}
	}
}