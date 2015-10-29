using Newtonsoft.Json;

namespace Teleopti.Ccc.TestCommon
{
	public static class ObjectCopyExtension
	{
		public static T CopyBySerialization<T>(this T instance) 
		{
			if (instance == null)
				return default(T);
			return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(instance));
		}
	}
}