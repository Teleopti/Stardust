using System;
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

		public static object CopyBySerialization(this object instance, Type type)
		{
			if (instance == null)
				return null;
			return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(instance), type);
		}
		
		public static TTarget CopyBySerialization<TSource, TTarget>(this TSource instance)
		{
			if (instance == null)
				return default(TTarget);
			return JsonConvert.DeserializeObject<TTarget>(JsonConvert.SerializeObject(instance));
		}

	}
}