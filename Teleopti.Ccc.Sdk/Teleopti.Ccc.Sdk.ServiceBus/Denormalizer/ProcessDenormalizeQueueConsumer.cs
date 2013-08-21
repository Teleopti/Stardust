using System;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Util;
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
				var messages = _denormalizerQueueRepository.DequeueDenormalizerMessages(((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit);
				var messagelist = messages.Select(m => Newtonsoft.Json.JsonConvert.DeserializeObject(m.Message.ToUncompressedString(), Type.GetType(m.Type, true, true)));
				foreach (var m in messagelist)
				{
					_serviceBus.Send(m);
				}
				unitOfWork.PersistAll();
			}
		}
	}

}