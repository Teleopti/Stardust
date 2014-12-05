using System;

namespace Teleopti.Interfaces
{
	public interface IJsonDeserializer
	{
		T DeserializeObject<T>(string value);
		object DeserializeObject(string value, Type type);
	}
}