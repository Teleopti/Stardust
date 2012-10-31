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
    [NUnit.Framework.DescriptionAttribute("Password Policy Mobile")]
    public partial class PasswordPolicyMobileFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "PasswordPolicyMobile.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Password Policy Mobile", "In order to have a good security\r\nAs a mobile user that is trying to sign in\r\nI h" +
                    "ave a password policy", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        "Max Number Of Attempts",
                        "3"});
            table1.AddRow(new string[] {
                        "Invalid Attempt Window",
                        "30"});
            table1.AddRow(new string[] {
                        "Password Valid For Day Count",
                        "30"});
            table1.AddRow(new string[] {
                        "Password Expire Warning Day Count",
                        "3"});
            table1.AddRow(new string[] {
                        "Rule1",
                        "PasswordLengthMin8"});
#line 7
 testRunner.Given("There is a password policy with", ((string)(null)), table1);
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sign in failed after account is locked")]
        public virtual void SignInFailedAfterAccountIsLocked()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sign in failed after account is locked", ((string[])(null)));
#line 15
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table2.AddRow(new string[] {
                        "IsLocked",
                        "true"});
#line 16
 testRunner.Given("I have user logon details with", ((string)(null)), table2);
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table3.AddRow(new string[] {
                        "UserName",
                        "aa"});
            table3.AddRow(new string[] {
                        "Password",
                        "P@ssword1"});
#line 19
 testRunner.And("I am a mobile user with", ((string)(null)), table3);
#line 23
 testRunner.And("I am viewing the sign in page");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table4.AddRow(new string[] {
                        "UserName",
                        "aa"});
            table4.AddRow(new string[] {
                        "Password",
                        "P@ssword1"});
#line 24
 testRunner.When("I try to sign in with", ((string)(null)), table4);
#line 28
 testRunner.Then("I should not be signed in");
#line 29
 testRunner.And("I should see an log on error");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sign in with password will expire soon")]
        public virtual void SignInWithPasswordWillExpireSoon()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sign in with password will expire soon", ((string[])(null)));
#line 31
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 32
 testRunner.Given("Current time is \'2012-01-29\'");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table5.AddRow(new string[] {
                        "Last Password Change",
                        "2012-01-01"});
#line 33
 testRunner.And("I have user logon details with", ((string)(null)), table5);
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table6.AddRow(new string[] {
                        "UserName",
                        "aa"});
            table6.AddRow(new string[] {
                        "Password",
                        "P@ssword1"});
#line 36
 testRunner.And("I am a mobile user with", ((string)(null)), table6);
#line 40
 testRunner.And("I am viewing the sign in page");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table7.AddRow(new string[] {
                        "UserName",
                        "aa"});
            table7.AddRow(new string[] {
                        "Password",
                        "P@ssword1"});
#line 41
 testRunner.When("I try to sign in with", ((string)(null)), table7);
#line 45
 testRunner.Then("I should be signed in");
#line 46
 testRunner.And("I should see a warning message that password will be expired");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sign in with password already expired")]
        public virtual void SignInWithPasswordAlreadyExpired()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sign in with password already expired", ((string[])(null)));
#line 48
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 49
 testRunner.Given("Current time is \'2012-01-31\'");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table8.AddRow(new string[] {
                        "Last Password Change",
                        "2012-01-01"});
#line 50
 testRunner.And("I have user logon details with", ((string)(null)), table8);
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table9.AddRow(new string[] {
                        "UserName",
                        "aa"});
            table9.AddRow(new string[] {
                        "Password",
                        "P@ssword1"});
#line 53
 testRunner.And("I am a mobile user with", ((string)(null)), table9);
#line 57
 testRunner.And("I am viewing the sign in page");
#line hidden
            TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table10.AddRow(new string[] {
                        "UserName",
                        "aa"});
            table10.AddRow(new string[] {
                        "Password",
                        "P@ssword1"});
#line 58
 testRunner.When("I try to sign in with", ((string)(null)), table10);
#line 62
 testRunner.Then("I should not be signed in");
#line 63
 testRunner.And("I should see the must change password page");
#line 64
 testRunner.And("I should see an error message password has already expired");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Navigate to other page when sign in with password already expired")]
        public virtual void NavigateToOtherPageWhenSignInWithPasswordAlreadyExpired()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Navigate to other page when sign in with password already expired", ((string[])(null)));
#line 66
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 67
 testRunner.Given("Current time is \'2012-01-31\'");
#line hidden
            TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table11.AddRow(new string[] {
                        "Last Password Change",
                        "2012-01-01"});
#line 68
 testRunner.And("I have user logon details with", ((string)(null)), table11);
#line hidden
            TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table12.AddRow(new string[] {
                        "UserName",
                        "aa"});
            table12.AddRow(new string[] {
                        "Password",
                        "P@ssword1"});
#line 71
 testRunner.And("I am a mobile user with", ((string)(null)), table12);
#line 75
 testRunner.And("I am viewing the sign in page");
#line hidden
            TechTalk.SpecFlow.Table table13 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table13.AddRow(new string[] {
                        "UserName",
                        "aa"});
            table13.AddRow(new string[] {
                        "Password",
                        "P@ssword1"});
#line 76
 testRunner.When("I try to sign in with", ((string)(null)), table13);
#line 80
 testRunner.And("I navigate to week schedule page");
#line 81
 testRunner.Then("I should see the sign in page");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Change password successfully when sign in with password already expired")]
        public virtual void ChangePasswordSuccessfullyWhenSignInWithPasswordAlreadyExpired()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Change password successfully when sign in with password already expired", ((string[])(null)));
#line 83
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 84
 testRunner.Given("Current time is \'2012-01-31\'");
#line hidden
            TechTalk.SpecFlow.Table table14 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table14.AddRow(new string[] {
                        "Last Password Change",
                        "2012-01-01"});
#line 85
 testRunner.And("I have user logon details with", ((string)(null)), table14);
#line hidden
            TechTalk.SpecFlow.Table table15 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table15.AddRow(new string[] {
                        "UserName",
                        "aa"});
            table15.AddRow(new string[] {
                        "Password",
                        "P@ssword1"});
#line 88
 testRunner.And("I am a mobile user with", ((string)(null)), table15);
#line 92
 testRunner.And("I am viewing the sign in page");
#line hidden
            TechTalk.SpecFlow.Table table16 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table16.AddRow(new string[] {
                        "UserName",
                        "aa"});
            table16.AddRow(new string[] {
                        "Password",
                        "P@ssword1"});
#line 93
 testRunner.When("I try to sign in with", ((string)(null)), table16);
#line hidden
            TechTalk.SpecFlow.Table table17 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table17.AddRow(new string[] {
                        "Password",
                        "NewP@ssword1"});
            table17.AddRow(new string[] {
                        "Confirmed Password",
                        "NewP@ssword1"});
            table17.AddRow(new string[] {
                        "Old Password",
                        "P@ssword1"});
#line 97
 testRunner.And("I change my password with", ((string)(null)), table17);
#line 102
 testRunner.Then("I should be signed in");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Change password failed when sign in with password already expired")]
        public virtual void ChangePasswordFailedWhenSignInWithPasswordAlreadyExpired()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Change password failed when sign in with password already expired", ((string[])(null)));
#line 104
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 105
 testRunner.Given("Current time is \'2012-01-31\'");
#line hidden
            TechTalk.SpecFlow.Table table18 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table18.AddRow(new string[] {
                        "Last Password Change",
                        "2012-01-01"});
#line 106
 testRunner.And("I have user logon details with", ((string)(null)), table18);
#line hidden
            TechTalk.SpecFlow.Table table19 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table19.AddRow(new string[] {
                        "UserName",
                        "aa"});
            table19.AddRow(new string[] {
                        "Password",
                        "P@ssword1"});
#line 109
 testRunner.And("I am a mobile user with", ((string)(null)), table19);
#line 113
 testRunner.And("I am viewing the sign in page");
#line hidden
            TechTalk.SpecFlow.Table table20 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table20.AddRow(new string[] {
                        "UserName",
                        "aa"});
            table20.AddRow(new string[] {
                        "Password",
                        "P@ssword1"});
#line 114
 testRunner.When("I try to sign in with", ((string)(null)), table20);
#line hidden
            TechTalk.SpecFlow.Table table21 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table21.AddRow(new string[] {
                        "Password",
                        "aa"});
            table21.AddRow(new string[] {
                        "Confirmed Password",
                        "aa"});
            table21.AddRow(new string[] {
                        "Old Password",
                        "P@ssword1"});
#line 118
 testRunner.And("I change my password with", ((string)(null)), table21);
#line 123
 testRunner.Then("I should see an error message password changed failed");
#line 124
 testRunner.And("I should not be signed in");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
