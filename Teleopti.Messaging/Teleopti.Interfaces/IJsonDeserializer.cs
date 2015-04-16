namespace Teleopti.Interfaces
{
	public interface IJsonDeserializer
	{
		T DeserializeObject<T>(string value);
	}
}