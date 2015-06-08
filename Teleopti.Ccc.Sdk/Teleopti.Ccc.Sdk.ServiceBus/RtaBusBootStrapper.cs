using System.Configuration;
using System.Threading.Tasks;
using Autofac;
using Rhino.ServiceBus;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class RtaBusBootStrapper : BusBootStrapper
	{
		public RtaBusBootStrapper(IContainer container) : base(container)
		{
		}

		protected override void OnEndStart()
		{
			Task.Factory.StartNew(() =>
			{
				var dbConnection = ConfigurationManager.ConnectionStrings["Queue"];
				QueueClearMessages.ClearMessages(dbConnection.ConnectionString, "rta");
			}).ContinueWith(_ => {
				//add RTA state checker
				var rtaChecker = new BusinessUnitStarter(() => Container.Resolve<IServiceBus>());
				rtaChecker.SendMessage();
			}, TaskContinuationOptions.OnlyOnRanToCompletion);
		}
	}
}