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
 testRunner.And("I should see 00:00 - 23:59 as the default times");
#line 30
 testRunner.And("I should see an absence type called Vacation in droplist");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Default absence request values from request view when checked Fullday")]
        public virtual void DefaultAbsenceRequestValuesFromRequestViewWhenCheckedFullday()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Default absence request values from request view when checked Fullday", ((string[])(null)));
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
 testRunner.And("I checked the full day checkbox");
#line 38
 testRunner.Then("I should see the absence request form with today\'s date as default");
#line 39
 testRunner.And("I should see 00:00 - 23:59 as the default times");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Default absence request values from request view when unchecked Fullday")]
        public virtual void DefaultAbsenceRequestValuesFromRequestViewWhenUncheckedFullday()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Default absence request values from request view when unchecked Fullday", ((string[])(null)));
#line 41
this.ScenarioSetup(scenarioInfo);
#line 42
 testRunner.Given("I am an agent");
#line 43
 testRunner.And("I am viewing requests");
#line 44
 testRunner.When("I click add request button in the toolbar");
#line 45
 testRunner.And("I click absence request tab");
#line 46
 testRunner.And("I unchecked the full day checkbox");
#line 47
 testRunner.Then("I should see the absence request form with today\'s date as default");
#line 48
 testRunner.And("I should see 08:00 - 17:00 as the default times");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Adding invalid absence request values")]
        public virtual void AddingInvalidAbsenceRequestValues()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Adding invalid absence request values", ((string[])(null)));
#line 50
this.ScenarioSetup(scenarioInfo);
#line 51
 testRunner.Given("I am an agent");
#line 52
 testRunner.And("I am viewing requests");
#line 53
 testRunner.When("I click add request button in the toolbar");
#line 54
 testRunner.And("I click absence request tab");
#line 55
 testRunner.And("I input empty subject");
#line 56
 testRunner.And("I input later start time than end time");
#line 57
 testRunner.And("I click the OK button");
#line 58
 testRunner.Then("I should see texts describing my errors");
#line 59
 testRunner.And("I should not see the absence request in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Adding too long message on absence request")]
        public virtual void AddingTooLongMessageOnAbsenceRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Adding too long message on absence request", ((string[])(null)));
#line 61
this.ScenarioSetup(scenarioInfo);
#line 62
 testRunner.Given("I am an agent");
#line 63
 testRunner.And("I am viewing requests");
#line 64
 testRunner.When("I click add request button in the toolbar");
#line 65
 testRunner.And("I click absence request tab");
#line 66
 testRunner.And("I input too long message request values");
#line 67
 testRunner.And("I click the OK button");
#line 68
 testRunner.Then("I should see texts describing too long text error");
#line 69
 testRunner.And("I should not see the absence request in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Adding too long subject on absence request")]
        public virtual void AddingTooLongSubjectOnAbsenceRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Adding too long subject on absence request", ((string[])(null)));
#line 71
this.ScenarioSetup(scenarioInfo);
#line 72
 testRunner.Given("I am an agent");
#line 73
 testRunner.And("I am viewing requests");
#line 74
 testRunner.When("I click add request button in the toolbar");
#line 75
 testRunner.And("I click absence request tab");
#line 76
 testRunner.And("I input too long subject request values");
#line 77
 testRunner.And("I click the OK button");
#line 78
 testRunner.Then("I should see texts describing too long subject error");
#line 79
 testRunner.And("I should not see the absence request in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View absence types")]
        public virtual void ViewAbsenceTypes()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View absence types", ((string[])(null)));
#line 81
this.ScenarioSetup(scenarioInfo);
#line 82
 testRunner.Given("I am an agent");
#line 83
 testRunner.And("I have a requestable absence called Vacation");
#line 84
 testRunner.And("I am viewing requests");
#line 85
 testRunner.When("I click add request button in the toolbar");
#line 86
 testRunner.And("I click absence request tab");
#line 87
 testRunner.Then("I should see an absence type called Vacation in droplist");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Hide text request tab when view an absence request")]
        public virtual void HideTextRequestTabWhenViewAnAbsenceRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Hide text request tab when view an absence request", ((string[])(null)));
#line 89
this.ScenarioSetup(scenarioInfo);
#line 90
 testRunner.Given("I am an agent");
#line 91
 testRunner.And("I have an existing absence request");
#line 92
 testRunner.And("I am viewing requests");
#line 93
 testRunner.When("I click on the request");
#line 94
 testRunner.Then("I should not see the text request tab (invisible)");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View absence request details")]
        public virtual void ViewAbsenceRequestDetails()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View absence request details", ((string[])(null)));
#line 96
this.ScenarioSetup(scenarioInfo);
#line 97
 testRunner.Given("I am an agent");
#line 98
 testRunner.And("I have an existing absence request");
#line 99
 testRunner.And("I am viewing requests");
#line 100
 testRunner.When("I click on the request");
#line 101
 testRunner.Then("I should see the absence request\'s details form");
#line 102
 testRunner.And("I should see the absence request\'s values");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Edit absence request")]
        public virtual void EditAbsenceRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Edit absence request", ((string[])(null)));
#line 104
this.ScenarioSetup(scenarioInfo);
#line 105
 testRunner.Given("I am an agent");
#line 106
 testRunner.And("I have a requestable absence called Illness");
#line 107
 testRunner.And("I have an existing absence request");
#line 108
 testRunner.And("I am viewing requests");
#line 109
 testRunner.When("I click on the request");
#line 110
 testRunner.And("I input new absence request values");
#line 111
 testRunner.And("I click the OK button");
#line 112
 testRunner.Then("I should see the new absence request values in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Delete absence request")]
        public virtual void DeleteAbsenceRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Delete absence request", ((string[])(null)));
#line 114
this.ScenarioSetup(scenarioInfo);
#line 115
 testRunner.Given("I am an agent");
#line 116
 testRunner.And("I have an existing absence request");
#line 117
 testRunner.And("I am viewing requests");
#line 118
 testRunner.When("I click the absence request\'s delete button");
#line 119
 testRunner.Then("I should not see the absence request in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can not edit approved absence requests")]
        public virtual void CanNotEditApprovedAbsenceRequests()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can not edit approved absence requests", ((string[])(null)));
#line 121
this.ScenarioSetup(scenarioInfo);
#line 122
 testRunner.Given("I am an agent");
#line 123
 testRunner.And("I have an approved absence request");
#line 124
 testRunner.And("I am viewing requests");
#line 125
 testRunner.When("I click on the request");
#line 126
 testRunner.Then("I should see the absence request\'s details form");
#line 127
 testRunner.And("I should not be able to input values for absence request");
#line 128
 testRunner.And("I should not see a save button");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can not edit denied absence requests")]
        public virtual void CanNotEditDeniedAbsenceRequests()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can not edit denied absence requests", ((string[])(null)));
#line 130
this.ScenarioSetup(scenarioInfo);
#line 131
 testRunner.Given("I am an agent");
#line 132
 testRunner.And("I have a denied absence request");
#line 133
 testRunner.And("I am viewing requests");
#line 134
 testRunner.When("I click on the request");
#line 135
 testRunner.Then("I should see the absence request\'s details form");
#line 136
 testRunner.And("I should not be able to input values for absence request");
#line 137
 testRunner.And("I should not see a save button");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can not delete approved absence request")]
        public virtual void CanNotDeleteApprovedAbsenceRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can not delete approved absence request", ((string[])(null)));
#line 139
this.ScenarioSetup(scenarioInfo);
#line 140
 testRunner.Given("I am an agent");
#line 141
 testRunner.And("I have an approved absence request");
#line 142
 testRunner.When("I am viewing requests");
#line 143
 testRunner.Then("I should not see a delete button");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can not delete denied absence request")]
        public virtual void CanNotDeleteDeniedAbsenceRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can not delete denied absence request", ((string[])(null)));
#line 145
this.ScenarioSetup(scenarioInfo);
#line 146
 testRunner.Given("I am an agent");
#line 147
 testRunner.And("I have a denied absence request");
#line 148
 testRunner.When("I am viewing requests");
#line 149
 testRunner.Then("I should not see a delete button");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can see why absence request was denied")]
        public virtual void CanSeeWhyAbsenceRequestWasDenied()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can see why absence request was denied", ((string[])(null)));
#line 151
this.ScenarioSetup(scenarioInfo);
#line 152
            testRunner.Given("I am an agent");
#line 153
            testRunner.And("I have a denied absence request beacuse of missing workflow control set");
#line 154
            testRunner.And("I am viewing requests");
#line 155
            testRunner.When("I click on the request");
#line 156
            testRunner.Then("I should see the absence request\'s details form");
#line 157
            testRunner.And("I should see that my request was denied with reason \'Din förfrågan kunde inte beh" +
                    "andlas. Du har inget arbetsflöde uppsatt.\'");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
