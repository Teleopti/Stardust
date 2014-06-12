using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public static class EntityExtensions
	{
		public static T WithId<T>(this T entity) where T : IEntity
		{
			entity.SetId(Guid.NewGuid());
			return entity;
		}
	}
}