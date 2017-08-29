using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public static class EntityExtensions
	{
		public static T WithId<T>(this T entity) where T : IEntity
		{
            entity.WithId(Guid.NewGuid());
			return entity;
		}

	    public static T WithId<T>(this T entity, Guid id) where T : IEntity
	    {
	        entity.SetId(id);
	        return entity;
	    }
	}
}