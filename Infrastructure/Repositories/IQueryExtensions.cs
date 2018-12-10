using System.Collections;
using NHibernate;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public static class IQueryExtensions
	{
		public static IQuery SetDateOnly(this IQuery query, string name, DateOnly date)
		{
			return query.SetDateTime(name, date.Date);
		}

		public static IQuery SetParameterListIf(this IQuery query, bool addParameter, string name, IEnumerable list)
		{
			return addParameter ? query.SetParameterList(name, list) : query;
		}
	}
}