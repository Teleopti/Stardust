using System;
using System.Collections.Generic;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Rta;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus.Rta
{
	public class BusinessUnitInfoConsumer : ConsumerOf<BusinessUnitInfo>
	{
		private readonly IServiceBus _serviceBus;
		private static readonly ILog Logger = LogManager.GetLogger(typeof (BusinessUnitInfoConsumer));
		private  IBusinessUnitRepository _businessRepository;
		private readonly IUnitOfWorkFactory unitOfWorkFactory;

		public BusinessUnitInfoConsumer(IServiceBus serviceBus, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_serviceBus = serviceBus;
			this.unitOfWorkFactory = unitOfWorkFactory;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void Consume(BusinessUnitInfo message)
		{
			IList<Guid> persons;
			using (var uow = unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				_businessRepository = new BusinessUnitRepository(uow);
				persons = _businessRepository.LoadAllPersonsWithExternalLogOn(message.BusinessUnitId, DateOnly.Today);
			}

			foreach (var person in persons)
			{
				try
				{
					_serviceBus.Send(new PersonActivityStarting
						{
							Datasource = message.Datasource,
							BusinessUnitId = message.BusinessUnitId,
							PersonId = person,
							Timestamp = DateTime.UtcNow
						});

					Logger.DebugFormat(
						"Sending PersonActivityStarting Message to Service Bus for Person={0} and Bussiness Unit Id={1}", person,
						message.BusinessUnitId);
				}
				catch (Exception exception)
				{
					Logger.Error("Exception occured while sending PersonActivityStarting message to Service Bus", exception);
					return;
				}
			}
		}
	}
}
