namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	public interface ISettings
	{
		string nhibConfPath();
		string MessageBroker();
		string MessageBrokerLongPolling();
	}
}