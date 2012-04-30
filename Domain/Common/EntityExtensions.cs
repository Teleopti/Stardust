using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public static class EntityExtensions
	{
		public static bool OptimizedEquals(this IEntity entity, IEntity entityToCompare)
		{
			if (entity == null || entityToCompare == null)
				return false;
			if (entity == entityToCompare)
				return true;
			var entityId = entity.Id;
			var entityToCompareId = entityToCompare.Id;
			return entityId.HasValue && entityToCompareId.HasValue && entityId.Equals(entityToCompareId);
		}
	}
}