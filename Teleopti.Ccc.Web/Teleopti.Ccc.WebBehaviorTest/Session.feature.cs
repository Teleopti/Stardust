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
    [NUnit.Framework.DescriptionAttribute("Session")]
    public partial class SessionFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Session.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Session", "In order to be able to work with the application\r\nAs an agent\r\nI want the applica" +
                    "tion to handle my login session approprietly", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        [NUnit.Framework.DescriptionAttribute("Stay signed in after server restart")]
        public virtual void StaySignedInAfterServerRestart()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Stay signed in after server restart", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 7
 testRunner.Given("I am signed in");
#line 8
 testRunner.Then("I should be signed in");
#line 9
 testRunner.When("I navigate the internet");
#line 10
 testRunner.And("the server restarts");
#line 11
 testRunner.And("I navigate to an application page");
#line 12
 testRunner.Then("I should be signed in");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Signed out when cookie expires while I browse the internet")]
        public virtual void SignedOutWhenCookieExpiresWhileIBrowseTheInternet()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Signed out when cookie expires while I browse the internet", ((string[])(null)));
#line 14
this.ScenarioSetup(scenarioInfo);
#line 15
 testRunner.Given("I am signed in");
#line 16
 testRunner.Then("I should be signed in");
#line 17
 testRunner.When("my cookie expires");
#line 18
 testRunner.And("I navigate the internet");
#line 19
 testRunner.And("I navigate to an application page");
#line 20
 testRunner.Then("I should be signed out");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Signed out when cookie expires while I have the browser open")]
        public virtual void SignedOutWhenCookieExpiresWhileIHaveTheBrowserOpen()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Signed out when cookie expires while I have the browser open", ((string[])(null)));
#line 22
this.ScenarioSetup(scenarioInfo);
#line 23
 testRunner.Given("I am signed in");
#line 24
 testRunner.Then("I should be signed in");
#line 25
 testRunner.When("my cookie expires");
#line 26
 testRunner.And("I navigate to an application page");
#line 27
 testRunner.Then("I should be signed out");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Signed out when saving preference when cookie is expired")]
        public virtual void SignedOutWhenSavingPreferenceWhenCookieIsExpired()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Signed out when saving preference when cookie is expired", ((string[])(null)));
#line 29
this.ScenarioSetup(scenarioInfo);
#line 30
 testRunner.Given("I am an agent");
#line 31
 testRunner.And("I have an open workflow control set with an allowed standard preference");
#line 32
 testRunner.And("I am viewing preferences");
#line 33
 testRunner.When("my cookie expires");
#line 34
 testRunner.And("I select an editable day without preference");
#line 35
 testRunner.And("I try to select a standard preference");
#line 36
 testRunner.Then("I should be signed out");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Signed out when navigating to next period when cookie is expired")]
        public virtual void SignedOutWhenNavigatingToNextPeriodWhenCookieIsExpired()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Signed out when navigating to next period when cookie is expired", ((string[])(null)));
#line 38
this.ScenarioSetup(scenarioInfo);
#line 39
 testRunner.Given("I am an agent");
#line 40
 testRunner.And("I have several virtual schedule periods");
#line 41
 testRunner.And("I am viewing preferences");
#line 42
 testRunner.When("my cookie expires");
#line 43
 testRunner.And("I click next virtual schedule period button");
#line 44
 testRunner.Then("I should be signed out");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Corrupt cookie due to upgrade should be overwritten by a logon")]
        public virtual void CorruptCookieDueToUpgradeShouldBeOverwrittenByALogon()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Corrupt cookie due to upgrade should be overwritten by a logon", ((string[])(null)));
#line 46
this.ScenarioSetup(scenarioInfo);
#line 47
 testRunner.Given("I am signed in");
#line 48
 testRunner.When("My cookie gets corrupt");
#line 49
 testRunner.And("I navigate to an application page");
#line 50
 testRunner.Then("I should be signed out");
#line 51
 testRunner.When("I sign in again");
#line 52
 testRunner.Then("I should be signed in");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Corrupt cookie due to no longer existing database should be overwritten by a logo" +
            "n")]
        public virtual void CorruptCookieDueToNoLongerExistingDatabaseShouldBeOverwrittenByALogon()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Corrupt cookie due to no longer existing database should be overwritten by a logo" +
                    "n", ((string[])(null)));
#line 54
this.ScenarioSetup(scenarioInfo);
#line 55
 testRunner.Given("I am signed in");
#line 56
 testRunner.When("My cookie gets pointed to non existing database");
#line 57
 testRunner.And("I navigate to an application page");
#line 58
 testRunner.Then("I should be signed out");
#line 59
 testRunner.When("I sign in again");
#line 60
 testRunner.Then("I should be signed in");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
