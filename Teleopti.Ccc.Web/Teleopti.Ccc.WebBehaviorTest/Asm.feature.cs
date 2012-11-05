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
    [NUnit.Framework.DescriptionAttribute("ASM")]
    public partial class ASMFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Asm.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "ASM", "In order to improve adherence\r\nAs an agent\r\nI want to see my current activities", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        "False"});
#line 11
 testRunner.And("there is a role with", ((string)(null)), table2);
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table3.AddRow(new string[] {
                        "Name",
                        "Published schedule"});
            table3.AddRow(new string[] {
                        "Schedule published to date",
                        "2040-06-24"});
#line 15
 testRunner.And("I have a workflow control set with", ((string)(null)), table3);
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table4.AddRow(new string[] {
                        "Start date",
                        "2012-06-18"});
            table4.AddRow(new string[] {
                        "Type",
                        "Week"});
            table4.AddRow(new string[] {
                        "Length",
                        "1"});
#line 19
 testRunner.And("I have a schedule period with", ((string)(null)), table4);
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table5.AddRow(new string[] {
                        "Start date",
                        "2012-06-18"});
#line 24
 testRunner.And("I have a person period with", ((string)(null)), table5);
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table6.AddRow(new string[] {
                        "StartTime",
                        "2030-01-01 08:00"});
            table6.AddRow(new string[] {
                        "EndTime",
                        "2030-01-01 17:00"});
            table6.AddRow(new string[] {
                        "Lunch3HoursAfterStart",
                        "true"});
#line 27
 testRunner.And("there is a shift with", ((string)(null)), table6);
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("No permission to ASM module")]
        public virtual void NoPermissionToASMModule()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("No permission to ASM module", ((string[])(null)));
#line 34
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 35
 testRunner.Given("I have the role \'No access to ASM\'");
#line 36
 testRunner.When("I am viewing week schedule");
#line 37
 testRunner.Then("ASM link should not be visible");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show part of agent\'s schedule in popup")]
        public virtual void ShowPartOfAgentSScheduleInPopup()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show part of agent\'s schedule in popup", ((string[])(null)));
#line 39
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 40
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 41
 testRunner.And("Current time is \'2030-01-01\'");
#line 42
 testRunner.When("I click ASM link");
#line 43
 testRunner.Then("I should see a schedule in popup");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show title in popup")]
        public virtual void ShowTitleInPopup()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show title in popup", ((string[])(null)));
#line 45
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 46
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 47
 testRunner.When("I click ASM link");
#line 48
 testRunner.Then("I should see a popup with title AgentScheduleMessenger");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Current activity should be shown")]
        public virtual void CurrentActivityShouldBeShown()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Current activity should be shown", ((string[])(null)));
#line 50
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 51
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 52
 testRunner.And("Current time is \'2030-01-01 16:00\'");
#line 53
 testRunner.When("I click ASM link");
#line 54
 testRunner.Then("I should see Phone as current activity");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("No current activity to show")]
        public virtual void NoCurrentActivityToShow()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("No current activity to show", ((string[])(null)));
#line 56
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 57
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 58
 testRunner.And("Current time is \'2030-01-01 07:00\'");
#line 59
 testRunner.When("I click ASM link");
#line 60
 testRunner.Then("I should not see a current activity");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Current activity changes")]
        public virtual void CurrentActivityChanges()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Current activity changes", ((string[])(null)));
#line 62
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 63
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 64
 testRunner.And("Current time is \'2030-01-01 11:59\'");
#line 65
 testRunner.When("I click ASM link");
#line 66
 testRunner.And("Current browser time has changed to \'2030-01-01 12:00\'");
#line 67
 testRunner.Then("I should see Phone as current activity");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Upcoming activity time period should be displayed")]
        public virtual void UpcomingActivityTimePeriodShouldBeDisplayed()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Upcoming activity time period should be displayed", ((string[])(null)));
#line 69
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 70
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 71
 testRunner.And("Current time is \'2030-01-01 00:01\'");
#line 72
 testRunner.When("I click ASM link");
#line 73
 testRunner.Then("I should see next activity time as \'08:00-11:00\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Upcoming activity time period starting after midnight should be indicated as next" +
            " day")]
        public virtual void UpcomingActivityTimePeriodStartingAfterMidnightShouldBeIndicatedAsNextDay()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Upcoming activity time period starting after midnight should be indicated as next" +
                    " day", ((string[])(null)));
#line 75
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 76
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 77
 testRunner.And("Current time is \'2029-12-31 23:59\'");
#line 78
 testRunner.When("I click ASM link");
#line 79
 testRunner.Then("I should see next activity time as \'08:00+1-11:00\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Agent should from ASM popup be notified when current shift has changed")]
        public virtual void AgentShouldFromASMPopupBeNotifiedWhenCurrentShiftHasChanged()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Agent should from ASM popup be notified when current shift has changed", ((string[])(null)));
#line 81
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 82
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 83
 testRunner.And("Current time is \'2030-01-01 00:00\'");
#line 84
 testRunner.When("I click ASM link");
#line 85
 testRunner.And("My schedule between \'2030-01-01 08:00\' to \'2030-01-01 17:00\' change");
#line 86
 testRunner.Then("I should see one notify message");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Agent should from portal be notified when current shift has changed")]
        public virtual void AgentShouldFromPortalBeNotifiedWhenCurrentShiftHasChanged()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Agent should from portal be notified when current shift has changed", ((string[])(null)));
#line 88
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 89
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 90
 testRunner.And("Current time is \'2030-01-01 00:00\'");
#line 91
 testRunner.When("I view preferences");
#line 92
 testRunner.And("My schedule between \'2030-01-01 08:00\' to \'2030-01-01 17:00\' change");
#line 93
 testRunner.Then("I should see one notify message");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Asm should be automatically reloaded when time passes")]
        public virtual void AsmShouldBeAutomaticallyReloadedWhenTimePasses()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Asm should be automatically reloaded when time passes", ((string[])(null)));
#line 95
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 96
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 97
 testRunner.And("Current time is \'2030-01-01 23:59\'");
#line 98
 testRunner.When("I click ASM link");
#line 99
 testRunner.Then("Now indicator should be at hour \'47\'");
#line 100
 testRunner.When("Current browser time has changed to \'2030-01-02 00:01\'");
#line 101
 testRunner.Then("Now indicator should be at hour \'24\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Asm should not indicate unread messages if no messages")]
        public virtual void AsmShouldNotIndicateUnreadMessagesIfNoMessages()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Asm should not indicate unread messages if no messages", ((string[])(null)));
#line 103
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 104
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 105
 testRunner.When("I click ASM link");
#line 106
 testRunner.Then("I shoud not see an indication that I have an unread message");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Asm should indicate unread messages")]
        public virtual void AsmShouldIndicateUnreadMessages()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Asm should indicate unread messages", ((string[])(null)));
#line 108
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 109
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table7.AddRow(new string[] {
                        "Title",
                        "New message"});
#line 110
 testRunner.And("I have an unread message with", ((string)(null)), table7);
#line 113
 testRunner.When("I click ASM link");
#line 114
 testRunner.Then("I shoud see an indication that I have an unread message");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Asm should update when I get new messages")]
        public virtual void AsmShouldUpdateWhenIGetNewMessages()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Asm should update when I get new messages", ((string[])(null)));
#line 116
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 117
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table8.AddRow(new string[] {
                        "Title",
                        "New message"});
#line 118
 testRunner.And("I have an unread message with", ((string)(null)), table8);
#line 121
 testRunner.When("I click ASM link");
#line 122
 testRunner.And("I recieve a new message");
#line 123
 testRunner.Then("I shoud see an indication that I have \'2\' unread messages");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Open messages when I click on unread messages")]
        public virtual void OpenMessagesWhenIClickOnUnreadMessages()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Open messages when I click on unread messages", ((string[])(null)));
#line 125
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 126
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table9.AddRow(new string[] {
                        "Title",
                        "New message"});
#line 127
 testRunner.And("I have an unread message with", ((string)(null)), table9);
#line 130
 testRunner.When("I click ASM link");
#line 131
 testRunner.And("I click the unread message");
#line 132
 testRunner.Then("I should see a window showing messages");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Agent should be notified when activity changes")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void AgentShouldBeNotifiedWhenActivityChanges()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Agent should be notified when activity changes", new string[] {
                        "ignore"});
#line 136
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 137
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 138
 testRunner.And("Current time is \'2030-01-01 11:59\'");
#line 139
 testRunner.When("I click ASM link");
#line 140
 testRunner.And("Current browser time has changed to \'2030-01-01 12:00\'");
#line 141
 testRunner.Then("I should see only one alert containing \'Phone\'");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
