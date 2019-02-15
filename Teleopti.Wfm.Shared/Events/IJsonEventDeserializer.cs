using System;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IJsonEventDeserializer
	{
		object DeserializeEvent(string value, Type type);
		T DeserializeEvent<T>(string value);
	}
}