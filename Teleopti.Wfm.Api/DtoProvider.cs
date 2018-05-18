using System;
using System.Linq;

namespace Teleopti.Wfm.Api
{
	public class DtoProvider
	{
		private Type[] types = typeof(QueryDtoProvider).Assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Dto", StringComparison.InvariantCultureIgnoreCase)).ToArray();

		public bool TryFindType(string typeName, out Type type)
		{
			var found = types.FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.InvariantCultureIgnoreCase));
			if (found != null)
			{
				type = found;
				return true;
			}
			type = null;
			return false;
		}

		public Type[] AllowedTypes()
		{
			return types;
		}
	}
}