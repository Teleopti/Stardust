using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Bindings;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public class DataContext
	{
		private static readonly DataFactory _dataFactory = new DataFactory();

		public static DataFactory Data()
		{
			//if (ExperimentalDataMode.ForEachScenario)
			//{
			//    if (ScenarioContext.Current.Value<DataFactory>("data") == null)
			//        ScenarioContext.Current.Value("data", new DataFactory());
			//    return ScenarioContext.Current.Value<DataFactory>("data");
			//}
			return _dataFactory;
		}
	}
}