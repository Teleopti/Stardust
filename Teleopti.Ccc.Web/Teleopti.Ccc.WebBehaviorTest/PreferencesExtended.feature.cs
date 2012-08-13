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
    [NUnit.Framework.DescriptionAttribute("Preferences Extended")]
    public partial class PreferencesExtendedFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "PreferencesExtended.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Preferences Extended", "In order to view and submit when I prefer to work in more detail\r\nAs an agent\r\nI " +
                    "want to view and submit extended preferences", ProgrammingLanguage.CSharp, ((string[])(null)));
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
#line 9
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table1.AddRow(new string[] {
                        "Name",
                        "Published schedule"});
            table1.AddRow(new string[] {
                        "Schedule published to date",
                        "2012-06-24"});
#line 10
    testRunner.Given("there is a workflow control set with", ((string)(null)), table1);
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table2.AddRow(new string[] {
                        "Name",
                        "Access to mytime"});
            table2.AddRow(new string[] {
                        "Access to mobile reports",
                        "false"});
#line 14
 testRunner.And("there is a role with", ((string)(null)), table2);
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table3.AddRow(new string[] {
                        "Name",
                        "No access to extended preferences"});
            table3.AddRow(new string[] {
                        "Access to extended preferences",
                        "false"});
#line 18
 testRunner.And("there is a role with", ((string)(null)), table3);
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("See indication of an extended preference NEW")]
        public virtual void SeeIndicationOfAnExtendedPreferenceNEW()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("See indication of an extended preference NEW", ((string[])(null)));
#line 23
this.ScenarioSetup(scenarioInfo);
#line 9
this.FeatureBackground();
#line 24
 testRunner.Given("I am a user");
#line 25
 testRunner.And("I have the role \'Access to mytime\'");
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
#line 26
 testRunner.And("I have a schedule period with", ((string)(null)), table4);
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table5.AddRow(new string[] {
                        "Start date",
                        "2012-06-18"});
#line 31
 testRunner.And("I have a person period with", ((string)(null)), table5);
#line 34
 testRunner.And("I have the workflow control set \'Published schedule\'");
#line 35
 testRunner.And("I have an extended preference on \'2012-06-20\'");
#line 36
 testRunner.When("I view preferences for date \'2012-06-20\'");
#line 37
 testRunner.And("I click the extended preference indication on \'2012-06-20\'");
#line 38
 testRunner.Then("I should see that I have an extended preference on \'2012-06-20\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("See extended preference NEW")]
        public virtual void SeeExtendedPreferenceNEW()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("See extended preference NEW", ((string[])(null)));
#line 40
this.ScenarioSetup(scenarioInfo);
#line 9
this.FeatureBackground();
#line 41
 testRunner.Given("I am a user");
#line 42
 testRunner.And("I have the role \'Access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table6.AddRow(new string[] {
                        "Start date",
                        "2012-06-18"});
            table6.AddRow(new string[] {
                        "Type",
                        "Week"});
            table6.AddRow(new string[] {
                        "Length",
                        "1"});
#line 43
 testRunner.And("I have a schedule period with", ((string)(null)), table6);
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table7.AddRow(new string[] {
                        "Start date",
                        "2012-06-18"});
#line 48
 testRunner.And("I have a person period with", ((string)(null)), table7);
#line 51
 testRunner.And("I have the workflow control set \'Published schedule\'");
#line 52
 testRunner.And("I have an extended preference on \'2012-06-20\'");
#line 53
 testRunner.When("I view preferences for date \'2012-06-20\'");
#line 54
 testRunner.And("I click the extended preference indication on \'2012-06-20\'");
#line 55
 testRunner.Then("I should see the extended preference on \'2012-06-20\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("See extended preference without permission NEW")]
        public virtual void SeeExtendedPreferenceWithoutPermissionNEW()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("See extended preference without permission NEW", ((string[])(null)));
#line 57
this.ScenarioSetup(scenarioInfo);
#line 9
this.FeatureBackground();
#line 58
 testRunner.Given("I am a user");
#line 59
 testRunner.And("I have the role \'No access to extended preferences\'");
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
#line 60
 testRunner.And("I have a schedule period with", ((string)(null)), table8);
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table9.AddRow(new string[] {
                        "Start date",
                        "2012-06-18"});
#line 65
 testRunner.And("I have a person period with", ((string)(null)), table9);
#line 68
 testRunner.And("I have the workflow control set \'Published schedule\'");
#line 69
 testRunner.And("I have an extended preference on \'2012-06-20\'");
#line 70
 testRunner.When("I view preferences for date \'2012-06-20\'");
#line 71
 testRunner.And("I click the extended preference indication on \'2012-06-20\'");
#line 72
 testRunner.Then("I should see the extended preference on \'2012-06-20\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("See indication of an extended preference")]
        public virtual void SeeIndicationOfAnExtendedPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("See indication of an extended preference", ((string[])(null)));
#line 78
this.ScenarioSetup(scenarioInfo);
#line 9
this.FeatureBackground();
#line 79
 testRunner.Given("I am an agent");
#line 80
 testRunner.And("I have an existing extended preference");
#line 81
 testRunner.When("I view preferences");
#line 82
 testRunner.Then("I should see that I have an existing extended preference");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("See extended preference")]
        public virtual void SeeExtendedPreference()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("See extended preference", ((string[])(null)));
#line 84
this.ScenarioSetup(scenarioInfo);
#line 9
this.FeatureBackground();
#line 85
 testRunner.Given("I am an agent");
#line 86
 testRunner.And("I have an existing extended preference");
#line 87
 testRunner.When("I view preferences");
#line 88
 testRunner.And("I click the extended preference indication");
#line 89
 testRunner.Then("I should see my existing extended preference");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("See extended preference without permission")]
        public virtual void SeeExtendedPreferenceWithoutPermission()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("See extended preference without permission", ((string[])(null)));
#line 91
this.ScenarioSetup(scenarioInfo);
#line 9
this.FeatureBackground();
#line 92
 testRunner.Given("I am an agent without access to extended preferences");
#line 93
 testRunner.And("I have an existing extended preference");
#line 94
 testRunner.When("I view preferences");
#line 95
 testRunner.And("I click the extended preference indication");
#line 96
 testRunner.Then("I should see my existing extended preference");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
