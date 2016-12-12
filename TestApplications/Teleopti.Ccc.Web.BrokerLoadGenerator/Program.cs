using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.BrokerLoadGenerator
{
	class Program
	{
		static void Main(string[] args)
		{
			var s = new Stopwatch();
			s.Start();

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
			var client = new HttpClient
			{
				Timeout = TimeSpan.FromMinutes(10)
			};

			var serializedMessage = JsonConvert.SerializeObject(message);

			var posts = from i in Enumerable.Range(1, 100)
				select client
					.PostAsync(url + "/MessageBroker/NotifyClients", new StringContent(serializedMessage, Encoding.UTF8, "application/json"));

			Console.WriteLine($"waiting...");
			Task.WaitAll(posts.ToArray());
			Console.WriteLine("Messages sent, took {0}", s.Elapsed);
			Console.ReadKey();
		}
	}
}
