using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public static class EntityExtensions
	{
		 public static bool OptimizedEquals(this IEntity entity, IEntity entityToCompare)
		 {
			 return entity == entityToCompare || (entity.Id.HasValue && entity.Id.Equals(entityToCompare.Id));
		 }
	}
}