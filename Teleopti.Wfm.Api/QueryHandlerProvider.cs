using System;
using System.Linq;

namespace Teleopti.Wfm.Api
{
	public class QueryHandlerProvider
	{
		private Type[] types = typeof(QueryDtoProvider).Assembly.GetTypes().Where(t =>
		{
			var i = t.GetInterfaces().FirstOrDefault();
			return i != null &&
				   t.IsClass &&
				   !t.IsAbstract &&
				   i.IsGenericType &&
				   typeof(IQueryHandler<,>) == i.GetGenericTypeDefinition();
		}).ToArray();

		public Tuple<Type, Type, Type>[] AllowedQueryTypes()
		{
			return types.Select(t =>
			{
				var args = t.GetInterfaces()[0].GetGenericArguments();
				return new Tuple<Type, Type, Type>(t, args[0], args[1]);
			}).ToArray();
		}
	}
}