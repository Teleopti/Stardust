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
    [NUnit.Framework.DescriptionAttribute("Sign in")]
    public partial class SignInFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "SignIn.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Sign in", "In order to access the site\r\nAs a user that is not signed in\r\nI want to be able t" +
                    "o sign in", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        [NUnit.Framework.DescriptionAttribute("Sign in with a user with multiple business units by user name")]
        public virtual void SignInWithAUserWithMultipleBusinessUnitsByUserName()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sign in with a user with multiple business units by user name", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 7
 testRunner.Given("I am a user with multiple business units");
#line 8
 testRunner.And("I am viewing the sign in page");
#line 9
 testRunner.When("I sign in by user name");
#line 10
 testRunner.And("I select a business unit");
#line 11
 testRunner.Then("I should be signed in");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sign in with a user with one business unit by user name and I should be directed " +
            "into that business unit direct without having to select it")]
        public virtual void SignInWithAUserWithOneBusinessUnitByUserNameAndIShouldBeDirectedIntoThatBusinessUnitDirectWithoutHavingToSelectIt()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sign in with a user with one business unit by user name and I should be directed " +
                    "into that business unit direct without having to select it", ((string[])(null)));
#line 13
this.ScenarioSetup(scenarioInfo);
#line 14
 testRunner.Given("I am a user with a single business unit");
#line 15
 testRunner.And("I am viewing the sign in page");
#line 16
 testRunner.When("I sign in by user name");
#line 17
 testRunner.Then("I should be signed in");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sign in with a user with multiple business units by Windows credentials")]
        public virtual void SignInWithAUserWithMultipleBusinessUnitsByWindowsCredentials()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sign in with a user with multiple business units by Windows credentials", ((string[])(null)));
#line 19
this.ScenarioSetup(scenarioInfo);
#line 20
 testRunner.Given("I am a user with multiple business units");
#line 21
 testRunner.And("I am viewing the sign in page");
#line 22
 testRunner.When("I sign in by windows credentials");
#line 23
 testRunner.And("I select a business unit");
#line 24
 testRunner.Then("I should be signed in");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sign in with a user with one business unit by Windows credentials and I should be" +
            " directed into that business unit direct without having to select it")]
        public virtual void SignInWithAUserWithOneBusinessUnitByWindowsCredentialsAndIShouldBeDirectedIntoThatBusinessUnitDirectWithoutHavingToSelectIt()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sign in with a user with one business unit by Windows credentials and I should be" +
                    " directed into that business unit direct without having to select it", ((string[])(null)));
#line 26
this.ScenarioSetup(scenarioInfo);
#line 27
 testRunner.Given("I am a user with a single business unit");
#line 28
 testRunner.And("I am viewing the sign in page");
#line 29
 testRunner.When("I sign in by windows credentials");
#line 30
 testRunner.Then("I should be signed in");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sign in with wrong password should give me an informative error")]
        public virtual void SignInWithWrongPasswordShouldGiveMeAnInformativeError()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sign in with wrong password should give me an informative error", ((string[])(null)));
#line 32
this.ScenarioSetup(scenarioInfo);
#line 33
 testRunner.Given("I am a user with a single business unit");
#line 34
 testRunner.And("I am viewing the sign in page");
#line 35
 testRunner.When("I sign in by user name and wrong password");
#line 36
 testRunner.Then("I should see an log on error");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sign in without permission")]
        public virtual void SignInWithoutPermission()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sign in without permission", ((string[])(null)));
#line 38
this.ScenarioSetup(scenarioInfo);
#line 39
 testRunner.Given("I dont have permission to sign in");
#line 40
 testRunner.And("I am viewing the sign in page");
#line 41
 testRunner.When("I sign in by user name");
#line 42
 testRunner.Then("I should not be signed in");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
