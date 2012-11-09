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
    [NUnit.Framework.DescriptionAttribute("Routing New")]
    [NUnit.Framework.IgnoreAttribute()]
    public partial class RoutingNewFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "RoutingNew.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Routing New", "In order make it easy to browse to the site\r\nAs a user\r\nI want to be redirected t" +
                    "o the correct locations", ProgrammingLanguage.CSharp, new string[] {
                        "ignore"});
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
                        "Access to all areas"});
            table1.AddRow(new string[] {
                        "Access to mobile reports",
                        "true"});
            table1.AddRow(new string[] {
                        "Access to mytime web",
                        "true"});
#line 8
 testRunner.Given("there is a role with", ((string)(null)), table1);
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table2.AddRow(new string[] {
                        "Name",
                        "Access to report not mytime"});
            table2.AddRow(new string[] {
                        "Access to mobile reports",
                        "true"});
            table2.AddRow(new string[] {
                        "Access to mytime web",
                        "false"});
#line 13
 testRunner.Given("there is a role with", ((string)(null)), table2);
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table3.AddRow(new string[] {
                        "Name",
                        "Access to mytime not report"});
            table3.AddRow(new string[] {
                        "Access to mobile reports",
                        "false"});
            table3.AddRow(new string[] {
                        "Access to mytime web",
                        "true"});
#line 18
 testRunner.Given("there is a role with", ((string)(null)), table3);
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Browse to root")]
        public virtual void BrowseToRoot()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Browse to root", ((string[])(null)));
#line 24
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 25
 testRunner.Given("I am not signed in");
#line 26
 testRunner.When("I navigate to the site\'s root");
#line 28
 testRunner.Then("I should see sign in page");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Browse to MyTime")]
        public virtual void BrowseToMyTime()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Browse to MyTime", ((string[])(null)));
#line 30
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 31
 testRunner.Given("I am not signed in");
#line 32
 testRunner.When("I navigate to MyTime");
#line 34
 testRunner.Then("I should see sign in page");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Browse to Mobile Reports")]
        public virtual void BrowseToMobileReports()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Browse to Mobile Reports", ((string[])(null)));
#line 37
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 38
 testRunner.Given("I am not signed in");
#line 39
 testRunner.When("I navigate to Mobile Reports");
#line 40
 testRunner.Then("I should see Mobile Report\'s sign in page");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Browse to root and sign in to menu")]
        public virtual void BrowseToRootAndSignInToMenu()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Browse to root and sign in to menu", ((string[])(null)));
#line 42
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 43
 testRunner.Given("I have the role \'Access to mytime and report\'");
#line 44
 testRunner.When("I navigate to the site\'s root");
#line 45
 testRunner.And("I sign in");
#line 46
 testRunner.Then("I should see the global menu");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Browse to root and sign in to mobile menu")]
        public virtual void BrowseToRootAndSignInToMobileMenu()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Browse to root and sign in to mobile menu", ((string[])(null)));
#line 50
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 51
 testRunner.Given("I have the role \'Access to all areas\'");
#line 52
 testRunner.When("I navigate to the site\'s root mobile signin page");
#line 53
 testRunner.And("I sign in");
#line 54
 testRunner.Then("I should see the mobile global menu");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Browse to root and sign in to MyTime")]
        public virtual void BrowseToRootAndSignInToMyTime()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Browse to root and sign in to MyTime", ((string[])(null)));
#line 56
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 57
 testRunner.Given("I have the role \'Access to mytime not report\'");
#line 58
 testRunner.When("I navigate to the site\'s root");
#line 59
 testRunner.And("I sign in");
#line 60
 testRunner.Then("I should see MyTime");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Browse to root and sign in to Mobile Reports")]
        public virtual void BrowseToRootAndSignInToMobileReports()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Browse to root and sign in to Mobile Reports", ((string[])(null)));
#line 62
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 63
 testRunner.Given("I have the role \'Access to report not mytime\'");
#line 64
 testRunner.When("I navigate to the site\'s root");
#line 65
 testRunner.And("I sign in");
#line 66
 testRunner.Then("I should see Mobile Reports");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Browse to MyTime and sign in")]
        public virtual void BrowseToMyTimeAndSignIn()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Browse to MyTime and sign in", ((string[])(null)));
#line 68
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 69
 testRunner.Given("I have the role \'Access to all areas\'");
#line 70
 testRunner.When("I navigate to MyTime");
#line 71
 testRunner.And("I sign in");
#line 72
 testRunner.Then("I should see MyTime");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Browse to Mobile Reports and sign in")]
        public virtual void BrowseToMobileReportsAndSignIn()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Browse to Mobile Reports and sign in", ((string[])(null)));
#line 74
this.ScenarioSetup(scenarioInfo);
#line 7
this.FeatureBackground();
#line 75
 testRunner.Given("I have the role \'Access to all areas\'");
#line 76
 testRunner.When("I navigate to Mobile Reports");
#line 77
 testRunner.And("I sign in");
#line 78
 testRunner.Then("I should see Mobile Reports");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
