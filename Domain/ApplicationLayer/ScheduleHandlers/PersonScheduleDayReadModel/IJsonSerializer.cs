namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public interface IJsonSerializer
	{
		string SerializeObject(object obj);
	}
}