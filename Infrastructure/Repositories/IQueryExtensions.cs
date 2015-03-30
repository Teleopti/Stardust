using NHibernate;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public static class IQueryExtensions
	{
		public static IQuery SetDateOnly(this IQuery query, string name, DateOnly date)
		{
			return query.SetDateTime(name, date.Date);
		}
	}
}