using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
    public static class Data
    {
		public static PersonDataFactory Person(string name)
		{
			return DataFactoryState.TestDataFactory.Person(name);
		}

		public static void Apply(IDataSetup setup)
        {
            DataFactoryState.TestDataFactory.Apply(setup);
        }
    }
}