using System;
using System.Linq;

namespace Teleopti.Wfm.Api
{
	public class QueryDtoProvider
	{
		private Type[] types = typeof(QueryDtoProvider).Assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && typeof(IQueryDto).IsAssignableFrom(t)).ToArray();

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
	}
}