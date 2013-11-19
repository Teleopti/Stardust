using System.Reflection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.TestData
{
	public static class Extensions
	{

		public static void SetBusinessUnit(this IBelongsToBusinessUnit aggregateRootWithBusinessUnit, IBusinessUnit businessUnit)
		{
			var type = typeof(VersionedAggregateRootWithBusinessUnit);
			var privateField = type.GetField("_businessUnit", BindingFlags.NonPublic | BindingFlags.Instance);
			privateField.SetValue(aggregateRootWithBusinessUnit, businessUnit);
		}

	}
}