using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Stardust.IntegrationTest.Stardust
{
	[Ignore("WIP"),StardustTest]
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
		public ICurrentUnitOfWork CurrentUnitOfWork;
			

	  
		[Test]
		public void ShouldRunEndToEndAbsenceRequest()
		{
			Now.Is("2016-02-25 08:00".Utc());
			WithUnitOfWork.Do(() =>
			{
				var person =  PersonRepository.LoadAll().FirstOrDefault(x => x.Name.FirstName.Equals("Ola"));
				var absence = AbsenceRepository.LoadAll().FirstOrDefault();
				var personRequest = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 02, 26, 12, 2016, 02, 26, 13)));
				personRequest.Subject = "I am going to have fun";
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

			startServiceBusAndPublishTick();

			performLevel1Assert();
			
			Thread.Sleep(600000);
		}

		private void performLevel1Assert()
		{
			////check in job queue
			//var connectionString = InfraTestConfigReader.ConnectionString;
			//using (var connection = new SqlConnection(connectionString))
			//{
			//	connection.Open();
			//	using (var command = new SqlCommand("select serialized from Stardust.JobQueue", connection))
			//	{
			//		while (true)
			//		{
						
			//			using (var reader = command.ExecuteReader())
			//			{
			//				if (reader.HasRows)
			//				{
			//					reader.Read();
			//					var jsonData = reader.GetString(0);
			//					NewMultiAbsenceRequestsCreatedEvent storedEvent = JsonConvert.DeserializeObject<NewMultiAbsenceRequestsCreatedEvent>(jsonData);
			//					break;
			//				}
								
			//			}
			//			Thread.Sleep(1000);

			//		}
					
			//	}
			//}



			
			//while there is a not a job in stardust
			// sleep for 1 sec
			//when i have a job do level1 assert on the content of the job

			//while there is not an end date on the job
			//sleep for 1 sec
			//when i have an end date do the level2 assert to check the status of the job

			//assert the personrequest as a level 3 assert on message and status

		}

		private void startServiceBusAndPublishTick()
		{
			var host = new ServiceBusRunner(i => { }, ConfigReader);
			host.Start();
			Thread.Sleep(2000);

			EventPublisher.Publish(new TenantMinuteTickEvent());
		}
	}
}