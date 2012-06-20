﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.8.1.0
//      SpecFlow Generator Version:1.8.0.0
//      Runtime Version:4.0.30319.269
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
    [NUnit.Framework.DescriptionAttribute("Absence request from requests")]
    public partial class AbsenceRequestFromRequestsFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "AbsenceRequestFromRequests.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Absence request from requests", "In order to make requests to my superior\r\nAs an agent\r\nI want to be able to submi" +
                    "t requests as absence", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        [NUnit.Framework.DescriptionAttribute("Add absence request")]
        public virtual void AddAbsenceRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Add absence request", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 7
 testRunner.Given("I am an agent");
#line 8
 testRunner.And("I have a requestable absence called Vacation");
#line 9
 testRunner.And("I am viewing requests");
#line 10
 testRunner.When("I click add request button in the toolbar");
#line 11
 testRunner.And("I click absence request tab");
#line 12
 testRunner.And("I input absence request values with Vacation");
#line 13
 testRunner.And("I click the OK button");
#line 14
 testRunner.Then("I should see the absence request in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can not add absence request from request view if no permission")]
        public virtual void CanNotAddAbsenceRequestFromRequestViewIfNoPermission()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can not add absence request from request view if no permission", ((string[])(null)));
#line 16
this.ScenarioSetup(scenarioInfo);
#line 17
 testRunner.Given("I am an agent without access to absence requests");
#line 18
 testRunner.And("I am viewing requests");
#line 19
 testRunner.When("I click add request button in the toolbar");
#line 20
 testRunner.Then("I should not see the absence request tab");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Default absence request values from request view")]
        public virtual void DefaultAbsenceRequestValuesFromRequestView()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Default absence request values from request view", ((string[])(null)));
#line 22
this.ScenarioSetup(scenarioInfo);
#line 23
 testRunner.Given("I am an agent");
#line 24
 testRunner.And("I have a requestable absence called Vacation");
#line 25
 testRunner.And("I am viewing requests");
#line 26
 testRunner.When("I click add request button in the toolbar");
#line 27
 testRunner.And("I click absence request tab");
#line 28
 testRunner.Then("I should see the absence request form with today\'s date as default");
#line 29
 testRunner.And("I should see 8:00 - 17:00 as the default times");
#line 30
 testRunner.And("I should see an absence type called Vacation in droplist");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Default absence request values from request view When checked Fullday")]
        public virtual void DefaultAbsenceRequestValuesFromRequestViewWhenCheckedFullday()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Default absence request values from request view When checked Fullday", ((string[])(null)));
#line 32
this.ScenarioSetup(scenarioInfo);
#line 33
 testRunner.Given("I am an agent");
#line 34
 testRunner.And("I am viewing requests");
#line 35
 testRunner.When("I click add request button in the toolbar");
#line 36
 testRunner.And("I click absence request tab");
#line 37
 testRunner.And("I click full day checkbox");
#line 38
 testRunner.Then("I should see the request form with tomorrow as default date");
#line 39
 testRunner.And("I should see 00:00 - 23:59 as the default times");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Cancel adding absence request from request view")]
        public virtual void CancelAddingAbsenceRequestFromRequestView()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Cancel adding absence request from request view", ((string[])(null)));
#line 41
this.ScenarioSetup(scenarioInfo);
#line 42
 testRunner.Given("I am an agent");
#line 43
 testRunner.And("I have a requestable absence called Vacation");
#line 44
 testRunner.And("I am viewing requests");
#line 45
 testRunner.When("I click add request button in the toolbar");
#line 46
 testRunner.And("I click absence request tab");
#line 47
 testRunner.And("I input absence request values with Vacation");
#line 48
 testRunner.And("I click the Cancel button");
#line 49
 testRunner.Then("I should not see the absence request in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Adding invalid absence request values")]
        public virtual void AddingInvalidAbsenceRequestValues()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Adding invalid absence request values", ((string[])(null)));
#line 51
this.ScenarioSetup(scenarioInfo);
#line 52
 testRunner.Given("I am an agent");
#line 53
 testRunner.And("I am viewing requests");
#line 54
 testRunner.When("I click add request button in the toolbar");
#line 55
 testRunner.And("I click absence request tab");
#line 56
 testRunner.And("I input empty subject");
#line 57
 testRunner.And("I input later start time than end time");
#line 58
 testRunner.And("I click the OK button");
#line 59
 testRunner.Then("I should see texts describing my errors");
#line 60
 testRunner.And("I should not see the absence request in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Adding too long message on absence request")]
        public virtual void AddingTooLongMessageOnAbsenceRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Adding too long message on absence request", ((string[])(null)));
#line 62
this.ScenarioSetup(scenarioInfo);
#line 63
 testRunner.Given("I am an agent");
#line 64
 testRunner.And("I am viewing requests");
#line 65
 testRunner.When("I click add request button in the toolbar");
#line 66
 testRunner.And("I click absence request tab");
#line 67
 testRunner.And("I input too long message request values");
#line 68
 testRunner.And("I click the OK button");
#line 69
 testRunner.Then("I should see texts describing too long text error");
#line 70
 testRunner.And("I should not see the absence request in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Adding too long subject on absence request")]
        public virtual void AddingTooLongSubjectOnAbsenceRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Adding too long subject on absence request", ((string[])(null)));
#line 72
this.ScenarioSetup(scenarioInfo);
#line 73
 testRunner.Given("I am an agent");
#line 74
 testRunner.And("I am viewing requests");
#line 75
 testRunner.When("I click add request button in the toolbar");
#line 76
 testRunner.And("I click absence request tab");
#line 77
 testRunner.And("I input too long subject request values");
#line 78
 testRunner.And("I click the OK button");
#line 79
 testRunner.Then("I should see texts describing too long subject error");
#line 80
 testRunner.And("I should not see the absence request in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View absence types")]
        public virtual void ViewAbsenceTypes()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View absence types", ((string[])(null)));
#line 82
this.ScenarioSetup(scenarioInfo);
#line 83
 testRunner.Given("I am an agent");
#line 84
 testRunner.And("I have a requestable absence called Vacation");
#line 85
 testRunner.And("I am viewing requests");
#line 86
 testRunner.When("I click add request button in the toolbar");
#line 87
 testRunner.And("I click absence request tab");
#line 88
 testRunner.Then("I should see an absence type called Vacation in droplist");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
