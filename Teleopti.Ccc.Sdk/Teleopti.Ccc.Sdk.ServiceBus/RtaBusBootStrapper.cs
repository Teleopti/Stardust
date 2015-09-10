using System.Configuration;
using System.Threading.Tasks;
using Autofac;
using Rhino.ServiceBus;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class RtaBusBootStrapper : BusBootStrapper
	{
		public RtaBusBootStrapper(IContainer container) : base(container)
		{
		}

		protected override void OnEndStart()
		{
			base.OnEndStart();

			Task.Run(() =>
			{
				var dbConnection = ConfigurationManager.ConnectionStrings["Queue"];
				QueueClearMessages.ClearMessages(dbConnection.ConnectionString, "rta");
			}).ContinueWith(_ => {
				//add RTA state checker
				var rtaChecker = new BusinessUnitStarter(() => Container.Resolve<IServiceBus>(), Container.Resolve<IDataSourceForTenant>());
				rtaChecker.SendMessage();
			}, TaskContinuationOptions.OnlyOnRanToCompletion);
		}
	}
}