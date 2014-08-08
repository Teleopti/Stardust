using System;
using System.Collections.Generic;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus.Rta
{
	public class BusinessUnitInfoConsumer : ConsumerOf<StartUpBusinessUnit>
	{
		private readonly IServiceBus _serviceBus;
		private static readonly ILog Logger = LogManager.GetLogger(typeof (BusinessUnitInfoConsumer));
		private readonly IBusinessUnitRepository _businessRepository;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public BusinessUnitInfoConsumer(IServiceBus serviceBus, ICurrentUnitOfWorkFactory unitOfWorkFactory, IBusinessUnitRepository businessRepository)
		{
			_serviceBus = serviceBus;
			_unitOfWorkFactory = unitOfWorkFactory;
			_businessRepository = businessRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public void Consume(StartUpBusinessUnit message)
		{
			IList<Guid> persons;
			using (_unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
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
							Timestamp = DateTime.UtcNow,
							PersonHaveExternalLogOn = true
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
