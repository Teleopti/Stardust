using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class SessionSpecificDataStringSerializer : ISessionSpecificDataStringSerializer
	{
		public string Serialize(SessionSpecificData data)
		{
			var formatter = new BinaryFormatter();
			using (var stream = new MemoryStream())
			{
				formatter.Serialize(stream, data);
				var bytes = stream.ToArray();
				return Convert.ToBase64String(bytes);	
			}
		}

		public SessionSpecificData Deserialize(string stringData)
		{
			if (string.IsNullOrEmpty(stringData))
				return null;
			var formatter = new BinaryFormatter();
			try
			{
				var bytes = Convert.FromBase64String(stringData);
				using (var stream = new MemoryStream(bytes))
				{
					return (SessionSpecificData)formatter.Deserialize(stream);
				}
			}
			catch (SerializationException)
			{
				return null;
			}
			catch (FormatException)
			{
				return null;
			}
		}
	}
}