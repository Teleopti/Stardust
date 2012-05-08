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
    [NUnit.Framework.DescriptionAttribute("Absence request from schedule")]
    public partial class AbsenceRequestFromScheduleFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "AbsenceRequestFromSchedule.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Absence request from schedule", "In order to make requests to my superior\r\nAs an agent\r\nI want to be able to submi" +
                    "t absence requests", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        [NUnit.Framework.DescriptionAttribute("Add absence request from week schedule view")]
        public virtual void AddAbsenceRequestFromWeekScheduleView()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Add absence request from week schedule view", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 7
 testRunner.Given("I am an agent");
#line 8
 testRunner.And("I am viewing week schedule");
#line 9
 testRunner.And("I have a requestable absence called Vacation");
#line 10
 testRunner.When("I click on today\'s summary");
#line 11
 testRunner.And("I click absence request tab");
#line 12
 testRunner.And("I input absence request values with Vacation");
#line 13
 testRunner.And("I click the OK button");
#line 14
 testRunner.Then("I should see a symbol at the top of the schedule");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can not add absence request if no permission")]
        public virtual void CanNotAddAbsenceRequestIfNoPermission()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can not add absence request if no permission", ((string[])(null)));
#line 16
this.ScenarioSetup(scenarioInfo);
#line 17
 testRunner.Given("I am an agent without access to absence requests");
#line 18
 testRunner.And("I am viewing week schedule");
#line 19
 testRunner.When("I click on today\'s summary");
#line 20
 testRunner.Then("I should not see the absence request tab");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Default absence request values from week schedule")]
        public virtual void DefaultAbsenceRequestValuesFromWeekSchedule()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Default absence request values from week schedule", ((string[])(null)));
#line 22
this.ScenarioSetup(scenarioInfo);
#line 23
 testRunner.Given("I am an agent");
#line 24
 testRunner.And("I am viewing week schedule");
#line 25
 testRunner.When("I click on tomorrows summary");
#line 26
 testRunner.And("I click absence request tab");
#line 27
 testRunner.Then("I should see the text request form with tomorrow as default date");
#line 28
 testRunner.And("I should see 00:00 - 23:59 as the default times");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Cancel adding absence request")]
        public virtual void CancelAddingAbsenceRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Cancel adding absence request", ((string[])(null)));
#line 30
this.ScenarioSetup(scenarioInfo);
#line 31
 testRunner.Given("I am an agent");
#line 32
 testRunner.And("I am viewing week schedule");
#line 33
 testRunner.When("I click on today\'s summary");
#line 34
 testRunner.And("I click absence request tab");
#line 35
 testRunner.And("I input absence request values");
#line 36
 testRunner.And("I click the Cancel button");
#line 37
 testRunner.Then("I should not see a symbol at the top of the schedule");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Adding invalid absence request values")]
        public virtual void AddingInvalidAbsenceRequestValues()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Adding invalid absence request values", ((string[])(null)));
#line 39
this.ScenarioSetup(scenarioInfo);
#line 40
 testRunner.Given("I am an agent");
#line 41
 testRunner.And("I am viewing week schedule");
#line 42
 testRunner.When("I click on today\'s summary");
#line 43
 testRunner.And("I click absence request tab");
#line 44
 testRunner.And("I input empty subject");
#line 45
 testRunner.And("I input later start time than end time");
#line 46
 testRunner.And("I click the OK button");
#line 47
 testRunner.Then("I should see texts describing my errors");
#line 48
 testRunner.And("I should not see a symbol at the top of the schedule");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View absence types")]
        public virtual void ViewAbsenceTypes()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View absence types", ((string[])(null)));
#line 50
this.ScenarioSetup(scenarioInfo);
#line 51
 testRunner.Given("I am an agent");
#line 52
 testRunner.And("I have a requestable absence called Vacation");
#line 53
 testRunner.And("I am viewing week schedule");
#line 54
 testRunner.When("I click on today\'s summary");
#line 55
 testRunner.And("I click absence request tab");
#line 56
 testRunner.Then("I should see an absence type called Vacation in droplist");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
