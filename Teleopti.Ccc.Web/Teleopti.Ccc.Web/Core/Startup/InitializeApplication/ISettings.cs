namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	public interface ISettings
	{
		string ConfigurationFilesPath();
		string MessageBroker();
		bool MessageBrokerLongPolling();
	}
}