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
    [NUnit.Framework.DescriptionAttribute("Text request from requests")]
    public partial class TextRequestFromRequestsFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "TextRequestFromRequests.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Text request from requests", "In order to make requests to my superior\r\nAs an agent\r\nI want to be able to submi" +
                    "t requests as text", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        [NUnit.Framework.DescriptionAttribute("Add text request")]
        public virtual void AddTextRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Add text request", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 7
 testRunner.Given("I am an agent");
#line 8
 testRunner.And("I am viewing requests");
#line 9
 testRunner.When("I click add text request button in the toolbar");
#line 10
 testRunner.And("I input text request values");
#line 11
 testRunner.And("I click the OK button");
#line 12
 testRunner.Then("I should see the text request in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Default text-request values from request view")]
        public virtual void DefaultText_RequestValuesFromRequestView()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Default text-request values from request view", ((string[])(null)));
#line 14
this.ScenarioSetup(scenarioInfo);
#line 15
 testRunner.Given("I am an agent");
#line 16
 testRunner.And("I am viewing requests");
#line 17
 testRunner.When("I click add text request button in the toolbar");
#line 18
 testRunner.Then("I should see the text request form with today\'s date as default");
#line 19
 testRunner.And("I should see 8:00 - 17:00 as the default times");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Adding invalid text request values")]
        public virtual void AddingInvalidTextRequestValues()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Adding invalid text request values", ((string[])(null)));
#line 21
this.ScenarioSetup(scenarioInfo);
#line 22
 testRunner.Given("I am an agent");
#line 23
 testRunner.And("I am viewing requests");
#line 24
 testRunner.When("I click add text request button in the toolbar");
#line 25
 testRunner.And("I input empty subject");
#line 26
 testRunner.And("I input later start time than end time");
#line 27
 testRunner.And("I click the OK button");
#line 28
 testRunner.Then("I should see texts describing my errors");
#line 29
 testRunner.And("I should not see the text request in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Adding too long text request")]
        public virtual void AddingTooLongTextRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Adding too long text request", ((string[])(null)));
#line 31
this.ScenarioSetup(scenarioInfo);
#line 32
 testRunner.Given("I am an agent");
#line 33
 testRunner.And("I am viewing requests");
#line 34
 testRunner.When("I click add text request button in the toolbar");
#line 35
 testRunner.And("I input too long text request values");
#line 36
 testRunner.And("I click the OK button");
#line 37
 testRunner.Then("I should see texts describing too long text error");
#line 38
 testRunner.And("I should not see the text request in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Adding too long subject request")]
        public virtual void AddingTooLongSubjectRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Adding too long subject request", ((string[])(null)));
#line 40
this.ScenarioSetup(scenarioInfo);
#line 41
 testRunner.Given("I am an agent");
#line 42
 testRunner.And("I am viewing requests");
#line 43
 testRunner.When("I click add text request button in the toolbar");
#line 44
 testRunner.And("I input too long subject request values");
#line 45
 testRunner.And("I click the OK button");
#line 46
 testRunner.Then("I should see texts describing too long subject error");
#line 47
 testRunner.And("I should not see the text request in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View text request details")]
        public virtual void ViewTextRequestDetails()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View text request details", ((string[])(null)));
#line 49
this.ScenarioSetup(scenarioInfo);
#line 50
 testRunner.Given("I am an agent");
#line 51
 testRunner.And("I have an existing text request");
#line 52
 testRunner.And("I am viewing requests");
#line 53
 testRunner.When("I click on the request");
#line 54
 testRunner.Then("I should see the text request\'s details form");
#line 55
 testRunner.And("I should see the request\'s values");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Edit text request")]
        public virtual void EditTextRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Edit text request", ((string[])(null)));
#line 57
this.ScenarioSetup(scenarioInfo);
#line 58
 testRunner.Given("I am an agent");
#line 59
 testRunner.And("I have an existing text request");
#line 60
 testRunner.And("I am viewing requests");
#line 61
 testRunner.When("I click on the request");
#line 62
 testRunner.And("I input new text request values");
#line 63
 testRunner.And("I click the OK button");
#line 64
 testRunner.Then("I should see the new text request values in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Delete text request")]
        public virtual void DeleteTextRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Delete text request", ((string[])(null)));
#line 66
this.ScenarioSetup(scenarioInfo);
#line 67
 testRunner.Given("I am an agent");
#line 68
 testRunner.And("I have an existing text request");
#line 69
 testRunner.And("I am viewing requests");
#line 70
 testRunner.When("I click the request\'s delete button");
#line 71
 testRunner.Then("I should not see the text request in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can not edit approved text requests")]
        public virtual void CanNotEditApprovedTextRequests()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can not edit approved text requests", ((string[])(null)));
#line 73
this.ScenarioSetup(scenarioInfo);
#line 74
 testRunner.Given("I am an agent");
#line 75
 testRunner.And("I have an approved text request");
#line 76
 testRunner.And("I am viewing requests");
#line 77
 testRunner.When("I click on the request");
#line 78
 testRunner.Then("I should see the text request\'s details form");
#line 79
 testRunner.And("I should not be able to input values");
#line 80
 testRunner.And("I should not see a save button");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can not edit denied text requests")]
        public virtual void CanNotEditDeniedTextRequests()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can not edit denied text requests", ((string[])(null)));
#line 82
this.ScenarioSetup(scenarioInfo);
#line 83
 testRunner.Given("I am an agent");
#line 84
 testRunner.And("I have a denied text request");
#line 85
 testRunner.And("I am viewing requests");
#line 86
 testRunner.When("I click on the request");
#line 87
 testRunner.Then("I should see the text request\'s details form");
#line 88
 testRunner.And("I should not be able to input values");
#line 89
 testRunner.And("I should not see a save button");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can not delete approved text request")]
        public virtual void CanNotDeleteApprovedTextRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can not delete approved text request", ((string[])(null)));
#line 91
this.ScenarioSetup(scenarioInfo);
#line 92
 testRunner.Given("I am an agent");
#line 93
 testRunner.And("I have an approved text request");
#line 94
 testRunner.When("I am viewing requests");
#line 95
 testRunner.Then("I should not see a delete button");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can not delete denied text request")]
        public virtual void CanNotDeleteDeniedTextRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can not delete denied text request", ((string[])(null)));
#line 97
this.ScenarioSetup(scenarioInfo);
#line 98
 testRunner.Given("I am an agent");
#line 99
 testRunner.And("I have a denied text request");
#line 100
 testRunner.When("I am viewing requests");
#line 101
 testRunner.Then("I should not see a delete button");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
