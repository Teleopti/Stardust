using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy
{
	public interface IJsonDeserializer
	{
		T DeserializeObject<T>(string value);
		object DeserializeObject(string value, Type type);
	}
}