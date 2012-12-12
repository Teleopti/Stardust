﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.77
//      SpecFlow Generator Version:1.9.0.0
//      Runtime Version:4.0.30319.296
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Teleopti.Ccc.WebBehaviorTest.AdminWeb
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("View team schedule")]
    public partial class ViewTeamScheduleFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Schedule.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "View team schedule", "In order to know how my team should work today\r\nAs a team leader\r\nI want to see t" +
                    "he schedules for the team", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        "Full access to Admin web"});
#line 7
 testRunner.Given("there is a role with", ((string)(null)), table1, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table2.AddRow(new string[] {
                        "Name",
                        "Team green"});
#line 10
 testRunner.And("there is a team with", ((string)(null)), table2, "And ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table3.AddRow(new string[] {
                        "Name",
                        "Pierre Baldi"});
            table3.AddRow(new string[] {
                        "TerminalDate",
                        "2012-12-31"});
#line 13
 testRunner.Given("there is a team member with", ((string)(null)), table3, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table4.AddRow(new string[] {
                        "Team",
                        "Team green"});
            table4.AddRow(new string[] {
                        "StartDate",
                        "2012-12-01"});
#line 17
 testRunner.And("there is a person period for \'Pierre Baldi\' with", ((string)(null)), table4, "And ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table5.AddRow(new string[] {
                        "Name",
                        "Lunch"});
#line 21
 testRunner.And("there is an activity with", ((string)(null)), table5, "And ");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table6.AddRow(new string[] {
                        "Name",
                        "Phone"});
#line 24
 testRunner.And("there is an activity with", ((string)(null)), table6, "And ");
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View team schedule")]
        public virtual void ViewTeamSchedule()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View team schedule", ((string[])(null)));
#line 28
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 29
 testRunner.Given("I am a team leader for \'Team green\' with role \'Full access to Admin web\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table7.AddRow(new string[] {
                        "Person",
                        "Pierre Baldi"});
            table7.AddRow(new string[] {
                        "Date",
                        "2012-12-02"});
            table7.AddRow(new string[] {
                        "StartTime",
                        "08:00"});
            table7.AddRow(new string[] {
                        "EndTime",
                        "17:00"});
            table7.AddRow(new string[] {
                        "Activity",
                        "Phone"});
            table7.AddRow(new string[] {
                        "LunchStartTime",
                        "11:30"});
            table7.AddRow(new string[] {
                        "LunchEndTime",
                        "12:15"});
            table7.AddRow(new string[] {
                        "LunchActivity",
                        "Lunch"});
#line 30
 testRunner.And("there is a shift with", ((string)(null)), table7, "And ");
#line 40
 testRunner.When("I view schedules for \'2012-12-02\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 41
 testRunner.Then("I should see schedule for \'Pierre Baldi\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View only my team\'s schedule")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void ViewOnlyMyTeamSSchedule()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View only my team\'s schedule", new string[] {
                        "ignore"});
#line 44
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 45
 testRunner.Given("I am an agent in a team with access to the whole site", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 46
 testRunner.And("I have a shift today", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 47
 testRunner.And("I have a colleague", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 48
 testRunner.And("My colleague has a shift today", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 49
 testRunner.And("I have a colleague in another team", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 50
 testRunner.And("The colleague in the other team has a shift today", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 51
 testRunner.When("I view team schedule", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 52
 testRunner.Then("I should see my schedule", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 53
 testRunner.And("I should see my colleague\'s schedule", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 54
 testRunner.And("I should not see the other colleague\'s schedule", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View team schedule, day off")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void ViewTeamScheduleDayOff()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View team schedule, day off", new string[] {
                        "ignore"});
#line 57
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 58
 testRunner.Given("I am an agent in a team", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 59
 testRunner.And("I have a colleague", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 60
 testRunner.And("My colleague has a dayoff today", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 61
 testRunner.When("I view team schedule", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 62
 testRunner.Then("I should see my colleague\'s day off", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View team schedule, absence")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void ViewTeamScheduleAbsence()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View team schedule, absence", new string[] {
                        "ignore"});
#line 65
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 66
 testRunner.Given("I am an agent in a team", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 67
 testRunner.And("I have a colleague", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 68
 testRunner.And("My colleague has an absence today", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 69
 testRunner.When("I view team schedule", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 70
 testRunner.Then("I should see my colleague\'s absence", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View team schedule, no shift")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void ViewTeamScheduleNoShift()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View team schedule, no shift", new string[] {
                        "ignore"});
#line 73
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 74
 testRunner.Given("I am an agent in a team", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 75
 testRunner.And("I have a colleague", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 76
 testRunner.When("I view team schedule", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 77
 testRunner.Then("I should see myself without schedule", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 78
 testRunner.And("I should see my colleague without schedule", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can\'t see confidential absence")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void CanTSeeConfidentialAbsence()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can\'t see confidential absence", new string[] {
                        "ignore"});
#line 81
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 82
 testRunner.Given("I am an agent in a team", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 83
 testRunner.And("I have a colleague", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 84
 testRunner.And("My colleague has a confidential absence", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 85
 testRunner.When("I view team schedule", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 86
 testRunner.Then("I should see my colleague\'s schedule", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 87
 testRunner.And("I should not see the absence\'s color", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can\'t see the team schedule tab without permission")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void CanTSeeTheTeamScheduleTabWithoutPermission()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can\'t see the team schedule tab without permission", new string[] {
                        "ignore"});
#line 90
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 91
 testRunner.Given("I am an agent with no access to team schedule", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 92
 testRunner.When("I am viewing an application page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 93
 testRunner.Then("I should not see the team schedule tab", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can\'t navigate to team schedule without permission")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void CanTNavigateToTeamScheduleWithoutPermission()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can\'t navigate to team schedule without permission", new string[] {
                        "ignore"});
#line 96
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 97
 testRunner.Given("I am an agent with no access to team schedule", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 98
 testRunner.When("I am viewing an application page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 99
 testRunner.And("I navigate to the team schedule", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 100
 testRunner.Then("I should see an error message", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Can\'t see colleagues schedule without permission")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void CanTSeeColleaguesScheduleWithoutPermission()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Can\'t see colleagues schedule without permission", new string[] {
                        "ignore"});
#line 103
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 104
 testRunner.Given("I am an agent in a team with access only to my own data", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 105
 testRunner.And("I have a colleague", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 106
 testRunner.And("My colleague has a shift today", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 107
 testRunner.When("I view team schedule", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 108
 testRunner.Then("I should not see my colleagues schedule", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
