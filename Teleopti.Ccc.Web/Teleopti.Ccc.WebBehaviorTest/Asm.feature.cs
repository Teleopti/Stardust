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
                        "Published schedule"});
            table2.AddRow(new string[] {
                        "Schedule published to date",
                        "2040-06-24"});
#line 11
  testRunner.And("I have a workflow control set with", ((string)(null)), table2);
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table3.AddRow(new string[] {
                        "Start date",
                        "2012-06-18"});
            table3.AddRow(new string[] {
                        "Type",
                        "Week"});
            table3.AddRow(new string[] {
                        "Length",
                        "1"});
#line 15
 testRunner.And("I have a schedule period with", ((string)(null)), table3);
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table4.AddRow(new string[] {
                        "Start date",
                        "2012-06-18"});
#line 20
 testRunner.And("I have a person period with", ((string)(null)), table4);
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table5.AddRow(new string[] {
                        "StartTime",
                        "2030-01-01 08:00"});
            table5.AddRow(new string[] {
                        "EndTime",
                        "2030-01-01 17:00"});
            table5.AddRow(new string[] {
                        "ShiftCategoryName",
                        "ForTest"});
#line 23
 testRunner.And("there is a shift with", ((string)(null)), table5);
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("No permission to ASM module")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void NoPermissionToASMModule()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("No permission to ASM module", new string[] {
                        "ignore"});
#line 30
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 31
 testRunner.Given("I have the role \'No access to ASM\'");
#line 32
 testRunner.When("I am viewing week schedule");
#line 33
 testRunner.Then("ASM link should not be visible");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show part of agent\'s schedule in popup")]
        public virtual void ShowPartOfAgentSScheduleInPopup()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show part of agent\'s schedule in popup", ((string[])(null)));
#line 35
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 36
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 37
 testRunner.And("Current time is \'2030-01-01\'");
#line 38
 testRunner.When("I view my week schedule");
#line 39
 testRunner.And("I click ASM link");
#line 40
 testRunner.Then("I should see a schedule in popup");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Write name of current activity")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void WriteNameOfCurrentActivity()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Write name of current activity", new string[] {
                        "ignore"});
#line 43
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 44
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 45
 testRunner.And("Current time is \'2030-01-01 10:00\'");
#line 46
 testRunner.When("I view my regional settings");
#line 47
 testRunner.And("I click ASM link");
#line 48
 testRunner.Then("I should see \'Lunch\' as current activity");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
