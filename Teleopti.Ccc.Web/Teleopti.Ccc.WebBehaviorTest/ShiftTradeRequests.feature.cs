﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.8.1.0
//      SpecFlow Generator Version:1.8.0.0
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
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("No access to make shift trade reuquests")]
        public virtual void NoAccessToMakeShiftTradeReuquests()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("No access to make shift trade reuquests", ((string[])(null)));
#line 65
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 66
 testRunner.Given("I have the role \'No access to Shift Trade\'");
#line 67
 testRunner.When("I view requests");
#line 68
 testRunner.Then("I should not see the Create Shift Trade Request button");
#line 69
 testRunner.And("I should not see the Requests button");
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
#line 6
this.FeatureBackground();
#line 72
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 73
 testRunner.And("I have no workflow control set");
#line 74
 testRunner.And("Current time is \'2030-01-01\'");
#line 75
 testRunner.When("I navigate to shift trade page");
#line 76
 testRunner.And("I navigate to messages");
#line 77
 testRunner.Then("the selected date should be \'2030-01-01\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Default to first day of open shift trade period")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void DefaultToFirstDayOfOpenShiftTradePeriod()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Default to first day of open shift trade period", new string[] {
                        "ignore"});
#line 79
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 80
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 81
 testRunner.And("Current time is \'2030-01-01\'");
#line 82
 testRunner.And("I can do shift trades between \'2030-01-03\' and \'2030-01-17\'");
#line 83
 testRunner.When("I navigate to shift trade page");
#line 84
 testRunner.Then("the selected date should be \'2030-01-03\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Default time line when I am not scheduled")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void DefaultTimeLineWhenIAmNotScheduled()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Default time line when I am not scheduled", new string[] {
                        "ignore"});
#line 86
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 87
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 88
 testRunner.And("Current time is \'2020-10-24\'");
#line 89
 testRunner.When("I navigate to shift trade page");
#line 90
 testRunner.Then("I should see the time line span from \'7:45\' to \'17:15\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Time line when I have a scheduled shift")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void TimeLineWhenIHaveAScheduledShift()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Time line when I have a scheduled shift", new string[] {
                        "ignore"});
#line 92
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 93
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 94
 testRunner.And("Current time is \'2030-01-01\'");
#line 95
 testRunner.When("I navigate to shift trade page");
#line 96
 testRunner.Then("I should see the time line span from \'5:45\' to \'16:15\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show my scheduled shift")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void ShowMyScheduledShift()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show my scheduled shift", new string[] {
                        "ignore"});
#line 98
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 99
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 100
 testRunner.And("Current time is \'2030-01-01\'");
#line 101
 testRunner.When("I navigate to shift trade page");
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
 testRunner.Then("I should see my schedule with", ((string)(null)), table3);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show my scheduled day off")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void ShowMyScheduledDayOff()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show my scheduled day off", new string[] {
                        "ignore"});
#line 107
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 108
 testRunner.Given("I have the role \'Full access to mytime\'");
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
 testRunner.And("I have a day off with", ((string)(null)), table4);
#line 113
 testRunner.And("Current time is \'2030-01-03\'");
#line 114
 testRunner.When("I navigate to shift trade page");
#line 115
 testRunner.Then("I should see my scheduled day off \'DayOff\'");
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
#line 117
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 118
 testRunner.Given("I have the role \'Full access to mytime\'");
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
#line 119
 testRunner.And("I have absence with", ((string)(null)), table5);
#line 124
 testRunner.And("Current time is \'2030-01-05\'");
#line 125
 testRunner.When("I navigate to shift trade page");
#line 126
 testRunner.Then("I should see my scheduled absence \'Illness\'");
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
#line 128
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 129
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 130
 testRunner.And("Current time is \'2030-01-05\'");
#line 131
 testRunner.And("I can do shift trades between \'2030-01-06\' and \'2030-01-17\'");
#line 132
 testRunner.When("I navigate to shift trade page for date \'2030-01-05\'");
#line 133
 testRunner.Then("I should see a user-friendly message explaining that shift trades cannot be made");
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
#line 135
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 136
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 137
 testRunner.And("Current time is \'2029-12-29\'");
#line 138
 testRunner.And("I can do shift trades between \'2030-01-01\' and \'2030-01-17\'");
#line 139
 testRunner.And("another agent named \'Other agent 1\' can do shift trades between \'2030-01-01\' and " +
                    "\'2030-01-17\'");
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
#line 140
 testRunner.And("an agent has a shift with", ((string)(null)), table6);
#line 147
 testRunner.When("I navigate to shift trade page");
#line 148
 testRunner.Then("I should have one possible shift to trade with");
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
#line 150
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 151
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 152
 testRunner.And("Current time is \'2029-12-29\'");
#line 153
 testRunner.And("I can do shift trades between \'2030-01-01\' and \'2030-01-17\'");
#line 154
 testRunner.And("another agent named \'Other agent 1\' can do shift trades between \'2030-01-01\' and " +
                    "\'2030-01-17\'");
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
#line 155
 testRunner.And("an agent has a shift with", ((string)(null)), table7);
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
#line 162
 testRunner.And("I have a updated workflow control set with", ((string)(null)), table8);
#line 166
 testRunner.When("I navigate to shift trade page");
#line 167
 testRunner.Then("I should see a user-friendly message explaining that shift trades cannot be made");
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
#line 169
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 170
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 171
 testRunner.And("Current time is \'2029-12-29\'");
#line 172
 testRunner.And("I can do shift trades between \'2030-01-01\' and \'2030-01-17\'");
#line 173
 testRunner.And("another agent named \'Other agent 1\' can do shift trades between \'2030-01-01\' and " +
                    "\'2030-01-17\'");
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
#line 174
 testRunner.And("an agent has a shift with", ((string)(null)), table9);
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
#line 181
 testRunner.And("I have a updated workflow control set with", ((string)(null)), table10);
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
#line 185
 testRunner.And("an agent has a updated workflow control set with", ((string)(null)), table11);
#line 190
 testRunner.When("I navigate to shift trade page");
#line 191
 testRunner.Then("I should have one possible shift to trade with");
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
#line 194
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 195
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 196
 testRunner.And("I have an existing shift trade request");
#line 197
 testRunner.And("I am viewing requests");
#line 198
 testRunner.When("I click on the request");
#line 199
 testRunner.Then("I should see the shift trade request form");
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
#line 202
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 203
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 204
 testRunner.And("I have created a shift trade request");
#line 205
 testRunner.And("I am viewing requests");
#line 206
 testRunner.When("I click on the request");
#line 207
 testRunner.And("I click the Approve button on the shift request");
#line 208
 testRunner.Then("I should not see the shift trade request in the list");
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
#line 211
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 212
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 213
 testRunner.And("I have received a shift trade request");
#line 214
 testRunner.And("I am viewing requests");
#line 215
 testRunner.When("I click on the request");
#line 216
 testRunner.And("I click the Deny button on the shift request");
#line 217
 testRunner.Then("I should not see the shift trade request in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Delete created shift trade request")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void DeleteCreatedShiftTradeRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Delete created shift trade request", new string[] {
                        "ignore"});
#line 220
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 221
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 222
 testRunner.And("I have created a shift trade request");
#line 223
 testRunner.And("I am viewing requests");
#line 224
 testRunner.And("I click the shift trade request\'s delete button");
#line 225
 testRunner.Then("I should not see the shift trade request in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Should not be able to delete shift trade requests from others")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void ShouldNotBeAbleToDeleteShiftTradeRequestsFromOthers()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Should not be able to delete shift trade requests from others", new string[] {
                        "ignore"});
#line 228
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 229
 testRunner.Given("I am an agent");
#line 230
 testRunner.And("I have received a shift trade request");
#line 231
 testRunner.And("I am viewing requests");
#line 232
 testRunner.Then("I should not see a deletebutton on the request");
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
#line 235
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 236
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 237
 testRunner.And("Current time is \'2012-01-14\'");
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
#line 238
 testRunner.And("I have a shift with", ((string)(null)), table12);
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
#line 243
 testRunner.And("an agent has a shift with", ((string)(null)), table13);
#line 249
 testRunner.And("I have an existing shift trade request for \'2012-01-15\' with \'Other agent 1\'");
#line 250
 testRunner.And("I am viewing requests");
#line 251
 testRunner.When("I click on the request");
#line 252
 testRunner.And("I click the Approve button on the shift request");
#line 253
 testRunner.And("I navigate to week schedule page for date \'2012-01-15\'");
#line 254
 testRunner.When("I view my week schedule for date \'2012-01-15\'");
#line hidden
            TechTalk.SpecFlow.Table table14 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table14.AddRow(new string[] {
                        "First activity times",
                        "11:00 - 16:00"});
#line 255
 testRunner.Then("I should see activities on date \'2012-01-15\' with:", ((string)(null)), table14);
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
