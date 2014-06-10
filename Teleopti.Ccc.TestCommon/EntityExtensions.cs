using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public static class EntityExtensions
	{
		public static T WithId<T>(this T entity) where T : IAggregateRoot
		{
			entity.SetId(Guid.NewGuid());
			return entity;
		}
	}
}