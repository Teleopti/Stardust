using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	public class DeleteAndResourceCalculateServiceSkipResCalcTest
	{
		[Test]
		public void ShouldResourceCalculate()
		{
			var resourceOptHelper = MockRepository.GenerateMock<IResourceOptimizationHelper>();
			var skillGroupInfo = MockRepository.GenerateStub<IResourceCalculateAfterDeleteDecider>();
			var agent = new Person();
			var date = new DateOnly(2000,1,1);
			var schedDic = new ScheduleDictionaryForTest(new Scenario("_"), new DateTimePeriod(1900, 1, 1, 2100, 1, 1));
			var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedDic, agent, date);
			skillGroupInfo.Expect(x => x.DoCalculation(agent, date)).Return(true);

			var target = new DeleteAndResourceCalculateService(MockRepository.GenerateStub<IDeleteSchedulePartService>(), resourceOptHelper, MockRepository.GenerateStub<IResourceCalculateDaysDecider>(), skillGroupInfo);
			target.DeleteWithResourceCalculation(scheduleDay, null, false, false);

			resourceOptHelper.AssertWasCalled(x => x.ResourceCalculateDate(date, false, false));
		}

		[Test]
		public void ShouldNotResourceCalculate()
		{
			var resourceOptHelper = MockRepository.GenerateMock<IResourceOptimizationHelper>();
			var skillGroupInfo = MockRepository.GenerateStub<IResourceCalculateAfterDeleteDecider>();
			var agent = new Person();
			var date = new DateOnly(2000, 1, 1);
			var schedDic = new ScheduleDictionaryForTest(new Scenario("_"), new DateTimePeriod(1900, 1, 1, 2100, 1, 1));
			var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedDic, agent, date);
			skillGroupInfo.Expect(x => x.DoCalculation(agent, date)).Return(false);

			var target = new DeleteAndResourceCalculateService(MockRepository.GenerateStub<IDeleteSchedulePartService>(), resourceOptHelper, MockRepository.GenerateStub<IResourceCalculateDaysDecider>(), skillGroupInfo);
			target.DeleteWithResourceCalculation(scheduleDay, null, false, false);

			resourceOptHelper.AssertWasNotCalled(x => x.ResourceCalculateDate(date, false, false));
		}
	}
}