using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.TestCommon
{
	public static class EntityExtensions
	{
		public static T WithId<T>(this T entity) where T : IEntity
		{
            entity.WithId(Guid.NewGuid());
			return entity;
		}
		
		public static T WithIdIfNotPresent<T>(this T entity) where T : IEntity
		{
			if(!entity.Id.HasValue)
				entity.WithId(Guid.NewGuid());
			return entity;
		}

	    public static T WithId<T>(this T entity, Guid id) where T : IEntity
	    {
	        entity.SetId(id);
	        return entity;
	    }

		public static T WithBusinessUnit<T>(this T entity, IBusinessUnit businessUnit) where T : IFilterOnBusinessUnit
		{
			entity.SetBusinessUnit(businessUnit);
			return entity;
		}
	}
}