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
    [NUnit.Framework.DescriptionAttribute("Preferences must-haves")]
    public partial class PreferencesMust_HavesFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "PreferencesMustHave.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Preferences must-haves", "In order to get scheduled according specific preferences\r\nAs an agent\r\nI want to " +
                    "stress which of my preferences are most important", ProgrammingLanguage.CSharp, ((string[])(null)));
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
            table1.AddRow(new string[] {
                        "Access to extended preferences",
                        "false"});
#line 7
 testRunner.Given("I have a role with", ((string)(null)), table1);
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table2.AddRow(new string[] {
                        "Name",
                        "Late"});
#line 11
    testRunner.And("there is a shift category with", ((string)(null)), table2);
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table3.AddRow(new string[] {
                        "Name",
                        "Published schedule"});
            table3.AddRow(new string[] {
                        "Schedule published to date",
                        "2012-08-26"});
#line 14
    testRunner.And("I have a workflow control set with", ((string)(null)), table3);
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table4.AddRow(new string[] {
                        "Start date",
                        "2012-08-20"});
            table4.AddRow(new string[] {
                        "Type",
                        "Week"});
            table4.AddRow(new string[] {
                        "Length",
                        "1"});
            table4.AddRow(new string[] {
                        "Must have preference",
                        "1"});
#line 18
 testRunner.And("I have a schedule period with", ((string)(null)), table4);
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table5.AddRow(new string[] {
                        "Start date",
                        "2012-08-20"});
#line 24
 testRunner.And("I have a person period with", ((string)(null)), table5);
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("See must have preference")]
        public virtual void SeeMustHavePreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("See must have preference", ((string[])(null)));
#line 28
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table6.AddRow(new string[] {
                        "Date",
                        "2012-08-23"});
            table6.AddRow(new string[] {
                        "Must have",
                        "true"});
            table6.AddRow(new string[] {
                        "Shift category",
                        "Late"});
#line 29
 testRunner.Given("I have a preference with", ((string)(null)), table6);
#line 34
 testRunner.When("I view preferences for date \'2012-08-23\'");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table7.AddRow(new string[] {
                        "Date",
                        "2012-08-23"});
            table7.AddRow(new string[] {
                        "Must have",
                        "true"});
#line 35
 testRunner.Then("I should see preference", ((string)(null)), table7);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Set must have on preference")]
        public virtual void SetMustHaveOnPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Set must have on preference", ((string[])(null)));
#line 40
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table8.AddRow(new string[] {
                        "Date",
                        "2012-08-23"});
            table8.AddRow(new string[] {
                        "Must have",
                        "false"});
            table8.AddRow(new string[] {
                        "Shift category",
                        "Late"});
#line 41
 testRunner.Given("I have a preference with", ((string)(null)), table8);
#line 46
 testRunner.When("I view preferences for date \'2012-08-23\'");
#line 47
 testRunner.And("I select day \'2012-08-23\'");
#line 48
 testRunner.And("I click set must have button");
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table9.AddRow(new string[] {
                        "Date",
                        "2012-08-23"});
            table9.AddRow(new string[] {
                        "Must have",
                        "true"});
#line 49
 testRunner.Then("I should see preference", ((string)(null)), table9);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Set must have on empty day should do nothing")]
        public virtual void SetMustHaveOnEmptyDayShouldDoNothing()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Set must have on empty day should do nothing", ((string[])(null)));
#line 54
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 55
 testRunner.When("I view preferences for date \'2012-08-23\'");
#line 56
 testRunner.And("I select day \'2012-08-23\'");
#line 57
 testRunner.And("I click set must have button");
#line hidden
            TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table10.AddRow(new string[] {
                        "Date",
                        "2012-08-23"});
            table10.AddRow(new string[] {
                        "Must have",
                        "false"});
#line 58
 testRunner.Then("I should see preference", ((string)(null)), table10);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Remove must have from preference")]
        public virtual void RemoveMustHaveFromPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Remove must have from preference", ((string[])(null)));
#line 63
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table11.AddRow(new string[] {
                        "Date",
                        "2012-08-23"});
            table11.AddRow(new string[] {
                        "Must have",
                        "true"});
            table11.AddRow(new string[] {
                        "Shift category",
                        "Late"});
#line 64
 testRunner.Given("I have a preference with", ((string)(null)), table11);
#line 69
 testRunner.When("I view preferences for date \'2012-08-23\'");
#line 70
 testRunner.And("I select day \'2012-08-23\'");
#line 71
 testRunner.And("I click remove must have button");
#line hidden
            TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table12.AddRow(new string[] {
                        "Date",
                        "2012-08-23"});
            table12.AddRow(new string[] {
                        "Must have",
                        "false"});
#line 72
 testRunner.Then("I should see preference", ((string)(null)), table12);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("See available must haves")]
        public virtual void SeeAvailableMustHaves()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("See available must haves", ((string[])(null)));
#line 77
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 78
 testRunner.When("I view preferences for date \'2012-08-23\'");
#line 79
 testRunner.Then("I should see I have \'1\' available must haves");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Decrement available must haves on set")]
        public virtual void DecrementAvailableMustHavesOnSet()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Decrement available must haves on set", ((string[])(null)));
#line 81
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table13 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table13.AddRow(new string[] {
                        "Date",
                        "2012-08-23"});
            table13.AddRow(new string[] {
                        "Must have",
                        "false"});
            table13.AddRow(new string[] {
                        "Shift category",
                        "Late"});
#line 82
 testRunner.Given("I have a preference with", ((string)(null)), table13);
#line 87
 testRunner.When("I view preferences for date \'2012-08-23\'");
#line 88
 testRunner.And("I select day \'2012-08-23\'");
#line 89
 testRunner.And("I click set must have button");
#line 90
 testRunner.Then("I should see I have \'0\' available must haves");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Increment available must haves on remove")]
        public virtual void IncrementAvailableMustHavesOnRemove()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Increment available must haves on remove", ((string[])(null)));
#line 92
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table14 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table14.AddRow(new string[] {
                        "Date",
                        "2012-08-23"});
            table14.AddRow(new string[] {
                        "Must have",
                        "true"});
            table14.AddRow(new string[] {
                        "Shift category",
                        "Late"});
#line 93
 testRunner.Given("I have a preference with", ((string)(null)), table14);
#line 98
 testRunner.When("I view preferences for date \'2012-08-23\'");
#line 99
 testRunner.And("I select day \'2012-08-23\'");
#line 100
 testRunner.And("I click remove must have button");
#line 101
 testRunner.Then("I should see I have \'1\' available must haves");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Disallow setting too many must haves")]
        public virtual void DisallowSettingTooManyMustHaves()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Disallow setting too many must haves", ((string[])(null)));
#line 103
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table15 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table15.AddRow(new string[] {
                        "Date",
                        "2012-08-23"});
            table15.AddRow(new string[] {
                        "Must have",
                        "true"});
            table15.AddRow(new string[] {
                        "Shift category",
                        "Late"});
#line 104
 testRunner.Given("I have a preference with", ((string)(null)), table15);
#line hidden
            TechTalk.SpecFlow.Table table16 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table16.AddRow(new string[] {
                        "Date",
                        "2012-08-24"});
            table16.AddRow(new string[] {
                        "Must have",
                        "false"});
            table16.AddRow(new string[] {
                        "Shift category",
                        "Late"});
#line 109
 testRunner.And("I have a preference with", ((string)(null)), table16);
#line 114
 testRunner.When("I view preferences for date \'2012-08-23\'");
#line 115
 testRunner.And("I select day \'2012-08-24\'");
#line 116
 testRunner.And("I click set must have button");
#line 117
 testRunner.Then("I should see I have \'0\' available must haves");
#line hidden
            TechTalk.SpecFlow.Table table17 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table17.AddRow(new string[] {
                        "Date",
                        "2012-08-36"});
            table17.AddRow(new string[] {
                        "Must have",
                        "true"});
#line 118
 testRunner.And("I should see preference", ((string)(null)), table17);
#line hidden
            TechTalk.SpecFlow.Table table18 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table18.AddRow(new string[] {
                        "Date",
                        "2012-08-24"});
            table18.AddRow(new string[] {
                        "Must have",
                        "false"});
#line 122
 testRunner.And("I should see preference", ((string)(null)), table18);
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
