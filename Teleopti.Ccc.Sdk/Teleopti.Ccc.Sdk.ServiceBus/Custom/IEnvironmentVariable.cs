namespace Teleopti.Ccc.Sdk.ServiceBus.Custom
{
	public interface IEnvironmentVariable
	{
		string GetValue(string environmentKey);
	}
}
