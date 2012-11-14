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
    [NUnit.Framework.DescriptionAttribute("MobileReports")]
    public partial class MobileReportsFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "MobileReports.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "MobileReports", "In order to keep track on my CC\r\nAs a Supervisor on the move\r\nI want to see repor" +
                    "ts on my mobile", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        "Access to mobile reports"});
            table1.AddRow(new string[] {
                        "Access to mobile reports",
                        "true"});
#line 7
 testRunner.Given("there is a role with", ((string)(null)), table1);
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table2.AddRow(new string[] {
                        "Name",
                        "No access to mobile reports"});
            table2.AddRow(new string[] {
                        "Access to mobile reports",
                        "false"});
#line 11
 testRunner.And("there is a role with", ((string)(null)), table2);
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Enter Application")]
        public virtual void EnterApplication()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Enter Application", ((string[])(null)));
#line 16
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 17
 testRunner.Given("I have the role \'Access to mobile reports\'");
#line 18
 testRunner.When("I view MobileReports");
#line 19
 testRunner.Then("I should see ReportSettings");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Default report settings")]
        public virtual void DefaultReportSettings()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Default report settings", ((string[])(null)));
#line 21
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 22
 testRunner.Given("I have the role \'Access to mobile reports\'");
#line 23
 testRunner.When("I view ReportSettings");
#line 24
 testRunner.Then("I should see ReportSettings with default value");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Enter Application without permission")]
        public virtual void EnterApplicationWithoutPermission()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Enter Application without permission", ((string[])(null)));
#line 26
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 27
 testRunner.Given("I have the role \'No access to mobile reports\'");
#line 28
 testRunner.When("I view MobileReports");
#line 29
 testRunner.Then("I should see friendly error message");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sign out from application")]
        public virtual void SignOutFromApplication()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sign out from application", ((string[])(null)));
#line 31
 this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 32
  testRunner.Given("I have the role \'Access to mobile reports\'");
#line 33
  testRunner.And("I view MobileReports");
#line 34
  testRunner.When("I click the signout button");
#line 35
  testRunner.Then("I should be signed out from MobileReports");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View Report")]
        public virtual void ViewReport()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View Report", ((string[])(null)));
#line 37
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 38
 testRunner.Given("I have the role \'Access to mobile reports\'");
#line 39
 testRunner.And("I have analytics data for today");
#line 40
 testRunner.And("I have analytics fact queue data");
#line 41
 testRunner.When("I view ReportSettings");
#line 42
 testRunner.And("I select a report");
#line 43
 testRunner.And("I select date today");
#line 44
 testRunner.And("I check type Graph");
#line 45
 testRunner.And("I check type Table");
#line 46
 testRunner.And("I click View Report Button");
#line 47
 testRunner.Then("I should see a report");
#line 48
 testRunner.And("I should see a graph");
#line 49
 testRunner.And("I should see a table");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Select date in date-picker")]
        public virtual void SelectDateInDate_Picker()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Select date in date-picker", ((string[])(null)));
#line 51
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 52
 testRunner.Given("I have the role \'Access to mobile reports\'");
#line 53
 testRunner.When("I view ReportSettings");
#line 54
 testRunner.And("I open the date-picker");
#line 55
 testRunner.And("I click on any date");
#line 56
 testRunner.Then("the date-picker should close");
#line 57
 testRunner.And("I should see the selected date");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Select skill in skill-picker")]
        public virtual void SelectSkillInSkill_Picker()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Select skill in skill-picker", ((string[])(null)));
#line 59
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 60
 testRunner.Given("I have the role \'Access to mobile reports\'");
#line 61
 testRunner.And("I have analytics data for today");
#line 62
 testRunner.And("I have skill analytics data");
#line 63
 testRunner.When("I view ReportSettings");
#line 64
 testRunner.And("I open the skill-picker");
#line 65
 testRunner.And("I select a skill");
#line 66
 testRunner.And("I close the skill-picker");
#line 67
 testRunner.Then("I should see the selected skill");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Select all skills item in skill-picker")]
        public virtual void SelectAllSkillsItemInSkill_Picker()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Select all skills item in skill-picker", ((string[])(null)));
#line 69
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 70
 testRunner.Given("I have the role \'Access to mobile reports\'");
#line 71
 testRunner.When("I view ReportSettings");
#line 72
 testRunner.And("I open the skill-picker");
#line 73
 testRunner.And("I select the all skills item");
#line 74
 testRunner.And("I close the skill-picker");
#line 75
 testRunner.Then("I should see the all skill item selected");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Navigate within report view to previous day")]
        public virtual void NavigateWithinReportViewToPreviousDay()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Navigate within report view to previous day", ((string[])(null)));
#line 77
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 78
 testRunner.Given("I have the role \'Access to mobile reports\'");
#line 79
 testRunner.And("I have analytics data for the current week");
#line 80
 testRunner.And("I am viewing a report");
#line 81
 testRunner.When("I click previous date");
#line 82
 testRunner.Then("I should see a report for previous date");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Navigate within report view to next day")]
        public virtual void NavigateWithinReportViewToNextDay()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Navigate within report view to next day", ((string[])(null)));
#line 84
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 85
 testRunner.Given("I have the role \'Access to mobile reports\'");
#line 86
 testRunner.And("I have analytics data for the current week");
#line 87
 testRunner.And("I am viewing a report");
#line 88
 testRunner.When("I click next date");
#line 89
 testRunner.Then("I should see a report for next date");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Enter Application with partial access to reports")]
        public virtual void EnterApplicationWithPartialAccessToReports()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Enter Application with partial access to reports", ((string[])(null)));
#line 91
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 92
 testRunner.Given("I am user with partial access to reports");
#line 93
 testRunner.When("I view ReportSettings");
#line 94
 testRunner.Then("I should only see reports i have access to");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Tabledata shows sunday as first day of week for US culture")]
        public virtual void TabledataShowsSundayAsFirstDayOfWeekForUSCulture()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Tabledata shows sunday as first day of week for US culture", ((string[])(null)));
#line 96
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 97
 testRunner.Given("I have the role \'Access to mobile reports\'");
#line 98
 testRunner.And("I am american");
#line 99
 testRunner.And("I have analytics data for the current week");
#line 100
 testRunner.And("I have analytics fact queue data");
#line 101
 testRunner.When("I view a report with week data");
#line 102
 testRunner.Then("I should see sunday as the first day of week in tabledata");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
