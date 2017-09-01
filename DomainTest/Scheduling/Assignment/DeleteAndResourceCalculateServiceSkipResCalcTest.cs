using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	public class DeleteAndResourceCalculateServiceSkipResCalcTest
	{
		[Test]
		public void ShouldResourceCalculate()
		{
			var resourceOptHelper = MockRepository.GenerateMock<IResourceCalculation>();
			var skillGroupInfo = MockRepository.GenerateStub<IResourceCalculateAfterDeleteDecider>();
			var agent = new Person();
			var date = new DateOnly(2000,1,1);
			var schedDic = new ScheduleDictionaryForTest(new Scenario("_"), new DateTimePeriod(1900, 1, 1, 2100, 1, 1));
			var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedDic, agent, date);
			skillGroupInfo.Expect(x => x.DoCalculation(agent, date)).Return(true);
			var stateHolder = new SchedulingResultStateHolder(new List<IPerson>(), new FakeScheduleDictionary(), new Dictionary<ISkill, IEnumerable<ISkillDay>>());

			var target = new DeleteAndResourceCalculateService(() => stateHolder, MockRepository.GenerateStub<IDeleteSchedulePartService>(), resourceOptHelper, new AffectedDates(new FakeTimeZoneGuard()));
			target.DeleteWithResourceCalculation(scheduleDay, null, false, false, skillGroupInfo);

			resourceOptHelper.AssertWasCalled(x => x.ResourceCalculate(date, new ResourceCalculationData(stateHolder, false, false)), options => options.IgnoreArguments());
		}

		[Test]
		public void ShouldNotResourceCalculate()
		{
			var resourceOptHelper = MockRepository.GenerateMock<IResourceCalculation>();
			var skillGroupInfo = MockRepository.GenerateStub<IResourceCalculateAfterDeleteDecider>();
			var agent = new Person();
			var date = new DateOnly(2000, 1, 1);
			var schedDic = new ScheduleDictionaryForTest(new Scenario("_"), new DateTimePeriod(1900, 1, 1, 2100, 1, 1));
			var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedDic, agent, date);
			skillGroupInfo.Expect(x => x.DoCalculation(agent, date)).Return(false);
			var stateHolder = new SchedulingResultStateHolder(new List<IPerson>(), new FakeScheduleDictionary(), new Dictionary<ISkill, IEnumerable<ISkillDay>>());

			var target = new DeleteAndResourceCalculateService(() => stateHolder, MockRepository.GenerateStub<IDeleteSchedulePartService>(), resourceOptHelper, new AffectedDates(new FakeTimeZoneGuard()));
			target.DeleteWithResourceCalculation(scheduleDay, null, false, false, skillGroupInfo);

			resourceOptHelper.AssertWasNotCalled(x => x.ResourceCalculate(date, new ResourceCalculationData(stateHolder, false, false)), options => options.IgnoreArguments());
		}
	}
}