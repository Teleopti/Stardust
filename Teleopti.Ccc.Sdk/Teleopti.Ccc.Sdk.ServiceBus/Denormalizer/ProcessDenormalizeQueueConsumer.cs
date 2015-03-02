using System;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class ProcessDenormalizeQueueConsumer : ConsumerOf<ProcessDenormalizeQueue>
	{
		private readonly IServiceBus _serviceBus;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IDenormalizerQueueRepository _denormalizerQueueRepository;

		public ProcessDenormalizeQueueConsumer(IServiceBus serviceBus,ICurrentUnitOfWorkFactory unitOfWorkFactory, IDenormalizerQueueRepository denormalizerQueueRepository)
		{
			_serviceBus = serviceBus;
			_unitOfWorkFactory = unitOfWorkFactory;
			_denormalizerQueueRepository = denormalizerQueueRepository;
		}

		public void Consume(ProcessDenormalizeQueue message)
		{
			using (var unitOfWork = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var messages = _denormalizerQueueRepository.DequeueDenormalizerMessages(((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity).BusinessUnit);
				var messagelist = messages.Select(m => SerializationHelper.Deserialize(Type.GetType(m.Type, true, true), m.Message));
				foreach (var m in messagelist)
				{
					_serviceBus.Send(m);
				}
				unitOfWork.PersistAll();
			}
		}
	}

}