using NHibernate.Envers.Query;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories.Audit
{
	public static class QueryExtensions
	{
		public static IEntityAuditQuery<IRevisionEntityInfo<T, Revision>> AddModifiedByIfNotNull<T>(this IEntityAuditQuery<IRevisionEntityInfo<T, Revision>> query, IPerson modifiedBy)
		{
			return modifiedBy == null ? 
						query : 
						query.Add(AuditEntity.RevisionProperty("ModifiedBy").Eq(modifiedBy));
		}
	}
}