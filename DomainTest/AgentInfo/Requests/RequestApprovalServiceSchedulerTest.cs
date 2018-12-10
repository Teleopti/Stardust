using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;


namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
    [TestFixture]
    public class RequestApprovalServiceSchedulerTest
    {
        [Test]
        public void ShouldPerformShiftTrade()
        {
			var scenario = ScenarioFactory.CreateScenario("Default", true, false);
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, new DateTimePeriod(2010, 1, 1, 2010, 1, 2));
			var businessRules = new FakeNewBusinessRuleCollection();
			var scheduleDayChangeCallback = new DoNothingScheduleDayChangeCallBack();
			var swapAndModifyService = new SwapAndModifyService(new SwapService(), scheduleDayChangeCallback);

			var target = new ShiftTradeRequestApprovalService(scheduleDictionary, swapAndModifyService, businessRules, new PersonRequestAuthorizationCheckerForTest(), new FakePersonRequestRepository());
	
			var shiftTradeRequest =
		        new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
		        {
			        new ShiftTradeSwapDetail(PersonFactory.CreatePerson(), PersonFactory.CreatePerson(), new DateOnly(2010, 1, 1),
				        new DateOnly(2010, 1, 2))
		        });

			var result = target.Approve(shiftTradeRequest);
	        Assert.IsTrue(result.IsEmpty());
        }

	    [Test]
	    public void ShouldApproveBasicAbsence()
	    {
			var scenario = ScenarioFactory.CreateScenario("Default", true, false);
		    var dateTimePeriod = new DateTimePeriod(2010, 1, 1, 2010, 1, 2);
		    var scheduleDictionary = new ScheduleDictionaryForTest(scenario, dateTimePeriod);
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var period = new DateTimePeriod(2011, 3, 4, 2011, 3, 4);
			var businessRules = new FakeNewBusinessRuleCollection();
			businessRules.SetRuleResponse(new List<IBusinessRuleResponse> {new BusinessRuleResponse(typeof(BusinessRuleResponse),"warning",true,false,dateTimePeriod,person,new DateOnlyPeriod(2010,1,1,2010,1,2), "test warning")});
			var scheduleDayChangeCallback = new DoNothingScheduleDayChangeCallBack();
			var globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			var personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();

			var target = new AbsenceRequestApprovalService(scenario, scheduleDictionary, businessRules, scheduleDayChangeCallback, globalSettingDataRepository, new CheckingPersonalAccountDaysProvider(personAbsenceAccountRepository));
		    var result = target.Approve(absence, period, person);

		    Assert.IsTrue(result.IsEmpty());
	    }
    }
}
