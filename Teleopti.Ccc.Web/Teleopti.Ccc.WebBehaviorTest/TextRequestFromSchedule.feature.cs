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
    [NUnit.Framework.DescriptionAttribute("Text request from schedule")]
    public partial class TextRequestFromScheduleFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "TextRequestFromSchedule.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Text request from schedule", "In order to make requests to my superior\r\nAs an agent\r\nI want to be able to submi" +
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
        
        public virtual void FeatureBackground()
        {
#line 6
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table1.AddRow(new string[] {
                        "Name",
                        "Full access to mytime"});
#line 7
 testRunner.Given("there is a role with", ((string)(null)), table1);
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table2.AddRow(new string[] {
                        "Name",
                        "No access to text requests"});
            table2.AddRow(new string[] {
                        "Access To Text Requests",
                        "False"});
#line 10
 testRunner.And("there is a role with", ((string)(null)), table2);
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Open add text request form")]
        public virtual void OpenAddTextRequestForm()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Open add text request form", ((string[])(null)));
#line 15
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 16
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 17
 testRunner.And("I view my week schedule for date \'2013-10-03\'");
#line 18
 testRunner.When("I click on the day symbol area for date \'2013-10-03\'");
#line 19
 testRunner.Then("I should see the text request form");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Open add text request form from day summary")]
        public virtual void OpenAddTextRequestFormFromDaySummary()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Open add text request form from day summary", ((string[])(null)));
#line 21
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 22
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 23
 testRunner.And("I view my week schedule for date \'2013-10-03\'");
#line 24
 testRunner.When("I click on the day summary for date \'2013-10-03\'");
#line 25
 testRunner.Then("I should see the text request form");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Add text request from week schedule view")]
        public virtual void AddTextRequestFromWeekScheduleView()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Add text request from week schedule view", ((string[])(null)));
#line 27
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 28
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 29
 testRunner.And("I view my week schedule for date \'2013-10-03\'");
#line 30
 testRunner.When("I click on the day symbol area for date \'2013-10-03\'");
#line 31
 testRunner.And("I input text request values for date \'2013-10-03\'");
#line 32
 testRunner.And("I click the OK button");
#line 33
 testRunner.Then("I should see a symbol at the top of the schedule for date \'2013-10-03\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can not add text request from day symbol area if no permission")]
        public virtual void CanNotAddTextRequestFromDaySymbolAreaIfNoPermission()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can not add text request from day symbol area if no permission", ((string[])(null)));
#line 35
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 36
 testRunner.Given("I have the role \'No access to text requests\'");
#line 37
 testRunner.And("I view my week schedule for date \'2013-10-03\'");
#line 38
 testRunner.When("I click on the day symbol area for date \'2013-10-03\'");
#line 39
 testRunner.Then("I should not see the text request form");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can not add text request from day summary if no permission")]
        public virtual void CanNotAddTextRequestFromDaySummaryIfNoPermission()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can not add text request from day summary if no permission", ((string[])(null)));
#line 41
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 42
 testRunner.Given("I have the role \'No access to text requests\'");
#line 43
 testRunner.And("I view my week schedule for date \'2013-10-03\'");
#line 44
 testRunner.When("I click on the day summary for date \'2013-10-03\'");
#line 45
 testRunner.Then("I should not see the text request form");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Default text request values from week schedule")]
        public virtual void DefaultTextRequestValuesFromWeekSchedule()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Default text request values from week schedule", ((string[])(null)));
#line 47
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 48
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 49
 testRunner.And("I view my week schedule for date \'2013-10-03\'");
#line 50
 testRunner.When("I click on the day summary for date \'2013-10-03\'");
#line 51
 testRunner.Then("I should see the text request form with \'2013-10-03\' as default date");
#line 52
 testRunner.And("I should see 8:00 - 17:00 as the default times");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Default full day text request values from week schedule")]
        public virtual void DefaultFullDayTextRequestValuesFromWeekSchedule()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Default full day text request values from week schedule", ((string[])(null)));
#line 54
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 55
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 56
 testRunner.And("I view my week schedule for date \'2013-10-03\'");
#line 57
 testRunner.When("I click on the day symbol area for date \'2013-10-03\'");
#line 58
 testRunner.And("I checked the full day checkbox");
#line 59
 testRunner.Then("I should see 00:00 - 23:59 as the default times");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Cancel adding text request")]
        public virtual void CancelAddingTextRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Cancel adding text request", ((string[])(null)));
#line 61
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 62
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 63
 testRunner.And("I view my week schedule for date \'2013-10-03\'");
#line 64
 testRunner.When("I click on the day symbol area for date \'2013-10-03\'");
#line 65
 testRunner.And("I input text request values for date \'2013-10-03\'");
#line 66
 testRunner.And("I click the Cancel button");
#line 67
 testRunner.Then("I should not see a symbol at the top of the schedule for date \'2013-10-03\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Adding invalid text request values")]
        public virtual void AddingInvalidTextRequestValues()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Adding invalid text request values", ((string[])(null)));
#line 69
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 70
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 71
 testRunner.And("I view my week schedule for date \'2013-10-03\'");
#line 72
 testRunner.When("I click on the day symbol area for date \'2013-10-03\'");
#line 73
 testRunner.And("I input empty subject");
#line 74
 testRunner.And("I input later start time than end time for date \'2013-10-03\'");
#line 75
 testRunner.And("I click the OK button");
#line 76
 testRunner.Then("I should see texts describing my errors");
#line 77
 testRunner.And("I should not see a symbol at the top of the schedule for date \'2013-10-03\'");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
