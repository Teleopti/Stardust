using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel.HistoricalData;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.HistoricalData
{
	public class HistoricalDataTest
	{
		[Test]
		public void OnlyStatistics()
		{
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("asdf"));
			var period = new DateOnlyPeriod();
			var workloadDay = new WorkloadDay();
			workloadDay.Create(new DateOnly(2000,1,1), workload, new List<TimePeriod>());
			var orgData = new List<IWorkloadDayBase> {workloadDay};
			var loadStatistics = MockRepository.GenerateStub<ILoadStatistics>();
			loadStatistics.Stub(x => x.LoadWorkloadDay(workload, period)).Return(orgData);
			IHistoricalDataProvider target = new HistoricalDataProvider(loadStatistics, null);

			var res = target.Calculate(workload, period).Single();

			res.Workload.Should().Be.SameInstanceAs(workload);
			res.TaskOwner.Should().Be.SameInstanceAs(workloadDay);
		}

		[Test]
		public void StatisticsAndValidatedData()
		{
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("asdf"));
			var period = new DateOnlyPeriod(2001, 1, 1, 2001, 1, 2);
			var workloadDay = new WorkloadDay();
			workloadDay.Create(period.StartDate, workload, new List<TimePeriod>());
			var loadStatistics = MockRepository.GenerateStub<ILoadStatistics>();
			loadStatistics.Stub(x => x.LoadWorkloadDay(workload, period)).Return(new List<IWorkloadDayBase> {workloadDay});
			var validatedVolumeDayRepository = MockRepository.GenerateStub<IValidatedVolumeDayRepository>();
			validatedVolumeDayRepository.Stub(x => x.FindRange(period, workload))
				.Return(new[] {new ValidatedVolumeDay(workload, period.StartDate)});
			IHistoricalDataProvider target = new HistoricalDataProvider(loadStatistics, validatedVolumeDayRepository);

			var res = target.Calculate(workload, period).Single();

			res.Workload.Should().Be.SameInstanceAs(workload);
			res.TaskOwner.Should().Be.SameInstanceAs(workloadDay);
		}

		[Test]
		public void ShouldReturnEmptyIfNoStatisticsEvenIfValidatedExist()
		{
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("asdf"));
			var period = new DateOnlyPeriod(2001, 1, 1, 2001, 1, 2);
			var workloadDay = new WorkloadDay();
			workloadDay.Create(period.StartDate, workload, new List<TimePeriod>());
			var loadStatistics = MockRepository.GenerateStub<ILoadStatistics>();
			loadStatistics.Stub(x => x.LoadWorkloadDay(workload, period)).Return(Enumerable.Empty<IWorkloadDayBase>());
			var validatedVolumeDayRepository = MockRepository.GenerateStub<IValidatedVolumeDayRepository>();
			validatedVolumeDayRepository.Stub(x => x.FindRange(period, workload))
				.Return(new[] { new ValidatedVolumeDay(workload, period.StartDate) });

			IHistoricalDataProvider target = new HistoricalDataProvider(loadStatistics, validatedVolumeDayRepository);
			target.Calculate(workload, period).Should().Be.Empty();
		}
	}
}