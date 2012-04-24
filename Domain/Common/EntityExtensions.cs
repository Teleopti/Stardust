using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public static class EntityExtensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static bool OptimizedEquals(this IEntity entity, IEntity entityToCompare)
		{
			if (entity == entityToCompare)
				return true;
			var entityId = entity.Id;
			return entityId.HasValue && entityId.Equals(entityToCompare.Id);
		}
	}
}