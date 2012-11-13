﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.8.1.0
//      SpecFlow Generator Version:1.8.0.0
//      Runtime Version:4.0.30319.544
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
    [NUnit.Framework.DescriptionAttribute("Week schedule time indicator")]
    public partial class WeekScheduleTimeIndicatorFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "WeekScheduleTimeIndicator.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Week schedule time indicator", "In order to get better control of my weekly schedule \r\nAs an agent\r\nI want to see" +
                    " an indication of the current and passed time in the week schedule", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        "No access to ASM"});
            table2.AddRow(new string[] {
                        "Access To Asm",
                        "false"});
#line 10
 testRunner.And("there is a role with", ((string)(null)), table2);
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Name"});
            table3.AddRow(new string[] {
                        "Day"});
#line 14
 testRunner.And("there are shift categories", ((string)(null)), table3);
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Do not show time indicator if no permission")]
        public virtual void DoNotShowTimeIndicatorIfNoPermission()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Do not show time indicator if no permission", ((string[])(null)));
#line 18
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 19
 testRunner.Given("I have the role \'No access to ASM\'");
#line 20
 testRunner.And("Current time is \'2030-10-03 12:00\'");
#line 21
 testRunner.When("I view my week schedule for date \'2030-10-03\'");
#line 22
 testRunner.Then("I should not see the time indicator for date \'2030-10-03\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show the time indicator at correct time")]
        public virtual void ShowTheTimeIndicatorAtCorrectTime()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show the time indicator at correct time", ((string[])(null)));
#line 24
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 25
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 26
 testRunner.And("Current time is \'2030-10-03 12:00\'");
#line 27
 testRunner.When("I view my week schedule for date \'2030-10-03\'");
#line 28
 testRunner.Then("I should see the time indicator at time \'2030-10-03 12:00\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show time indicator movement")]
        public virtual void ShowTimeIndicatorMovement()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show time indicator movement", ((string[])(null)));
#line 30
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 31
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 32
 testRunner.And("Current time is \'2030-09-20 12:00\'");
#line 33
 testRunner.And("I view my week schedule for date \'2030-09-20\'");
#line 34
 testRunner.And("I should see the time indicator at time \'2030-09-20 12:00\'");
#line 35
 testRunner.When("Current browser time has changed to \'2030-09-20 12:01\'");
#line 36
 testRunner.Then("I should see the time indicator at time \'2030-09-20 12:01\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show time indicator movement at midnight")]
        public virtual void ShowTimeIndicatorMovementAtMidnight()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show time indicator movement at midnight", ((string[])(null)));
#line 38
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 39
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 40
 testRunner.And("Current time is \'2030-09-20 23:59\'");
#line 41
 testRunner.And("I view my week schedule for date \'2030-09-20\'");
#line 42
 testRunner.And("I should see the time indicator at time \'2030-09-20 23:59\'");
#line 43
 testRunner.When("Current browser time has changed to \'2030-09-21 0:00\'");
#line 44
 testRunner.Then("I should see the time indicator at time \'2030-09-21 0:00\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Do not show time indicator when viewing other week than current")]
        public virtual void DoNotShowTimeIndicatorWhenViewingOtherWeekThanCurrent()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Do not show time indicator when viewing other week than current", ((string[])(null)));
#line 46
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 47
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 48
 testRunner.And("Current time is \'2030-03-12 12:00\'");
#line 49
 testRunner.When("I view my week schedule for date \'2030-03-05\'");
#line 50
 testRunner.Then("I should not see the time indicator");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show the time indicator at correct time with a shift")]
        public virtual void ShowTheTimeIndicatorAtCorrectTimeWithAShift()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show the time indicator at correct time with a shift", ((string[])(null)));
#line 52
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 53
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table4.AddRow(new string[] {
                        "Name",
                        "Published schedule"});
            table4.AddRow(new string[] {
                        "Schedule published to date",
                        "2040-06-24"});
#line 54
 testRunner.And("I have a workflow control set with", ((string)(null)), table4);
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table5.AddRow(new string[] {
                        "Start date",
                        "2012-06-18"});
            table5.AddRow(new string[] {
                        "Type",
                        "Week"});
            table5.AddRow(new string[] {
                        "Length",
                        "1"});
#line 58
 testRunner.And("I have a schedule period with", ((string)(null)), table5);
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table6.AddRow(new string[] {
                        "Start date",
                        "2012-06-18"});
#line 63
 testRunner.And("I have a person period with", ((string)(null)), table6);
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table7.AddRow(new string[] {
                        "StartTime",
                        "2030-01-01 10:00"});
            table7.AddRow(new string[] {
                        "EndTime",
                        "2030-01-01 12:00"});
            table7.AddRow(new string[] {
                        "Shift category",
                        "Day"});
#line 66
 testRunner.And("I have a shift with", ((string)(null)), table7);
#line 71
 testRunner.And("Current time is \'2030-01-01 11:00\'");
#line 72
 testRunner.When("I view my week schedule for date \'2030-01-01\'");
#line 73
 testRunner.Then("I should see the time indicator at time \'2030-01-01 11:00\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Do not show the time indicator after passing end of timeline")]
        public virtual void DoNotShowTheTimeIndicatorAfterPassingEndOfTimeline()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Do not show the time indicator after passing end of timeline", ((string[])(null)));
#line 75
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 76
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table8.AddRow(new string[] {
                        "Name",
                        "Published schedule"});
            table8.AddRow(new string[] {
                        "Schedule published to date",
                        "2040-06-24"});
#line 77
 testRunner.And("I have a workflow control set with", ((string)(null)), table8);
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table9.AddRow(new string[] {
                        "Start date",
                        "2012-06-18"});
            table9.AddRow(new string[] {
                        "Type",
                        "Week"});
            table9.AddRow(new string[] {
                        "Length",
                        "1"});
#line 81
 testRunner.And("I have a schedule period with", ((string)(null)), table9);
#line hidden
            TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table10.AddRow(new string[] {
                        "Start date",
                        "2012-06-18"});
#line 86
 testRunner.And("I have a person period with", ((string)(null)), table10);
#line hidden
            TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table11.AddRow(new string[] {
                        "StartTime",
                        "2030-03-12 04:00"});
            table11.AddRow(new string[] {
                        "EndTime",
                        "2030-03-12 12:00"});
            table11.AddRow(new string[] {
                        "Shift category",
                        "Day"});
#line 89
 testRunner.And("I have a shift with", ((string)(null)), table11);
#line 94
 testRunner.And("Current time is \'2030-03-12 12:00\'");
#line 95
 testRunner.And("I view my week schedule for date \'2030-03-12\'");
#line 96
 testRunner.And("I should see the time indicator at time \'2030-03-12 12:00\'");
#line 97
 testRunner.When("Current browser time has changed to \'2030-03-12 12:01\'");
#line 98
 testRunner.Then("I should not see the time indicator");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
