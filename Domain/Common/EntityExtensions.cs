using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public static class EntityExtensions
	{
		public static bool OptimizedEquals(this IEntity entity, IEntity entityToCompare)
		{
			if (entity == entityToCompare)
				return true;
			var entityId = entity.Id;
			return entityId.HasValue && entityId.Equals(entityToCompare.Id);
		}
	}
}