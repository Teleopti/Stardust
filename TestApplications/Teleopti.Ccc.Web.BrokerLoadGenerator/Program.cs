using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.BrokerLoadGenerator
{
	class Program
	{
		static void Main(string[] args)
		{
			var scenarioId = Guid.Parse("3348D359-D15F-4069-9277-A24200ACEB3D");
			var businessUnitId = Guid.Parse("C087E9B6-5F52-4969-968A-A24200ACE742");
			var dataSouce = "Teleopti WFM";
			var schedulingScreenDate = DateTime.UtcNow;
			var url = "http://wfmrc2/TeleoptiWFM/Web";
			var message = new Message
			{
				DomainType = typeof(IScheduleChangedMessage).Name,
				DomainReferenceId = Subscription.IdToString(scenarioId),
				StartDate = Subscription.DateToString(schedulingScreenDate),
				EndDate = Subscription.DateToString(schedulingScreenDate),
				DataSource = dataSouce,
				BusinessUnitId = Subscription.IdToString(businessUnitId),
				DomainUpdateType = (int)DomainUpdateType.NotApplicable,
				DomainQualifiedType = typeof(IScheduleChangedMessage).AssemblyQualifiedName,
			};
			var client = new HttpClient();

			var serializedMessage = JsonConvert.SerializeObject(message);
			var httpResponseMessage = client
				.PostAsync(url+ "/MessageBroker/NotifyClients", new StringContent(serializedMessage, Encoding.UTF8, "application/json"))
				.GetAwaiter()
				.GetResult();
		}
	}
}
