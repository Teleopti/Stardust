using System.Linq;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Stardust.IntegrationTest.Stardust
{
	[Ignore("WIP"), StardustTest]
	public class AbsenceRequestEndToEndTest
	{
		public WithUnitOfWork WithUnitOfWork;
		public IPersonRepository PersonRepository;
		public IAbsenceRepository AbsenceRepository;
		public IPersonRequestRepository PersonRequestRepository;
		public MutableNow Now;
		public IQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public IEventPublisher EventPublisher;
		public IConfigReader ConfigReader;

	  
		[Test]
		public void ShouldRunEndToEndAbsenceRequest()
		{
			Now.Is("2016-02-25 08:00".Utc());
			WithUnitOfWork.Do(() =>
			{
				var person =  PersonRepository.LoadAll().FirstOrDefault(x => x.Name.FirstName.Equals("Ola"));
				var absence = AbsenceRepository.LoadAll().FirstOrDefault();
				var personRequest = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 02, 26, 12, 2016, 02, 26, 13)));
				personRequest.Pending();
				PersonRequestRepository.Add(personRequest);
				
				
				var queuedReq = (new QueuedAbsenceRequest
				{
					PersonRequest = personRequest.Id.GetValueOrDefault(),
					MandatoryValidators = RequestValidatorsFlag.None,
					StartDateTime = personRequest.Request.Period.StartDateTime,
					EndDateTime = personRequest.Request.Period.EndDateTime,
					Created = Now.UtcDateTime().AddMinutes(-20)
				});
				QueuedAbsenceRequestRepository.Add(queuedReq);

			});
			startServiceBus();
			Thread.Sleep(10000);
			EventPublisher.Publish(new TenantMinuteTickEvent());

			

			Thread.Sleep(600000);
		}

		private void startServiceBus()
		{
			var host = new ServiceBusRunner(i => { }, ConfigReader);
			host.Start();
		}
	}
}