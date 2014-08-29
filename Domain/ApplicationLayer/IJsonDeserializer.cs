namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IJsonDeserializer
	{
		T DeserializeObject<T>(string value);
	}
}