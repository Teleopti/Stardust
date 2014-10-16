using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.HistoricalData;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.HistoricalData
{
	public class HistoricalDataTest
	{
		[Test]
		public void ConvertOneWorkloadDay()
		{
			var wl = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("asdf"));
			var period = new DateOnlyPeriod();
			var workloadDay = new WorkloadDay();
			workloadDay.Create(new DateOnly(2000,1,1), wl, new List<TimePeriod>());
			var orgData = new List<IWorkloadDayBase> {workloadDay};
			var loadStatistics = MockRepository.GenerateStub<ILoadStatistics>();
			loadStatistics.Stub(x => x.LoadWorkloadDay(wl, period)).Return(orgData);
			IHistoricalDataProvider target = new HistoricalDataProvider(loadStatistics);

			var res = target.Calculate(wl, period).Single();

			res.Workload.Should().Be.SameInstanceAs(wl);
			res.TaskOwner.Should().Be.SameInstanceAs(workloadDay);
		}
	}
}