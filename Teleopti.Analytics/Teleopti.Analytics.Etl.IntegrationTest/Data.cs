using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	public static class Data
	{
		public static PersonDataFactory Person(string name)
		{
			return TestState.TestDataFactory.Person(name);
		}

		public static void Apply<T>(T specOrSetup)
		{
			TestState.TestDataFactory.Apply(specOrSetup);
		}
	}
}