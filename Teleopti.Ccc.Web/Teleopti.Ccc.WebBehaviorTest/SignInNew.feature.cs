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
    [NUnit.Framework.DescriptionAttribute("Sign in New")]
    public partial class SignInNewFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "SignInNew.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Sign in New", "In order to access the site\r\nAs a user that is not signed in\r\nI want to be able t" +
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
        
        public virtual void FeatureBackground()
        {
#line 6
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table1.AddRow(new string[] {
                        "Name",
                        "Business Unit 1"});
#line 8
 testRunner.Given("there is a business unit with", ((string)(null)), table1);
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table2.AddRow(new string[] {
                        "Name",
                        "Business Unit 2"});
#line 11
 testRunner.And("there is a business unit with", ((string)(null)), table2);
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table3.AddRow(new string[] {
                        "Name",
                        "Scenario 1"});
            table3.AddRow(new string[] {
                        "Business Unit",
                        "Business Unit 1"});
#line 14
 testRunner.And("there is a scenario", ((string)(null)), table3);
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table4.AddRow(new string[] {
                        "Name",
                        "Scenario 2"});
            table4.AddRow(new string[] {
                        "Business Unit",
                        "Business Unit 2"});
#line 18
 testRunner.And("there is a scenario", ((string)(null)), table4);
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table5.AddRow(new string[] {
                        "Name",
                        "Role for business unit 1"});
            table5.AddRow(new string[] {
                        "Business Unit",
                        "Business Unit 1"});
            table5.AddRow(new string[] {
                        "Access to mytime web",
                        "true"});
#line 22
 testRunner.And("there is a role with", ((string)(null)), table5);
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table6.AddRow(new string[] {
                        "Name",
                        "Role for business unit 2"});
            table6.AddRow(new string[] {
                        "Business Unit",
                        "Business Unit 2"});
            table6.AddRow(new string[] {
                        "Access to mytime web",
                        "true"});
#line 27
 testRunner.And("there is a role with", ((string)(null)), table6);
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sign in with a user with multiple business units by user name")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void SignInWithAUserWithMultipleBusinessUnitsByUserName()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sign in with a user with multiple business units by user name", new string[] {
                        "ignore"});
#line 33
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 34
 testRunner.Given("I have the role \'Role for business unit 1\'");
#line 35
 testRunner.And("I have the role \'Role for business unit 2\'");
#line 36
 testRunner.And("I am viewing the new sign in page");
#line 37
 testRunner.When("I select application logon data source");
#line 38
 testRunner.And("I try to sign in by application logon");
#line 39
 testRunner.And("I select a business unit");
#line 41
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
#line 43
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 44
 testRunner.Given("I have the role \'Role for business unit 1\'");
#line 45
 testRunner.And("I am viewing the new sign in page");
#line 46
 testRunner.When("I select application logon data source");
#line 47
 testRunner.And("I try to sign in by application logon");
#line 48
 testRunner.Then("I should be signed in");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sign in with a user with multiple business units by Windows credentials")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void SignInWithAUserWithMultipleBusinessUnitsByWindowsCredentials()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sign in with a user with multiple business units by Windows credentials", new string[] {
                        "ignore"});
#line 50
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 51
 testRunner.Given("Windows user have the role \'Role for business unit 1\'");
#line 52
 testRunner.And("Windows user have the role \'Role for business unit 2\'");
#line 53
 testRunner.And("I am viewing the sign in page");
#line 54
 testRunner.When("I select windows logon data source");
#line 55
 testRunner.And("I sign in by windows credentials");
#line 56
 testRunner.And("I select a business unit");
#line 57
 testRunner.Then("I should be signed in");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sign in with a user with one business unit by Windows credentials and I should be" +
            " directed into that business unit direct without having to select it")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void SignInWithAUserWithOneBusinessUnitByWindowsCredentialsAndIShouldBeDirectedIntoThatBusinessUnitDirectWithoutHavingToSelectIt()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sign in with a user with one business unit by Windows credentials and I should be" +
                    " directed into that business unit direct without having to select it", new string[] {
                        "ignore"});
#line 59
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 60
 testRunner.Given("Windows user have the role \'Role for business unit 2\'");
#line 61
 testRunner.And("I am viewing the sign in page");
#line 62
 testRunner.When("I select windows logon data source");
#line 63
 testRunner.And("I sign in by windows credentials");
#line 64
 testRunner.Then("I should be signed in");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sign in with wrong password should give me an informative error")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void SignInWithWrongPasswordShouldGiveMeAnInformativeError()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sign in with wrong password should give me an informative error", new string[] {
                        "ignore"});
#line 66
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 67
 testRunner.Given("I have the role \'Role for business unit 1\'");
#line 68
 testRunner.And("I am viewing the sign in page");
#line 69
 testRunner.When("I select application logon data source");
#line 70
 testRunner.And("I sign in by user name and wrong password");
#line 71
 testRunner.Then("I should see an log on error");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sign in without permission")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void SignInWithoutPermission()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sign in without permission", new string[] {
                        "ignore"});
#line 73
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 74
 testRunner.Given("I dont have permission to sign in");
#line 75
 testRunner.And("I am viewing the new sign in page");
#line 76
 testRunner.When("I select application logon data source");
#line 77
 testRunner.And("I try to sign in by application logon");
#line 78
 testRunner.Then("I should not be signed in");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
