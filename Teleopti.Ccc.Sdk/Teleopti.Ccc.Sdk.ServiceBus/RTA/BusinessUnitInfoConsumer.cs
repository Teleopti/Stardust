using System;
using System.Collections.Generic;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.PulseLoop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using log4net;
using Teleopti.Interfaces.Messages.Rta;

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
			using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				persons = _businessRepository.LoadAllPersonsWithExternalLogOn(message.LogOnBusinessUnitId, DateOnly.Today);	
			}

			foreach (var person in persons)
			{
				try
				{
					_serviceBus.Send(new PersonActivityChangePulseEvent
						{
							LogOnDatasource = message.LogOnDatasource,
							LogOnBusinessUnitId = message.LogOnBusinessUnitId,
							PersonId = person,
							Timestamp = DateTime.UtcNow,
							PersonHaveExternalLogOn = true
						});

					Logger.DebugFormat(
						"Sending PersonActivityChangePulseEvent Message to Service Bus for Person={0} and Bussiness Unit Id={1}", person,
						message.LogOnBusinessUnitId);
				}
				catch (Exception exception)
				{
					Logger.Error("Exception occured while sending PersonActivityChangePulseEvent message to Service Bus", exception);
					return;
				}
			}
		}
	}
}
