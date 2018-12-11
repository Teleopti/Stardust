using System.Reflection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.TestData
{
	public static class Extensions
	{
		public static void SetBusinessUnit(this IBelongsToBusinessUnit aggregateRootWithBusinessUnit, IBusinessUnit businessUnit)
		{
			var type = typeof (VersionedAggregateRootWithBusinessUnit);
			if (aggregateRootWithBusinessUnit is VersionedAggregateRootWithBusinessUnitWithoutChangeInfo)
				type = typeof(VersionedAggregateRootWithBusinessUnitWithoutChangeInfo);
			var privateField = type.GetField("_businessUnit", BindingFlags.NonPublic | BindingFlags.Instance);
			privateField.SetValue(aggregateRootWithBusinessUnit, businessUnit);
		}
		
		public static void SetBusinessUnit(this IBelongsToBusinessUnitId aggregateRootWithBusinessUnit, IBusinessUnit businessUnit)
		{
			var type = typeof (VersionedAggregateRootWithBusinessUnit);
			if (aggregateRootWithBusinessUnit is VersionedAggregateRootWithBusinessUnitIdWithoutChangeInfo)
				type = typeof(VersionedAggregateRootWithBusinessUnitWithoutChangeInfo);
			var privateField = type.GetField("_businessUnit", BindingFlags.NonPublic | BindingFlags.Instance);
			privateField.SetValue(aggregateRootWithBusinessUnit, businessUnit);
		}
	}
}