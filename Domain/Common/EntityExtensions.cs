﻿using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

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
	}
}