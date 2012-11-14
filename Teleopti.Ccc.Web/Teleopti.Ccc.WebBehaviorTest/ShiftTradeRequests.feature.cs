﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.8.1.0
//      SpecFlow Generator Version:1.8.0.0
//      Runtime Version:4.0.30319.586
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
    [NUnit.Framework.DescriptionAttribute("Shift Trade Requests")]
    public partial class ShiftTradeRequestsFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "ShiftTradeRequests.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Shift Trade Requests", "In order to avoid an unwanted scheduled shifts\r\nAs an agent\r\nI want to be able to" +
                    " trade shifts with other agents", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        "No access to Shift Trade"});
            table2.AddRow(new string[] {
                        "Access To Shift Trade Requests",
                        "False"});
#line 10
 testRunner.And("there is a role with", ((string)(null)), table2);
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Name"});
            table3.AddRow(new string[] {
                        "Day"});
            table3.AddRow(new string[] {
                        "Late"});
#line 14
 testRunner.And("there are shift categories", ((string)(null)), table3);
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table4.AddRow(new string[] {
                        "Name",
                        "DayOff"});
#line 18
 testRunner.And("there is a dayoff with", ((string)(null)), table4);
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table5.AddRow(new string[] {
                        "Name",
                        "Illness"});
#line 21
 testRunner.And("there is an absence with", ((string)(null)), table5);
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table6.AddRow(new string[] {
                        "Name",
                        "Skill 1"});
#line 24
 testRunner.And("there is a skill with", ((string)(null)), table6);
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table7.AddRow(new string[] {
                        "Name",
                        "Published schedule"});
            table7.AddRow(new string[] {
                        "Schedule published to date",
                        "2040-06-24"});
#line 27
 testRunner.And("I have a workflow control set with", ((string)(null)), table7);
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table8.AddRow(new string[] {
                        "Start date",
                        "2012-06-18"});
            table8.AddRow(new string[] {
                        "Type",
                        "Week"});
            table8.AddRow(new string[] {
                        "Length",
                        "1"});
#line 31
 testRunner.And("I have a schedule period with", ((string)(null)), table8);
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table9.AddRow(new string[] {
                        "Start date",
                        "2012-06-18"});
            table9.AddRow(new string[] {
                        "Start date",
                        "2012-06-18"});
            table9.AddRow(new string[] {
                        "Skill",
                        "Skill 1"});
#line 36
 testRunner.And("I have a person period with", ((string)(null)), table9);
#line hidden
            TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table10.AddRow(new string[] {
                        "StartTime",
                        "2030-01-01 06:00"});
            table10.AddRow(new string[] {
                        "EndTime",
                        "2030-01-01 16:00"});
            table10.AddRow(new string[] {
                        "Shift category",
                        "Day"});
            table10.AddRow(new string[] {
                        "Lunch3HoursAfterStart",
                        "true"});
#line 41
 testRunner.And("I have a shift with", ((string)(null)), table10);
#line hidden
            TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table11.AddRow(new string[] {
                        "Agent name",
                        "Other agent 1"});
            table11.AddRow(new string[] {
                        "Name",
                        "Published schedule"});
            table11.AddRow(new string[] {
                        "Schedule published to date",
                        "2040-06-24"});
#line 47
 testRunner.And("an agent has a workflow control set with", ((string)(null)), table11);
#line hidden
            TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table12.AddRow(new string[] {
                        "Agent name",
                        "Other agent 1"});
            table12.AddRow(new string[] {
                        "Start date",
                        "2012-06-18"});
            table12.AddRow(new string[] {
                        "Type",
                        "Week"});
            table12.AddRow(new string[] {
                        "Length",
                        "1"});
#line 52
 testRunner.And("an agent has a schedule period with", ((string)(null)), table12);
#line hidden
            TechTalk.SpecFlow.Table table13 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table13.AddRow(new string[] {
                        "Agent name",
                        "Other agent 1"});
            table13.AddRow(new string[] {
                        "Start date",
                        "2012-06-18"});
            table13.AddRow(new string[] {
                        "Skill",
                        "Skill 1"});
#line 58
 testRunner.And("an agent has a person period with", ((string)(null)), table13);
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Do not show shift trade request tab if no permission")]
        public virtual void DoNotShowShiftTradeRequestTabIfNoPermission()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Do not show shift trade request tab if no permission", ((string[])(null)));
#line 65
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 66
 testRunner.Given("I have the role \'No access to Shift Trade\'");
#line 67
 testRunner.When("I sign in");
#line 68
 testRunner.Then("shift trade tab should not be visible");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Default to first day of open period when viewing shift trade")]
        public virtual void DefaultToFirstDayOfOpenPeriodWhenViewingShiftTrade()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Default to first day of open period when viewing shift trade", ((string[])(null)));
#line 70
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 71
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 72
 testRunner.And("Current time is \'2030-01-01\'");
#line 73
 testRunner.And("I can do shift trades between \'2030-01-03\' and \'2030-01-17\'");
#line 74
 testRunner.When("I navigate to shift trade page");
#line 75
 testRunner.Then("the selected date should be \'2030-01-03\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Default time line when I am not scheduled")]
        public virtual void DefaultTimeLineWhenIAmNotScheduled()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Default time line when I am not scheduled", ((string[])(null)));
#line 77
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 78
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 79
 testRunner.And("Current time is \'2020-10-24\'");
#line 80
 testRunner.When("I navigate to shift trade page");
#line 81
 testRunner.Then("I should see the time line span from \'7:45\' to \'17:15\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Time line when I have a scheduled shift")]
        public virtual void TimeLineWhenIHaveAScheduledShift()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Time line when I have a scheduled shift", ((string[])(null)));
#line 83
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 84
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 85
 testRunner.And("Current time is \'2030-01-01\'");
#line 86
 testRunner.When("I navigate to shift trade page");
#line 87
 testRunner.Then("I should see the time line span from \'5:45\' to \'16:15\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show my scheduled shift")]
        public virtual void ShowMyScheduledShift()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show my scheduled shift", ((string[])(null)));
#line 89
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 90
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 91
 testRunner.And("Current time is \'2030-01-01\'");
#line 92
 testRunner.When("I navigate to shift trade page");
#line hidden
            TechTalk.SpecFlow.Table table14 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table14.AddRow(new string[] {
                        "Start time",
                        "06:00"});
            table14.AddRow(new string[] {
                        "End time",
                        "16:00"});
#line 93
 testRunner.Then("I should see my schedule with", ((string)(null)), table14);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show my scheduled day off")]
        public virtual void ShowMyScheduledDayOff()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show my scheduled day off", ((string[])(null)));
#line 98
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 99
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table15 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table15.AddRow(new string[] {
                        "Date",
                        "2030-01-03"});
            table15.AddRow(new string[] {
                        "Name",
                        "DayOff"});
#line 100
 testRunner.And("I have a day off with", ((string)(null)), table15);
#line 104
 testRunner.And("Current time is \'2030-01-03\'");
#line 105
 testRunner.When("I navigate to shift trade page");
#line 106
 testRunner.Then("I should see my scheduled day off \'DayOff\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show my full-day absence")]
        public virtual void ShowMyFull_DayAbsence()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show my full-day absence", ((string[])(null)));
#line 108
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 109
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table16 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table16.AddRow(new string[] {
                        "Date",
                        "2030-01-05"});
            table16.AddRow(new string[] {
                        "Name",
                        "Illness"});
            table16.AddRow(new string[] {
                        "Full Day",
                        "True"});
#line 110
 testRunner.And("I have absence with", ((string)(null)), table16);
#line 115
 testRunner.And("Current time is \'2030-01-05\'");
#line 116
 testRunner.When("I navigate to shift trade page");
#line 117
 testRunner.Then("I should see my scheduled absence \'Illness\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show message when no possible shift trades")]
        public virtual void ShowMessageWhenNoPossibleShiftTrades()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show message when no possible shift trades", ((string[])(null)));
#line 119
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 120
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 121
 testRunner.And("Current time is \'2030-01-05\'");
#line 122
 testRunner.And("I can do shift trades between \'2030-01-06\' and \'2030-01-17\'");
#line 123
 testRunner.When("I navigate to shift trade page for date \'2030-01-05\'");
#line 124
 testRunner.Then("I should see a user-friendly message explaining that shift trades cannot be made");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("One possible shift to trade with because shift trade periods match")]
        public virtual void OnePossibleShiftToTradeWithBecauseShiftTradePeriodsMatch()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("One possible shift to trade with because shift trade periods match", ((string[])(null)));
#line 126
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 127
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 128
 testRunner.And("Current time is \'2029-12-29\'");
#line 129
 testRunner.And("I can do shift trades between \'2030-01-01\' and \'2030-01-17\'");
#line 130
 testRunner.And("another agent named \'Other agent 1\' can do shift trades between \'2030-01-01\' and " +
                    "\'2030-01-17\'");
#line hidden
            TechTalk.SpecFlow.Table table17 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table17.AddRow(new string[] {
                        "Agent name",
                        "Other agent 1"});
            table17.AddRow(new string[] {
                        "StartTime",
                        "2030-01-01 10:00"});
            table17.AddRow(new string[] {
                        "EndTime",
                        "2030-01-01 20:00"});
            table17.AddRow(new string[] {
                        "Shift category",
                        "Late"});
            table17.AddRow(new string[] {
                        "Lunch3HoursAfterStart",
                        "true"});
#line 131
 testRunner.And("an agent has a shift with", ((string)(null)), table17);
#line 138
 testRunner.When("I navigate to shift trade page");
#line 139
 testRunner.Then("I should have one possible shift to trade with");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Not possible to trade shift because no matching skills")]
        public virtual void NotPossibleToTradeShiftBecauseNoMatchingSkills()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Not possible to trade shift because no matching skills", ((string[])(null)));
#line 141
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 142
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 143
 testRunner.And("Current time is \'2029-12-29\'");
#line 144
 testRunner.And("I can do shift trades between \'2030-01-01\' and \'2030-01-17\'");
#line 145
 testRunner.And("another agent named \'Other agent 1\' can do shift trades between \'2030-01-01\' and " +
                    "\'2030-01-17\'");
#line hidden
            TechTalk.SpecFlow.Table table18 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table18.AddRow(new string[] {
                        "Agent name",
                        "Other agent 1"});
            table18.AddRow(new string[] {
                        "StartTime",
                        "2030-01-01 10:00"});
            table18.AddRow(new string[] {
                        "EndTime",
                        "2030-01-01 20:00"});
            table18.AddRow(new string[] {
                        "Shift category",
                        "Late"});
            table18.AddRow(new string[] {
                        "Lunch3HoursAfterStart",
                        "true"});
#line 146
 testRunner.And("an agent has a shift with", ((string)(null)), table18);
#line hidden
            TechTalk.SpecFlow.Table table19 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table19.AddRow(new string[] {
                        "Name",
                        "Published schedule"});
            table19.AddRow(new string[] {
                        "Shift trade matching skill",
                        "Skill 1"});
#line 153
 testRunner.And("I have a updated workflow control set with", ((string)(null)), table19);
#line 157
 testRunner.When("I navigate to shift trade page");
#line 158
 testRunner.Then("I should see a user-friendly message explaining that shift trades cannot be made");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("One possible shift to trade with because shift trade periods and skills are match" +
            "ing")]
        public virtual void OnePossibleShiftToTradeWithBecauseShiftTradePeriodsAndSkillsAreMatching()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("One possible shift to trade with because shift trade periods and skills are match" +
                    "ing", ((string[])(null)));
#line 160
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 161
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 162
 testRunner.And("Current time is \'2029-12-29\'");
#line 163
 testRunner.And("I can do shift trades between \'2030-01-01\' and \'2030-01-17\'");
#line 164
 testRunner.And("another agent named \'Other agent 1\' can do shift trades between \'2030-01-01\' and " +
                    "\'2030-01-17\'");
#line hidden
            TechTalk.SpecFlow.Table table20 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table20.AddRow(new string[] {
                        "Agent name",
                        "Other agent 1"});
            table20.AddRow(new string[] {
                        "StartTime",
                        "2030-01-01 10:00"});
            table20.AddRow(new string[] {
                        "EndTime",
                        "2030-01-01 20:00"});
            table20.AddRow(new string[] {
                        "Shift category",
                        "Late"});
            table20.AddRow(new string[] {
                        "Lunch3HoursAfterStart",
                        "true"});
#line 165
 testRunner.And("an agent has a shift with", ((string)(null)), table20);
#line hidden
            TechTalk.SpecFlow.Table table21 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table21.AddRow(new string[] {
                        "Name",
                        "Published schedule"});
            table21.AddRow(new string[] {
                        "Shift trade matching skill",
                        "Skill 1"});
#line 172
 testRunner.And("I have a updated workflow control set with", ((string)(null)), table21);
#line hidden
            TechTalk.SpecFlow.Table table22 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table22.AddRow(new string[] {
                        "Agent name",
                        "Other agent 1"});
            table22.AddRow(new string[] {
                        "Name",
                        "Published schedule"});
            table22.AddRow(new string[] {
                        "Shift trade matching skill",
                        "Skill 1"});
#line 176
 testRunner.And("an agent has a updated workflow control set with", ((string)(null)), table22);
#line 181
 testRunner.When("I navigate to shift trade page");
#line 182
 testRunner.Then("I should have one possible shift to trade with");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
