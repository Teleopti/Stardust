using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Stardust.IntegrationTest.Stardust
{
	[StardustTest]
	public class AbsenceRequestEndToEndTest
	{
		public WithUnitOfWork WithUnitOfWork;
		public IPersonRepository PersonRepository;
		public IAbsenceRepository AbsenceRepository;
		public IPersonRequestRepository PersonRequestRepository;
		public MutableNow Now;
		public IQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;

		[Ignore("WIP"),Test]
		public void StardustEx()
		{
			startServiceBus();
		}

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
		}

		private void startServiceBus()
		{
			var configReader = new TestConfigReader();
			configReader.ConfigValues.Add("ManagerLocation", TestSiteConfigurationSetup.URL.AbsoluteUri + @"StardustDashboard/");
			configReader.ConfigValues.Add("NumberOfNodes", "1");
			
			var host = new ServiceBusRunner(i => { }, configReader);
			host.Start();
			//the test will stop in 10 sec
			Thread.Sleep(60000);
		}
	}

	public class TestConfigReader : ConfigReader
	{
		public readonly Dictionary<string, string> ConfigValues = new Dictionary<string, string>();
		
		public override string AppConfig(string name)
		{
			ConfigValues.TryGetValue(name, out var value);
			return value ?? base.AppConfig(name);
		}

		public override string ConnectionString(string name)
		{
			throw new System.NotImplementedException();
		}
	}
}