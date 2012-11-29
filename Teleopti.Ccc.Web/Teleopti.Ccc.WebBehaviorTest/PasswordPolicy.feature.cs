﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.77
//      SpecFlow Generator Version:1.9.0.0
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
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Password Policy")]
    public partial class PasswordPolicyFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "PasswordPolicy.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Password Policy", "In order to have a good security\r\nAs a user that is trying to sign in or change p" +
                    "assword\r\nI have a password policy", ProgrammingLanguage.CSharp, ((string[])(null)));
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
 testRunner.Given("There is a password policy with", ((string)(null)), table1, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table2.AddRow(new string[] {
                        "Name",
                        "Agent"});
#line 14
 testRunner.And("I have a role with", ((string)(null)), table2, "And ");
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Change password failed against the policy")]
        public virtual void ChangePasswordFailedAgainstThePolicy()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Change password failed against the policy", ((string[])(null)));
#line 18
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
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
 testRunner.Given("I am a user signed in with", ((string)(null)), table3, "Given ");
#line 23
 testRunner.When("I view password setting page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table4.AddRow(new string[] {
                        "Password",
                        "aa"});
            table4.AddRow(new string[] {
                        "Confirmed Password",
                        "aa"});
            table4.AddRow(new string[] {
                        "Old Password",
                        "P@ssword1"});
#line 24
 testRunner.And("I change my password with", ((string)(null)), table4, "And ");
#line 29
 testRunner.Then("I should see password changed failed with message", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sign in failed after account is locked")]
        public virtual void SignInFailedAfterAccountIsLocked()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sign in failed after account is locked", ((string[])(null)));
#line 31
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table5.AddRow(new string[] {
                        "IsLocked",
                        "true"});
#line 32
 testRunner.Given("I have user logon details with", ((string)(null)), table5, "Given ");
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
#line 35
 testRunner.And("I have user credential with", ((string)(null)), table6, "And ");
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
#line 39
 testRunner.When("I try to sign in with", ((string)(null)), table7, "When ");
#line 43
 testRunner.Then("I should not be signed in", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 44
 testRunner.And("I should see a log on error \'LogOnFailedAccountIsLocked\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sign in with password will expire soon")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void SignInWithPasswordWillExpireSoon()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sign in with password will expire soon", new string[] {
                        "ignore"});
#line 47
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 48
 testRunner.Given("Current time is \'2012-01-30\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table8.AddRow(new string[] {
                        "Last Password Change",
                        "2012-01-01"});
#line 49
 testRunner.And("I have user logon details with", ((string)(null)), table8, "And ");
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
#line 52
 testRunner.And("I have user credential with", ((string)(null)), table9, "And ");
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
#line 56
 testRunner.When("I try to sign in with", ((string)(null)), table10, "When ");
#line 60
 testRunner.Then("I should be signed in", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sign in with password already expired")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void SignInWithPasswordAlreadyExpired()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sign in with password already expired", new string[] {
                        "ignore"});
#line 63
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 64
 testRunner.Given("Current time is \'2012-01-31\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table11.AddRow(new string[] {
                        "Last Password Change",
                        "2012-01-01"});
#line 65
 testRunner.And("I have user logon details with", ((string)(null)), table11, "And ");
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
#line 68
 testRunner.And("I am a user with", ((string)(null)), table12, "And ");
#line 72
 testRunner.And("I am viewing the sign in page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
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
#line 73
 testRunner.When("I try to sign in with", ((string)(null)), table13, "When ");
#line 77
 testRunner.Then("I should not be signed in", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 78
 testRunner.And("I should be see the must change password page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 79
 testRunner.And("I should see an error message password has already expired", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Navigate to other page when sign in with password already expired")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void NavigateToOtherPageWhenSignInWithPasswordAlreadyExpired()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Navigate to other page when sign in with password already expired", new string[] {
                        "ignore"});
#line 81
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 82
 testRunner.Given("Current time is \'2012-01-31\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table14 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table14.AddRow(new string[] {
                        "Last Password Change",
                        "2012-01-01"});
#line 83
 testRunner.And("I have user logon details with", ((string)(null)), table14, "And ");
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
#line 86
 testRunner.And("I am a user with", ((string)(null)), table15, "And ");
#line 90
 testRunner.And("I am viewing the sign in page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
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
#line 91
 testRunner.When("I try to sign in with", ((string)(null)), table16, "When ");
#line 95
 testRunner.And("I navigate to week schedule page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 96
 testRunner.Then("I should see the sign in page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Change password successfully when password already expired")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void ChangePasswordSuccessfullyWhenPasswordAlreadyExpired()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Change password successfully when password already expired", new string[] {
                        "ignore"});
#line 98
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 99
 testRunner.Given("Current time is \'2012-01-31\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table17 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table17.AddRow(new string[] {
                        "Last Password Change",
                        "2012-01-01"});
#line 100
 testRunner.And("I have user logon details with", ((string)(null)), table17, "And ");
#line hidden
            TechTalk.SpecFlow.Table table18 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table18.AddRow(new string[] {
                        "UserName",
                        "aa"});
            table18.AddRow(new string[] {
                        "Password",
                        "P@ssword1"});
#line 103
 testRunner.And("I am a user with", ((string)(null)), table18, "And ");
#line 107
 testRunner.And("I am viewing the sign in page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
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
#line 108
 testRunner.When("I try to sign in with", ((string)(null)), table19, "When ");
#line hidden
            TechTalk.SpecFlow.Table table20 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table20.AddRow(new string[] {
                        "Password",
                        "NewP@ssword1"});
            table20.AddRow(new string[] {
                        "Confirmed Password",
                        "NewP@ssword1"});
            table20.AddRow(new string[] {
                        "Old Password",
                        "P@ssword1"});
#line 112
 testRunner.And("I change my password with", ((string)(null)), table20, "And ");
#line 117
 testRunner.Then("I should be signed in", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Change password failed when password already expired")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void ChangePasswordFailedWhenPasswordAlreadyExpired()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Change password failed when password already expired", new string[] {
                        "ignore"});
#line 119
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 120
 testRunner.Given("Current time is \'2012-01-31\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table21 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table21.AddRow(new string[] {
                        "Last Password Change",
                        "2012-01-01"});
#line 121
 testRunner.And("I have user logon details with", ((string)(null)), table21, "And ");
#line hidden
            TechTalk.SpecFlow.Table table22 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table22.AddRow(new string[] {
                        "UserName",
                        "aa"});
            table22.AddRow(new string[] {
                        "Password",
                        "P@ssword1"});
#line 124
 testRunner.And("I am a user with", ((string)(null)), table22, "And ");
#line 128
 testRunner.And("I am viewing the sign in page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table23 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table23.AddRow(new string[] {
                        "UserName",
                        "aa"});
            table23.AddRow(new string[] {
                        "Password",
                        "P@ssword1"});
#line 129
 testRunner.When("I try to sign in with", ((string)(null)), table23, "When ");
#line hidden
            TechTalk.SpecFlow.Table table24 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table24.AddRow(new string[] {
                        "Password",
                        "aa"});
            table24.AddRow(new string[] {
                        "Confirmed Password",
                        "aa"});
            table24.AddRow(new string[] {
                        "Old Password",
                        "P@ssword1"});
#line 133
 testRunner.And("I change my password with", ((string)(null)), table24, "And ");
#line 138
 testRunner.Then("I should see an error message password changed failed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 139
 testRunner.And("I should not be signed in", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
