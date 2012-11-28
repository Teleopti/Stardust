﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.77
//      SpecFlow Generator Version:1.9.0.0
//      Runtime Version:4.0.30319.17929
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
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
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
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Shift Trade Requests", "In order to avoid unwanted scheduled shifts\r\nAs an agent\r\nI want to be able to tr" +
                    "ade shifts with other agents", ProgrammingLanguage.CSharp, ((string[])(null)));
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
#line 7
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table1.AddRow(new string[] {
                        "Name",
                        "Full access to mytime"});
#line 8
 testRunner.Given("there is a role with", ((string)(null)), table1, "Given ");
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
#line 11
 testRunner.And("there is a role with", ((string)(null)), table2, "And ");
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("No access to make shift trade reuquests")]
        public virtual void NoAccessToMakeShiftTradeReuquests()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("No access to make shift trade reuquests", ((string[])(null)));
#line 65
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 66
 testRunner.Given("I have the role \'No access to Shift Trade\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 67
 testRunner.When("I view requests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 68
 testRunner.Then("I should not see the Create Shift Trade Request button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 69
 testRunner.And("I should not see the Requests button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Default to today if no open shift trade period")]
        public virtual void DefaultToTodayIfNoOpenShiftTradePeriod()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Default to today if no open shift trade period", ((string[])(null)));
#line 71
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 72
 testRunner.Given("I have the role \'Full access to mytime\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 73
 testRunner.And("I have no workflow control set", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 74
 testRunner.And("Current time is \'2030-01-01\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 75
 testRunner.When("I navigate to shift trade page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 76
 testRunner.And("I navigate to messages", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 77
 testRunner.Then("the selected date should be \'2030-01-01\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Default to first day of open shift trade period")]
        public virtual void DefaultToFirstDayOfOpenShiftTradePeriod()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Default to first day of open shift trade period", ((string[])(null)));
#line 79
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 80
 testRunner.Given("I have the role \'Full access to mytime\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 81
 testRunner.And("Current time is \'2030-01-01\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 82
 testRunner.And("I can do shift trades between \'2030-01-03\' and \'2030-01-17\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 83
 testRunner.When("I navigate to shift trade page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 84
 testRunner.Then("the selected date should be \'2030-01-03\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Default time line when I am not scheduled")]
        public virtual void DefaultTimeLineWhenIAmNotScheduled()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Default time line when I am not scheduled", ((string[])(null)));
#line 86
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 87
 testRunner.Given("I have the role \'Full access to mytime\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 88
 testRunner.And("Current time is \'2020-10-24\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 89
 testRunner.When("I navigate to shift trade page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 90
 testRunner.Then("I should see the time line span from \'7:45\' to \'17:15\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Time line when I have a scheduled shift")]
        public virtual void TimeLineWhenIHaveAScheduledShift()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Time line when I have a scheduled shift", ((string[])(null)));
#line 92
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 93
 testRunner.Given("I have the role \'Full access to mytime\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 94
 testRunner.And("Current time is \'2030-01-01\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 95
 testRunner.When("I navigate to shift trade page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 96
 testRunner.Then("I should see the time line span from \'5:45\' to \'16:15\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show my scheduled shift")]
        public virtual void ShowMyScheduledShift()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show my scheduled shift", ((string[])(null)));
#line 98
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 99
 testRunner.Given("I have the role \'Full access to mytime\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 100
 testRunner.And("Current time is \'2030-01-01\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 101
 testRunner.When("I navigate to shift trade page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table3.AddRow(new string[] {
                        "Start time",
                        "06:00"});
            table3.AddRow(new string[] {
                        "End time",
                        "16:00"});
#line 102
 testRunner.Then("I should see my schedule with", ((string)(null)), table3, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show my scheduled day off")]
        public virtual void ShowMyScheduledDayOff()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show my scheduled day off", ((string[])(null)));
#line 107
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 108
 testRunner.Given("I have the role \'Full access to mytime\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table4.AddRow(new string[] {
                        "Date",
                        "2030-01-03"});
            table4.AddRow(new string[] {
                        "Name",
                        "DayOff"});
#line 109
 testRunner.And("I have a day off with", ((string)(null)), table4, "And ");
#line 113
 testRunner.And("Current time is \'2030-01-03\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 114
 testRunner.When("I navigate to shift trade page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 115
 testRunner.Then("I should see my scheduled day off \'DayOff\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show my full-day absence")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void ShowMyFull_DayAbsence()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show my full-day absence", new string[] {
                        "ignore"});
#line 118
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 119
 testRunner.Given("I have the role \'Full access to mytime\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table5.AddRow(new string[] {
                        "Date",
                        "2030-01-05"});
            table5.AddRow(new string[] {
                        "Name",
                        "Illness"});
            table5.AddRow(new string[] {
                        "Full Day",
                        "True"});
#line 120
 testRunner.And("I have absence with", ((string)(null)), table5, "And ");
#line 125
 testRunner.And("Current time is \'2030-01-05\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 126
 testRunner.When("I navigate to shift trade page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 127
 testRunner.Then("I should see my scheduled absence \'Illness\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show message when no possible shift trades")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void ShowMessageWhenNoPossibleShiftTrades()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show message when no possible shift trades", new string[] {
                        "ignore"});
#line 130
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 131
 testRunner.Given("I have the role \'Full access to mytime\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 132
 testRunner.And("Current time is \'2030-01-05\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 133
 testRunner.And("I can do shift trades between \'2030-01-06\' and \'2030-01-17\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 134
 testRunner.When("I navigate to shift trade page for date \'2030-01-05\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 135
 testRunner.Then("I should see a user-friendly message explaining that shift trades cannot be made", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("One possible shift to trade with because shift trade periods match")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void OnePossibleShiftToTradeWithBecauseShiftTradePeriodsMatch()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("One possible shift to trade with because shift trade periods match", new string[] {
                        "ignore"});
#line 138
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 139
 testRunner.Given("I have the role \'Full access to mytime\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 140
 testRunner.And("Current time is \'2029-12-29\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 141
 testRunner.And("I can do shift trades between \'2030-01-01\' and \'2030-01-17\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 142
 testRunner.And("another agent named \'Other agent 1\' can do shift trades between \'2030-01-01\' and " +
                    "\'2030-01-17\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table6.AddRow(new string[] {
                        "Agent name",
                        "Other agent 1"});
            table6.AddRow(new string[] {
                        "StartTime",
                        "2030-01-01 10:00"});
            table6.AddRow(new string[] {
                        "EndTime",
                        "2030-01-01 20:00"});
            table6.AddRow(new string[] {
                        "Shift category",
                        "Late"});
            table6.AddRow(new string[] {
                        "Lunch3HoursAfterStart",
                        "true"});
#line 143
 testRunner.And("an agent has a shift with", ((string)(null)), table6, "And ");
#line 150
 testRunner.When("I navigate to shift trade page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 151
 testRunner.Then("I should have one possible shift to trade with", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Not possible to trade shift because no matching skills")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void NotPossibleToTradeShiftBecauseNoMatchingSkills()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Not possible to trade shift because no matching skills", new string[] {
                        "ignore"});
#line 154
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 155
 testRunner.Given("I have the role \'Full access to mytime\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 156
 testRunner.And("Current time is \'2029-12-29\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 157
 testRunner.And("I can do shift trades between \'2030-01-01\' and \'2030-01-17\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 158
 testRunner.And("another agent named \'Other agent 1\' can do shift trades between \'2030-01-01\' and " +
                    "\'2030-01-17\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table7.AddRow(new string[] {
                        "Agent name",
                        "Other agent 1"});
            table7.AddRow(new string[] {
                        "StartTime",
                        "2030-01-01 10:00"});
            table7.AddRow(new string[] {
                        "EndTime",
                        "2030-01-01 20:00"});
            table7.AddRow(new string[] {
                        "Shift category",
                        "Late"});
            table7.AddRow(new string[] {
                        "Lunch3HoursAfterStart",
                        "true"});
#line 159
 testRunner.And("an agent has a shift with", ((string)(null)), table7, "And ");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table8.AddRow(new string[] {
                        "Name",
                        "Published schedule"});
            table8.AddRow(new string[] {
                        "Shift trade matching skill",
                        "Skill 1"});
#line 166
 testRunner.And("I have a updated workflow control set with", ((string)(null)), table8, "And ");
#line 170
 testRunner.When("I navigate to shift trade page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 171
 testRunner.Then("I should see a user-friendly message explaining that shift trades cannot be made", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("One possible shift to trade with because shift trade periods and skills are match" +
            "ing")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void OnePossibleShiftToTradeWithBecauseShiftTradePeriodsAndSkillsAreMatching()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("One possible shift to trade with because shift trade periods and skills are match" +
                    "ing", new string[] {
                        "ignore"});
#line 174
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 175
 testRunner.Given("I have the role \'Full access to mytime\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 176
 testRunner.And("Current time is \'2029-12-29\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 177
 testRunner.And("I can do shift trades between \'2030-01-01\' and \'2030-01-17\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 178
 testRunner.And("another agent named \'Other agent 1\' can do shift trades between \'2030-01-01\' and " +
                    "\'2030-01-17\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table9.AddRow(new string[] {
                        "Agent name",
                        "Other agent 1"});
            table9.AddRow(new string[] {
                        "StartTime",
                        "2030-01-01 10:00"});
            table9.AddRow(new string[] {
                        "EndTime",
                        "2030-01-01 20:00"});
            table9.AddRow(new string[] {
                        "Shift category",
                        "Late"});
            table9.AddRow(new string[] {
                        "Lunch3HoursAfterStart",
                        "true"});
#line 179
 testRunner.And("an agent has a shift with", ((string)(null)), table9, "And ");
#line hidden
            TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table10.AddRow(new string[] {
                        "Name",
                        "Published schedule"});
            table10.AddRow(new string[] {
                        "Shift trade matching skill",
                        "Skill 1"});
#line 186
 testRunner.And("I have a updated workflow control set with", ((string)(null)), table10, "And ");
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
                        "Shift trade matching skill",
                        "Skill 1"});
#line 190
 testRunner.And("an agent has a updated workflow control set with", ((string)(null)), table11, "And ");
#line 195
 testRunner.When("I navigate to shift trade page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 196
 testRunner.Then("I should have one possible shift to trade with", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View shift trade request details")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void ViewShiftTradeRequestDetails()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View shift trade request details", new string[] {
                        "ignore"});
#line 199
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 200
 testRunner.Given("I have the role \'Full access to mytime\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 201
 testRunner.And("I have an existing shift trade request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 202
 testRunner.And("I am viewing requests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 203
 testRunner.When("I click on the request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 204
 testRunner.Then("I should see the shift trade request form", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Approve shift trade request")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void ApproveShiftTradeRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Approve shift trade request", new string[] {
                        "ignore"});
#line 207
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 208
 testRunner.Given("I have the role \'Full access to mytime\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 209
 testRunner.And("I have created a shift trade request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 210
 testRunner.And("I am viewing requests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 211
 testRunner.When("I click on the request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 212
 testRunner.And("I click the Approve button on the shift request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 213
 testRunner.Then("I should not see the shift trade request in the list", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Deny shift trade request")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void DenyShiftTradeRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Deny shift trade request", new string[] {
                        "ignore"});
#line 216
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 217
 testRunner.Given("I have the role \'Full access to mytime\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 218
 testRunner.And("I have received a shift trade request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 219
 testRunner.And("I am viewing requests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 220
 testRunner.When("I click on the request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 221
 testRunner.And("I click the Deny button on the shift request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 222
 testRunner.Then("I should not see the shift trade request in the list", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Delete created shift trade request")]
        public virtual void DeleteCreatedShiftTradeRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Delete created shift trade request", ((string[])(null)));
#line 224
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 225
 testRunner.Given("I have the role \'Full access to mytime\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 226
 testRunner.And("I have created a shift trade request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 227
 testRunner.And("I am viewing requests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 228
 testRunner.And("I click the shift trade request\'s delete button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 229
 testRunner.Then("I should not see the shift trade request in the list", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Should not be able to delete received shift trade request")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void ShouldNotBeAbleToDeleteReceivedShiftTradeRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Should not be able to delete received shift trade request", new string[] {
                        "ignore"});
#line 232
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 233
 testRunner.Given("I have the role \'Full access to mytime\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 234
 testRunner.And("I have received a shift trade request from \'Ashley\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 235
 testRunner.And("I am viewing requests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 236
 testRunner.Then("I should not see any delete button on my existing shift trade request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Approve shift trade on same day request should update shift in schedule")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void ApproveShiftTradeOnSameDayRequestShouldUpdateShiftInSchedule()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Approve shift trade on same day request should update shift in schedule", new string[] {
                        "ignore"});
#line 239
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 240
 testRunner.Given("I have the role \'Full access to mytime\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 241
 testRunner.And("Current time is \'2012-01-14\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table12.AddRow(new string[] {
                        "StartTime",
                        "2012-01-15 10:00"});
            table12.AddRow(new string[] {
                        "EndTime",
                        "2012-01-15 15:00"});
            table12.AddRow(new string[] {
                        "Shift category",
                        "Night"});
#line 242
 testRunner.And("I have a shift with", ((string)(null)), table12, "And ");
#line hidden
            TechTalk.SpecFlow.Table table13 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table13.AddRow(new string[] {
                        "Agent name",
                        "Other agent 1"});
            table13.AddRow(new string[] {
                        "StartTime",
                        "2012-01-15 11:00"});
            table13.AddRow(new string[] {
                        "EndTime",
                        "2012-01-15 16:00"});
            table13.AddRow(new string[] {
                        "Shift category",
                        "Late"});
#line 247
 testRunner.And("an agent has a shift with", ((string)(null)), table13, "And ");
#line 253
 testRunner.And("I have an existing shift trade request for \'2012-01-15\' with \'Other agent 1\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 254
 testRunner.And("I am viewing requests", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 255
 testRunner.When("I click on the request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 256
 testRunner.And("I click the Approve button on the shift request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 257
 testRunner.And("I navigate to week schedule page for date \'2012-01-15\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 258
 testRunner.When("I view my week schedule for date \'2012-01-15\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table14 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table14.AddRow(new string[] {
                        "First activity times",
                        "11:00 - 16:00"});
#line 259
 testRunner.Then("I should see activities on date \'2012-01-15\' with:", ((string)(null)), table14, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
