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
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table1.AddRow(new string[] {
                        "StartTime",
                        "2012-08-27 20:00"});
            table1.AddRow(new string[] {
                        "EndTime",
                        "2012-08-28 04:00"});
            table1.AddRow(new string[] {
                        "ShiftCategoryName",
                        "ForTest"});
            table1.AddRow(new string[] {
                        "Lunch",
                        "true"});
#line 14
 testRunner.And("there is a shift with", ((string)(null)), table1);
#line 20
 testRunner.And("My schedule is published");
#line 21
 testRunner.When("I view my week schedule for date \'2012-08-27\'");
#line 22
 testRunner.Then("I should not see the end of the shift on date \'2012-08-27\'");
#line 23
 testRunner.And("I should see the end of the shift on date \'2012-08-28\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View start of night shift on last day of week for swedish culture")]
        public virtual void ViewStartOfNightShiftOnLastDayOfWeekForSwedishCulture()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View start of night shift on last day of week for swedish culture", ((string[])(null)));
#line 25
this.ScenarioSetup(scenarioInfo);
#line 26
 testRunner.Given("I am an agent");
#line 27
 testRunner.And("I am swedish");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table2.AddRow(new string[] {
                        "StartTime",
                        "2012-08-26 20:00"});
            table2.AddRow(new string[] {
                        "EndTime",
                        "2012-08-27 04:00"});
            table2.AddRow(new string[] {
                        "ShiftCategoryName",
                        "ForTest"});
            table2.AddRow(new string[] {
                        "Lunch",
                        "true"});
#line 28
 testRunner.And("there is a shift with", ((string)(null)), table2);
#line 34
 testRunner.And("My schedule is published");
#line 35
 testRunner.When("I view my week schedule for date \'2012-08-26\'");
#line 36
 testRunner.Then("I should see the start of the shift on date \'2012-08-26\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View end of night shift from previuos week for swedish culture")]
        public virtual void ViewEndOfNightShiftFromPreviuosWeekForSwedishCulture()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View end of night shift from previuos week for swedish culture", ((string[])(null)));
#line 38
this.ScenarioSetup(scenarioInfo);
#line 39
 testRunner.Given("I am an agent");
#line 40
 testRunner.And("I am swedish");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table3.AddRow(new string[] {
                        "StartTime",
                        "2012-08-26 20:00"});
            table3.AddRow(new string[] {
                        "EndTime",
                        "2012-08-27 04:00"});
            table3.AddRow(new string[] {
                        "ShiftCategoryName",
                        "ForTest"});
            table3.AddRow(new string[] {
                        "Lunch",
                        "true"});
#line 41
 testRunner.And("there is a shift with", ((string)(null)), table3);
#line 47
 testRunner.And("My schedule is published");
#line 48
 testRunner.When("I view my week schedule for date \'2012-08-27\'");
#line 49
 testRunner.Then("I should see the end of the shift on date \'2012-08-27\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Do not show unpublished schedule")]
        public virtual void DoNotShowUnpublishedSchedule()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Do not show unpublished schedule", ((string[])(null)));
#line 51
this.ScenarioSetup(scenarioInfo);
#line 52
 testRunner.Given("I am an agent");
#line 53
 testRunner.And("I have shifts scheduled for two weeks");
#line 54
 testRunner.And("My schedule is not published");
#line 55
 testRunner.When("I view my week schedule");
#line 56
 testRunner.Then("I should not see any shifts");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Do not show unpublished schedule for part of week")]
        public virtual void DoNotShowUnpublishedScheduleForPartOfWeek()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Do not show unpublished schedule for part of week", ((string[])(null)));
#line 58
this.ScenarioSetup(scenarioInfo);
#line 59
 testRunner.Given("I am an agent");
#line 60
 testRunner.And("I have shifts scheduled for two weeks");
#line 61
 testRunner.And("My schedule is published until wednesday");
#line 62
 testRunner.When("I view my week schedule");
#line 63
 testRunner.Then("I should not see any shifts after wednesday");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View meeting")]
        public virtual void ViewMeeting()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View meeting", ((string[])(null)));
#line 65
this.ScenarioSetup(scenarioInfo);
#line 66
 testRunner.Given("I am an agent");
#line 68
 testRunner.And("I have a shift on thursday");
#line 69
 testRunner.And("I have a meeting scheduled on thursday");
#line 70
 testRunner.And("My schedule is published");
#line 71
 testRunner.When("I view my week schedule");
#line 72
 testRunner.And("I click on the meeting");
#line 73
 testRunner.Then("I should see the meeting details");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View public note")]
        public virtual void ViewPublicNote()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View public note", ((string[])(null)));
#line 75
this.ScenarioSetup(scenarioInfo);
#line 76
 testRunner.Given("I am an agent");
#line 77
 testRunner.And("I have a public note on tuesday");
#line 78
 testRunner.And("My schedule is published");
#line 79
 testRunner.When("I view my week schedule");
#line 80
 testRunner.Then("I should see the public note on tuesday");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Select week from week-picker")]
        public virtual void SelectWeekFromWeek_Picker()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Select week from week-picker", ((string[])(null)));
#line 82
this.ScenarioSetup(scenarioInfo);
#line 83
 testRunner.Given("I am an agent");
#line 84
 testRunner.And("My schedule is published");
#line 85
 testRunner.And("I view my week schedule");
#line 86
 testRunner.When("I open the week-picker");
#line 87
 testRunner.And("I click on any day of a week");
#line 88
 testRunner.Then("the week-picker should close");
#line 89
 testRunner.And("I should see the selected week");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Week-picker monday first day of week for swedish culture")]
        public virtual void Week_PickerMondayFirstDayOfWeekForSwedishCulture()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Week-picker monday first day of week for swedish culture", ((string[])(null)));
#line 91
this.ScenarioSetup(scenarioInfo);
#line 92
 testRunner.Given("I am an agent");
#line 93
 testRunner.And("I am swedish");
#line 94
 testRunner.And("I view my week schedule");
#line 95
 testRunner.When("I open the week-picker");
#line 96
 testRunner.Then("I should see monday as the first day of week");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Week-picker sunday first day of week for US culture")]
        public virtual void Week_PickerSundayFirstDayOfWeekForUSCulture()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Week-picker sunday first day of week for US culture", ((string[])(null)));
#line 98
this.ScenarioSetup(scenarioInfo);
#line 99
 testRunner.Given("I am an agent");
#line 100
 testRunner.And("I am american");
#line 101
 testRunner.And("I view my week schedule");
#line 102
 testRunner.When("I open the week-picker");
#line 103
 testRunner.Then("I should see sunday as the first day of week");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show text request symbol")]
        public virtual void ShowTextRequestSymbol()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show text request symbol", ((string[])(null)));
#line 105
this.ScenarioSetup(scenarioInfo);
#line 106
 testRunner.Given("I am an agent");
#line 107
 testRunner.And("I have an existing text request");
#line 108
 testRunner.When("I view my week schedule");
#line 109
 testRunner.Then("I should see a symbol at the top of the schedule");
#line 110
 testRunner.And("I should see a number with the request count");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Multiple day text requests symbol")]
        public virtual void MultipleDayTextRequestsSymbol()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Multiple day text requests symbol", ((string[])(null)));
#line 112
this.ScenarioSetup(scenarioInfo);
#line 113
 testRunner.Given("I am an agent");
#line 114
 testRunner.And("I have an existing text request spanning over 2 days");
#line 115
 testRunner.When("I view my week schedule");
#line 116
 testRunner.Then("I should see a symbol at the top of the schedule for the first day");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show both text and absence requests")]
        public virtual void ShowBothTextAndAbsenceRequests()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show both text and absence requests", ((string[])(null)));
#line 118
this.ScenarioSetup(scenarioInfo);
#line 119
 testRunner.Given("I am an agent");
#line 120
 testRunner.And("I have an existing text request");
#line 121
 testRunner.And("I have an existing absence request");
#line 122
 testRunner.When("I view my week schedule");
#line 123
 testRunner.Then("I should see 2 with the request count");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Navigate to request page by clicking request symbol")]
        public virtual void NavigateToRequestPageByClickingRequestSymbol()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Navigate to request page by clicking request symbol", ((string[])(null)));
#line 125
this.ScenarioSetup(scenarioInfo);
#line 126
 testRunner.Given("I am an agent");
#line 127
 testRunner.And("I have an existing text request");
#line 128
 testRunner.When("I view my week schedule");
#line 129
 testRunner.And("I click the request symbol");
#line 130
 testRunner.Then("I should see request page");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Navigate to current week")]
        public virtual void NavigateToCurrentWeek()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Navigate to current week", ((string[])(null)));
#line 132
this.ScenarioSetup(scenarioInfo);
#line 133
 testRunner.Given("I am an agent");
#line 134
 testRunner.And("I view my week schedule one month ago");
#line 135
 testRunner.When("I click the current week button");
#line 136
 testRunner.Then("I should see the start and end dates for current week");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show timeline with no schedule")]
        public virtual void ShowTimelineWithNoSchedule()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show timeline with no schedule", ((string[])(null)));
#line 138
this.ScenarioSetup(scenarioInfo);
#line 139
 testRunner.Given("I am an agent");
#line 140
 testRunner.When("I view my week schedule");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table4.AddRow(new string[] {
                        "start timeline",
                        "0:00"});
            table4.AddRow(new string[] {
                        "end timeline",
                        "23:59"});
            table4.AddRow(new string[] {
                        "timeline count",
                        "25"});
#line 141
 testRunner.Then("I should see start timeline and end timeline according to schedule with:", ((string)(null)), table4);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show timeline with schedule")]
        public virtual void ShowTimelineWithSchedule()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show timeline with schedule", ((string[])(null)));
#line 147
this.ScenarioSetup(scenarioInfo);
#line 148
 testRunner.Given("I am an agent");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table5.AddRow(new string[] {
                        "StartTime",
                        "2012-08-27 10:00"});
            table5.AddRow(new string[] {
                        "EndTime",
                        "2012-08-27 20:00"});
            table5.AddRow(new string[] {
                        "ShiftCategoryName",
                        "ForTest"});
            table5.AddRow(new string[] {
                        "Lunch",
                        "true"});
#line 149
 testRunner.And("there is a shift with", ((string)(null)), table5);
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table6.AddRow(new string[] {
                        "StartTime",
                        "2012-08-28 08:00"});
            table6.AddRow(new string[] {
                        "EndTime",
                        "2012-08-28 17:00"});
            table6.AddRow(new string[] {
                        "ShiftCategoryName",
                        "ForTest"});
            table6.AddRow(new string[] {
                        "Lunch",
                        "true"});
#line 155
 testRunner.And("there is a shift with", ((string)(null)), table6);
#line 161
 testRunner.And("My schedule is published");
#line 162
 testRunner.When("I view my week schedule");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table7.AddRow(new string[] {
                        "start timeline",
                        "8:00"});
            table7.AddRow(new string[] {
                        "end timeline",
                        "20:00"});
            table7.AddRow(new string[] {
                        "timeline count",
                        "13"});
#line 163
 testRunner.Then("I should see start timeline and end timeline according to schedule with:", ((string)(null)), table7);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show timeline with night shift")]
        public virtual void ShowTimelineWithNightShift()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show timeline with night shift", ((string[])(null)));
#line 169
this.ScenarioSetup(scenarioInfo);
#line 170
 testRunner.Given("I am an agent");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table8.AddRow(new string[] {
                        "StartTime",
                        "2012-08-27 20:00"});
            table8.AddRow(new string[] {
                        "EndTime",
                        "2012-08-28 04:00"});
            table8.AddRow(new string[] {
                        "ShiftCategoryName",
                        "ForTest"});
            table8.AddRow(new string[] {
                        "Lunch",
                        "true"});
#line 171
 testRunner.And("there is a shift with", ((string)(null)), table8);
#line 177
 testRunner.And("My schedule is published");
#line 178
 testRunner.When("I view my week schedule for date \'2012-08-27\'");
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table9.AddRow(new string[] {
                        "start timeline",
                        "0:00"});
            table9.AddRow(new string[] {
                        "end timeline",
                        "23:59"});
            table9.AddRow(new string[] {
                        "timeline count",
                        "25"});
#line 179
 testRunner.Then("I should see start timeline and end timeline according to schedule with:", ((string)(null)), table9);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show timeline with night shift from the last day of the previous week")]
        public virtual void ShowTimelineWithNightShiftFromTheLastDayOfThePreviousWeek()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show timeline with night shift from the last day of the previous week", ((string[])(null)));
#line 185
this.ScenarioSetup(scenarioInfo);
#line 186
 testRunner.Given("I am an agent");
#line hidden
            TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table10.AddRow(new string[] {
                        "StartTime",
                        "2012-08-26 20:00"});
            table10.AddRow(new string[] {
                        "EndTime",
                        "2012-08-27 04:00"});
            table10.AddRow(new string[] {
                        "ShiftCategoryName",
                        "ForTest"});
            table10.AddRow(new string[] {
                        "Lunch",
                        "true"});
#line 187
 testRunner.And("there is a shift with", ((string)(null)), table10);
#line 193
 testRunner.And("My schedule is published");
#line 194
 testRunner.When("I view my week schedule for date \'2012-08-27\'");
#line hidden
            TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table11.AddRow(new string[] {
                        "start timeline",
                        "0:00"});
            table11.AddRow(new string[] {
                        "end timeline",
                        "4:00"});
            table11.AddRow(new string[] {
                        "timeline count",
                        "5"});
#line 195
 testRunner.Then("I should see start timeline and end timeline according to schedule with:", ((string)(null)), table11);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show timeline with night shift starting on the last day of current week")]
        public virtual void ShowTimelineWithNightShiftStartingOnTheLastDayOfCurrentWeek()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show timeline with night shift starting on the last day of current week", ((string[])(null)));
#line 201
this.ScenarioSetup(scenarioInfo);
#line 202
 testRunner.Given("I am an agent");
#line hidden
            TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table12.AddRow(new string[] {
                        "StartTime",
                        "2012-08-26 20:00"});
            table12.AddRow(new string[] {
                        "EndTime",
                        "2012-08-27 04:00"});
            table12.AddRow(new string[] {
                        "ShiftCategoryName",
                        "ForTest"});
            table12.AddRow(new string[] {
                        "Lunch",
                        "true"});
#line 203
 testRunner.And("there is a shift with", ((string)(null)), table12);
#line 209
 testRunner.And("My schedule is published");
#line 210
 testRunner.When("I view my week schedule for date \'2012-08-26\'");
#line hidden
            TechTalk.SpecFlow.Table table13 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table13.AddRow(new string[] {
                        "start timeline",
                        "20:00"});
            table13.AddRow(new string[] {
                        "end timeline",
                        "23:59"});
            table13.AddRow(new string[] {
                        "timeline count",
                        "5"});
#line 211
 testRunner.Then("I should see start timeline and end timeline according to schedule with:", ((string)(null)), table13);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show activity with correct position, height and color")]
        public virtual void ShowActivityWithCorrectPositionHeightAndColor()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show activity with correct position, height and color", ((string[])(null)));
#line 217
this.ScenarioSetup(scenarioInfo);
#line 218
 testRunner.Given("I am an agent");
#line hidden
            TechTalk.SpecFlow.Table table14 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table14.AddRow(new string[] {
                        "Phone",
                        "09:00-10:30"});
            table14.AddRow(new string[] {
                        "Shortbreak",
                        "10:30-11:00"});
            table14.AddRow(new string[] {
                        "Phone",
                        "11:00-12:00"});
            table14.AddRow(new string[] {
                        "Lunch",
                        "12:00-14:00"});
            table14.AddRow(new string[] {
                        "Phone",
                        "14:00-18:00"});
#line 219
 testRunner.And("I have custom shifts scheduled on wednesday for two weeks:", ((string)(null)), table14);
#line 226
 testRunner.And("My schedule is published");
#line 227
 testRunner.When("I view my week schedule");
#line hidden
            TechTalk.SpecFlow.Table table15 = new TechTalk.SpecFlow.Table(new string[] {
                        "Activity",
                        "Start Position",
                        "Height",
                        "Color"});
            table15.AddRow(new string[] {
                        "Phone",
                        "67",
                        "99px",
                        "Green"});
            table15.AddRow(new string[] {
                        "Shortbreak",
                        "167",
                        "32px",
                        "Red"});
#line 228
 testRunner.Then("I should see wednesday\'s activities:", ((string)(null)), table15);
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
