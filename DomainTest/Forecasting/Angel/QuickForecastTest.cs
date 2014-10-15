/*
 * Namespaces...


LegacyWrappers -> just old things

"Flow" -> a couple of lines in some "top service" (called from web client/Bus or whatever)
1. HistoricalDataProvider -> "joins" statistics and validated data
2. Delete old skilldays (?) -> 
3. Apply new things on new skilldays


The idea is that all tests calls "top service" in each namespace. No need for "testing methods".
 * 
 * */

using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.HistoricalData;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class QuickForecastTest
	{
		[Test]
		public void CalllForecastStepsInOrder() //to be continued - very temporary test....
		{
			var period = new DateOnlyPeriod();
			var workload = new Workload(SkillFactory.CreateSkill("d")); // or should the api use skill?
			
			var historicalDataSvc = MockRepository.GenerateStub<IHistoricalDataProvider>();
			

			var target = new QuickForecaster(historicalDataSvc);
			target.Execute(workload, period);
			historicalDataSvc.AssertWasCalled(x => x.Calculate(workload, period));
		}
	}
}