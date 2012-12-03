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
        [NUnit.Framework.DescriptionAttribute("Change password fails against the policy")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void ChangePasswordFailsAgainstThePolicy()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Change password fails against the policy", new string[] {
                        "ignore"});
#line 19
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
#line 20
 testRunner.Given("I am a user signed in with", ((string)(null)), table3, "Given ");
#line 24
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
#line 25
 testRunner.And("I change my password in my profile with", ((string)(null)), table4, "And ");
#line 30
 testRunner.Then("I should see password change failed with message", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sign in fails after account is locked")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void SignInFailsAfterAccountIsLocked()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sign in fails after account is locked", new string[] {
                        "ignore"});
#line 33
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
#line 34
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
#line 37
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
#line 41
 testRunner.When("I try to sign in with", ((string)(null)), table7, "When ");
#line 45
 testRunner.Then("I should not be signed in", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 46
 testRunner.And("I should see a log on error \'LogOnFailedAccountIsLocked\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("See change password when password will expire soon")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void SeeChangePasswordWhenPasswordWillExpireSoon()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("See change password when password will expire soon", new string[] {
                        "ignore"});
#line 49
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table8.AddRow(new string[] {
                        "Last Password Change X Days Ago",
                        "29"});
#line 50
 testRunner.Given("I have user logon details with", ((string)(null)), table8, "Given ");
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
#line 57
 testRunner.When("I try to sign in with", ((string)(null)), table10, "When ");
#line 61
 testRunner.Then("I should see change password page with warning \'YourPasswordWillExpireSoon\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Skip change password when password will expire soon")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void SkipChangePasswordWhenPasswordWillExpireSoon()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Skip change password when password will expire soon", new string[] {
                        "ignore"});
#line 64
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table11.AddRow(new string[] {
                        "Last Password Change X Days Ago",
                        "29"});
#line 65
 testRunner.Given("I have user logon details with", ((string)(null)), table11, "Given ");
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
 testRunner.And("I have user credential with", ((string)(null)), table12, "And ");
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
#line 72
 testRunner.When("I try to sign in with", ((string)(null)), table13, "When ");
#line 76
 testRunner.And("I click skip button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 77
 testRunner.Then("I should be signed in", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("See change password when password already expired")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void SeeChangePasswordWhenPasswordAlreadyExpired()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("See change password when password already expired", new string[] {
                        "ignore"});
#line 80
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table14 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table14.AddRow(new string[] {
                        "Last Password Change X Days Ago",
                        "30"});
#line 81
 testRunner.Given("I have user logon details with", ((string)(null)), table14, "Given ");
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
#line 84
 testRunner.And("I have user credential with", ((string)(null)), table15, "And ");
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
#line 88
 testRunner.When("I try to sign in with", ((string)(null)), table16, "When ");
#line 92
 testRunner.Then("I should see must change password page with warning \'YourPasswordHasAlreadyExpire" +
                    "d\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 93
 testRunner.And("I should not see skip button", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Manually navigate to other page when sign in with password already expired")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void ManuallyNavigateToOtherPageWhenSignInWithPasswordAlreadyExpired()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Manually navigate to other page when sign in with password already expired", new string[] {
                        "ignore"});
#line 95
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table17 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table17.AddRow(new string[] {
                        "Last Password Change X Days Ago",
                        "30"});
#line 96
 testRunner.Given("I have user logon details with", ((string)(null)), table17, "Given ");
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
#line 99
 testRunner.And("I have user credential with", ((string)(null)), table18, "And ");
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
#line 103
 testRunner.When("I try to sign in with", ((string)(null)), table19, "When ");
#line 107
 testRunner.And("I manually navigate to week schedule page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 108
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
#line 110
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table20 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table20.AddRow(new string[] {
                        "Last Password Change X Days Ago",
                        "30"});
#line 111
 testRunner.Given("I have user logon details with", ((string)(null)), table20, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table21 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table21.AddRow(new string[] {
                        "UserName",
                        "aa"});
            table21.AddRow(new string[] {
                        "Password",
                        "P@ssword1"});
#line 114
 testRunner.And("I have user credential with", ((string)(null)), table21, "And ");
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
#line 118
 testRunner.When("I try to sign in with", ((string)(null)), table22, "When ");
#line hidden
            TechTalk.SpecFlow.Table table23 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table23.AddRow(new string[] {
                        "Password",
                        "NewP@ssword1"});
            table23.AddRow(new string[] {
                        "Confirmed Password",
                        "NewP@ssword1"});
            table23.AddRow(new string[] {
                        "Old Password",
                        "P@ssword1"});
#line 122
 testRunner.And("I change my password with", ((string)(null)), table23, "And ");
#line 127
 testRunner.Then("I should be signed in", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Change password fails when password already expired")]
        [NUnit.Framework.IgnoreAttribute()]
        public virtual void ChangePasswordFailsWhenPasswordAlreadyExpired()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Change password fails when password already expired", new string[] {
                        "ignore"});
#line 129
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table24 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table24.AddRow(new string[] {
                        "Last Password Change X Days Ago",
                        "30"});
#line 130
 testRunner.Given("I have user logon details with", ((string)(null)), table24, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table25 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table25.AddRow(new string[] {
                        "UserName",
                        "aa"});
            table25.AddRow(new string[] {
                        "Password",
                        "P@ssword1"});
#line 133
 testRunner.And("I have user credential with", ((string)(null)), table25, "And ");
#line hidden
            TechTalk.SpecFlow.Table table26 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table26.AddRow(new string[] {
                        "UserName",
                        "aa"});
            table26.AddRow(new string[] {
                        "Password",
                        "P@ssword1"});
#line 137
 testRunner.When("I try to sign in with", ((string)(null)), table26, "When ");
#line hidden
            TechTalk.SpecFlow.Table table27 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table27.AddRow(new string[] {
                        "Password",
                        "aa"});
            table27.AddRow(new string[] {
                        "Confirmed Password",
                        "aa"});
            table27.AddRow(new string[] {
                        "Old Password",
                        "P@ssword1"});
#line 141
 testRunner.And("I change my password with", ((string)(null)), table27, "And ");
#line 146
 testRunner.Then("I should see an error \'PasswordChangeFailed\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 147
 testRunner.And("I should not be signed in", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
