using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
	public static class EntityExtensions
	{
		public static bool OptimizedEquals(this IEntity entity, IEntity entityToCompare)
		{
			if (entity == entityToCompare)
				return true;
			if (entity == null || entityToCompare == null)
				return false;
			var entityId = entity.Id;
			var entityToCompareId = entityToCompare.Id;
			return entityId.HasValue && entityToCompareId.HasValue && entityId.Equals(entityToCompareId);
		}

		public static IEnumerable<T> NonDeleted<T>(this IEnumerable<T> aggregates) where T : IAggregateRoot
		{
			return aggregates.Where(s => !((IDeleteTag) s).IsDeleted).ToArray();
		}
	}
}