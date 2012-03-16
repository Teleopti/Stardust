﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.8.1.0
//      SpecFlow Generator Version:1.8.0.0
//      Runtime Version:4.0.30319.261
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Teleopti.Ccc.WebBehaviorTest
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.8.1.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("View week schedule")]
    public partial class ViewWeekScheduleFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "WeekSchedule.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "View week schedule", "In order to know how to work this week\r\nAs an agent\r\nI want to see my schedule de" +
                    "tails", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View current week")]
        public virtual void ViewCurrentWeek()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View current week", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 7
 testRunner.Given("I am an agent");
#line 8
 testRunner.And("My schedule is published");
#line 9
 testRunner.When("I view my week schedule");
#line 10
 testRunner.Then("I should see the start and end dates for current week");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View night shift")]
        public virtual void ViewNightShift()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View night shift", ((string[])(null)));
#line 12
this.ScenarioSetup(scenarioInfo);
#line 13
 testRunner.Given("I am an agent");
#line 14
 testRunner.And("I have a night shift starting on monday");
#line 15
 testRunner.And("My schedule is published");
#line 16
 testRunner.When("I view my week schedule");
#line 17
 testRunner.Then("the shift should end on monday");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Do not show unpublished schedule")]
        public virtual void DoNotShowUnpublishedSchedule()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Do not show unpublished schedule", ((string[])(null)));
#line 19
this.ScenarioSetup(scenarioInfo);
#line 20
 testRunner.Given("I am an agent");
#line 21
 testRunner.And("I have shifts scheduled for two weeks");
#line 22
 testRunner.And("My schedule is not published");
#line 23
 testRunner.When("I view my week schedule");
#line 24
 testRunner.Then("I should not see any shifts");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Do not show unpublished schedule for part of week")]
        public virtual void DoNotShowUnpublishedScheduleForPartOfWeek()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Do not show unpublished schedule for part of week", ((string[])(null)));
#line 26
this.ScenarioSetup(scenarioInfo);
#line 27
 testRunner.Given("I am an agent");
#line 28
 testRunner.And("I have shifts scheduled for two weeks");
#line 29
 testRunner.And("My schedule is published until wednesday");
#line 30
 testRunner.When("I view my week schedule");
#line 31
 testRunner.Then("I should not see any shifts after wednesday");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View meeting")]
        public virtual void ViewMeeting()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View meeting", ((string[])(null)));
#line 33
this.ScenarioSetup(scenarioInfo);
#line 34
 testRunner.Given("I am an agent");
#line 36
 testRunner.And("I have a shift on thursday");
#line 37
 testRunner.And("I have a meeting scheduled on thursday");
#line 38
 testRunner.And("My schedule is published");
#line 39
 testRunner.When("I view my week schedule");
#line 40
 testRunner.And("I click on the meeting");
#line 41
 testRunner.Then("I should see the meeting details");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View public note")]
        public virtual void ViewPublicNote()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View public note", ((string[])(null)));
#line 43
this.ScenarioSetup(scenarioInfo);
#line 44
 testRunner.Given("I am an agent");
#line 45
 testRunner.And("I have a public note on tuesday");
#line 46
 testRunner.And("My schedule is published");
#line 47
 testRunner.When("I view my week schedule");
#line 48
 testRunner.Then("I should see the public note on tuesday");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Select week from week-picker")]
        public virtual void SelectWeekFromWeek_Picker()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Select week from week-picker", ((string[])(null)));
#line 50
this.ScenarioSetup(scenarioInfo);
#line 51
 testRunner.Given("I am an agent");
#line 52
 testRunner.And("My schedule is published");
#line 53
 testRunner.And("I view my week schedule");
#line 54
 testRunner.When("I open the week-picker");
#line 55
 testRunner.And("I click on any day of a week");
#line 56
 testRunner.Then("the week-picker should close");
#line 57
 testRunner.And("I should see the selected week");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Week-picker monday first day of week for swedish culture")]
        public virtual void Week_PickerMondayFirstDayOfWeekForSwedishCulture()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Week-picker monday first day of week for swedish culture", ((string[])(null)));
#line 59
this.ScenarioSetup(scenarioInfo);
#line 60
 testRunner.Given("I am an agent");
#line 61
 testRunner.And("I am swedish");
#line 62
 testRunner.And("I view my week schedule");
#line 63
 testRunner.When("I open the week-picker");
#line 64
 testRunner.Then("I should see monday as the first day of week");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Week-picker sunday first day of week for US culture")]
        public virtual void Week_PickerSundayFirstDayOfWeekForUSCulture()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Week-picker sunday first day of week for US culture", ((string[])(null)));
#line 66
this.ScenarioSetup(scenarioInfo);
#line 67
 testRunner.Given("I am an agent");
#line 68
 testRunner.And("I am american");
#line 69
 testRunner.And("I view my week schedule");
#line 70
 testRunner.When("I open the week-picker");
#line 71
 testRunner.Then("I should see sunday as the first day of week");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show text request symbol")]
        public virtual void ShowTextRequestSymbol()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show text request symbol", ((string[])(null)));
#line 77
this.ScenarioSetup(scenarioInfo);
#line 78
 testRunner.Given("I am an agent");
#line 79
 testRunner.And("I have an existing text request");
#line 80
 testRunner.When("I view my week schedule");
#line 81
 testRunner.Then("I should see a symbol at the top of the schedule");
#line 82
 testRunner.And("I should see a number with the text request count");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Multiple day text requests symbol")]
        public virtual void MultipleDayTextRequestsSymbol()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Multiple day text requests symbol", ((string[])(null)));
#line 84
this.ScenarioSetup(scenarioInfo);
#line 85
 testRunner.Given("I am an agent");
#line 86
 testRunner.And("I have an existing text request spanning over 2 days");
#line 87
 testRunner.When("I view my week schedule");
#line 88
 testRunner.Then("I should see a symbol at the top of the schedule for the first day");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
