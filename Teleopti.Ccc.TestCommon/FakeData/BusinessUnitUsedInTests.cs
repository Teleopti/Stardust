using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public static class BusinessUnitUsedInTests
	{
		private const string Name = "Business unit used in test";
		
		private static Lazy<IBusinessUnit> _businessUnit = new Lazy<IBusinessUnit>(() => BusinessUnitFactory.CreateSimpleBusinessUnit(Name).WithId());

		public static void Reset()
		{
			_businessUnit = new Lazy<IBusinessUnit>(() => BusinessUnitFactory.CreateSimpleBusinessUnit(Name).WithId());
		}

		public static IBusinessUnit BusinessUnit => _businessUnit.Value;
	}
}