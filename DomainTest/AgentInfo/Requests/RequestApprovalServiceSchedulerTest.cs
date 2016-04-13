using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
    [TestFixture]
    public class RequestApprovalServiceSchedulerTest
    {
        private MockRepository mocks;
        private IRequestApprovalService target;
        private IScheduleDictionary scheduleDictionary;
        private IScenario scenario;
        private ISwapAndModifyService swapAndModifyService;
        private INewBusinessRuleCollection businessRules;
        private IScheduleDayChangeCallback scheduleDayChangeCallback;
        private IGlobalSettingDataRepository globalSettingDataRepository;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            scheduleDictionary = mocks.StrictMock<IScheduleDictionary>();
            scenario = mocks.DynamicMock<IScenario>();
            swapAndModifyService = mocks.StrictMock<ISwapAndModifyService>();
            businessRules = mocks.DynamicMock<INewBusinessRuleCollection>();
            scheduleDayChangeCallback = mocks.DynamicMock<IScheduleDayChangeCallback>();
            globalSettingDataRepository = mocks.StrictMock<IGlobalSettingDataRepository>();

            target = new RequestApprovalServiceScheduler(scheduleDictionary, scenario, swapAndModifyService, businessRules, scheduleDayChangeCallback, globalSettingDataRepository);
        }

        [Test]
        public void ShouldPerformShiftTrade()
        {
            var shiftTradeRequest = mocks.DynamicMock<IShiftTradeRequest>();
            var swapDetail = mocks.DynamicMock<IShiftTradeSwapDetail>();
            var swapDetails = new ReadOnlyCollection<IShiftTradeSwapDetail>(new[] {swapDetail});
            using (mocks.Record())
            {
                Expect.Call(swapAndModifyService.SwapShiftTradeSwapDetails(swapDetails, scheduleDictionary, businessRules, new ScheduleTagSetter(NullScheduleTag.Instance))).IgnoreArguments().Return(
                    new List<IBusinessRuleResponse>());
                Expect.Call(shiftTradeRequest.ShiftTradeSwapDetails).Return(swapDetails);
            }
            using (mocks.Playback())
            {
                var result = target.ApproveShiftTrade(shiftTradeRequest);
                Assert.IsTrue(result.IsEmpty());
            }
        }

        [Test]
        public void ShouldApproveBasicAbsence()
        {
            var person = PersonFactory.CreatePerson();
            person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.Utc));
            var absence = AbsenceFactory.CreateAbsence("Holiday");
            var period = new DateTimePeriod(2011, 3, 4, 2011, 3, 4);
            var range = mocks.DynamicMock<IScheduleRange>();
            var day = mocks.DynamicMock<IScheduleDay>();
            var ruleResponse = mocks.DynamicMock<IBusinessRuleResponse>();
            var emptyRuleResponse = new List<IBusinessRuleResponse>();
            var ruleResponses = new List<IBusinessRuleResponse> {ruleResponse};
            using (mocks.Record())
            {
                Expect.Call(day.FullAccess).Return(true);
                Expect.Call(scheduleDictionary[person]).Return(range);
                Expect.Call(range.ScheduledDay(new DateOnly(2011, 3, 4))).Return(day);
                Expect.Call(() => day.Add(null)).Constraints(
                    Rhino.Mocks.Constraints.Is.Matching<IScheduleData>(p => p.Scenario == scenario &&
                                                                            p.Person == person &&
                                                                            p.Period == period));
                Expect.Call(scheduleDictionary.Modify(ScheduleModifier.Request, day, businessRules, scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).IgnoreArguments().Return(
                    ruleResponses);
                Expect.Call(scheduleDictionary.Modify(ScheduleModifier.Request, day, businessRules, scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).IgnoreArguments().Return(
                    emptyRuleResponse);
            }
            using (mocks.Playback())
            {
                var result = target.ApproveAbsence(absence,period,person);
                Assert.IsTrue(result.IsEmpty());
            }
        }

	}
}
