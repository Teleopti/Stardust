using System;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Support.Library.Config;

namespace Teleopti.Ccc.TestCommon.TestData
{
	public static class Extensions
	{
		public static void SetBusinessUnit(this IFilterOnBusinessUnit aggregateRootWithBusinessUnit, IBusinessUnit businessUnit)
		{
			setBusinessUnit(aggregateRootWithBusinessUnit, businessUnit);
		}
		
		public static void SetBusinessUnit(this IFilterOnBusinessUnitId aggregateRootWithBusinessUnit, IBusinessUnit businessUnit)
		{
			setBusinessUnit(aggregateRootWithBusinessUnit, businessUnit.Id.Value);
		}

		private static void setBusinessUnit(object entity, object value)
		{
			var type = entity.GetType();
			var types = new[] {type, type.BaseType, type.BaseType?.BaseType}
				.Where(x => x != null);
			var fields = types.Select(x => x.GetField("_businessUnit", BindingFlags.NonPublic | BindingFlags.Instance))
				.Where(x => x != null)
				.ToArray();
			if (!fields.Any())
				throw new ArgumentException("Field _businessUnit not found");
			fields.ForEach(x =>
			{
				x.SetValue(entity, value);
			});
		}
	}
}