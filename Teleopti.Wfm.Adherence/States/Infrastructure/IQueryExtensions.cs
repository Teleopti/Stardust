using System.Collections;
using NHibernate;

namespace Teleopti.Wfm.Adherence.States.Infrastructure
{
	public static class IQueryExtensions
	{
		public static IQuery SetParameterListIf(this IQuery query, bool addParameter, string name, IEnumerable list)
		{
			return addParameter ? query.SetParameterList(name, list) : query;
		}
	}
}