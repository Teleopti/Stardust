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
    [NUnit.Framework.DescriptionAttribute("ASM Message")]
    public partial class ASMMessageFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "AsmMessage.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "ASM Message", "In order to communicate with supervisors\r\nAs an agent\r\nI want to receive and send" +
                    " information", ProgrammingLanguage.CSharp, ((string[])(null)));
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
#line 7
 testRunner.Given("there is a role with", ((string)(null)), table1);
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table2.AddRow(new string[] {
                        "Name",
                        "No access to ASM"});
            table2.AddRow(new string[] {
                        "Access To Asm",
                        "False"});
#line 10
 testRunner.And("there is a role with", ((string)(null)), table2);
#line hidden
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Do not show message tab if no permission to ASM")]
        public virtual void DoNotShowMessageTabIfNoPermissionToASM()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Do not show message tab if no permission to ASM", ((string[])(null)));
#line 15
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 16
 testRunner.Given("I have the role \'No access to ASM\'");
#line 17
 testRunner.When("I am viewing week schedule");
#line 18
 testRunner.Then("Message tab should not be visible");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show message tab")]
        public virtual void ShowMessageTab()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show message tab", ((string[])(null)));
#line 20
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 21
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 22
 testRunner.When("I am viewing week schedule");
#line 23
 testRunner.Then("Message tab should be visible");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Indicate new message while logged on")]
        public virtual void IndicateNewMessageWhileLoggedOn()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Indicate new message while logged on", ((string[])(null)));
#line 25
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 26
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table3.AddRow(new string[] {
                        "Title",
                        "New message"});
#line 27
 testRunner.And("My supervisor sends me a message with", ((string)(null)), table3);
#line 30
 testRunner.And("I am viewing week schedule");
#line 31
 testRunner.When("I receive new message(s)");
#line 32
 testRunner.Then("I should be notified that I have \'1\' unread message(s)");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Indicate another new message while logged on")]
        public virtual void IndicateAnotherNewMessageWhileLoggedOn()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Indicate another new message while logged on", ((string[])(null)));
#line 34
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 35
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table4.AddRow(new string[] {
                        "Title",
                        "New message"});
#line 36
 testRunner.And("I have an unread message with", ((string)(null)), table4);
#line 39
 testRunner.And("I am viewing week schedule");
#line 40
 testRunner.And("I should be notified that I have \'1\' unread message(s)");
#line 41
 testRunner.When("I receive a new message");
#line 42
 testRunner.Then("I should be notified that I have \'2\' unread message(s)");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Indicate new message at logon")]
        public virtual void IndicateNewMessageAtLogon()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Indicate new message at logon", ((string[])(null)));
#line 44
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 45
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table5.AddRow(new string[] {
                        "Title",
                        "New message"});
#line 46
 testRunner.And("I have an unread message with", ((string)(null)), table5);
#line 49
 testRunner.When("I am viewing week schedule");
#line 50
 testRunner.Then("I should be notified that I have \'1\' unread message(s)");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Navigate to message tab")]
        public virtual void NavigateToMessageTab()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Navigate to message tab", ((string[])(null)));
#line 54
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 55
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 56
 testRunner.And("I have no unread messages");
#line 57
 testRunner.And("I am viewing week schedule");
#line 58
 testRunner.When("I navigate to messages");
#line 59
 testRunner.Then("I should not see any messages");
#line 60
 testRunner.And("I should see a user-friendly message explaining I dont have any messages");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Navigate to message tab with an unread message")]
        public virtual void NavigateToMessageTabWithAnUnreadMessage()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Navigate to message tab with an unread message", ((string[])(null)));
#line 62
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 63
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table6.AddRow(new string[] {
                        "Title",
                        "New message"});
#line 64
 testRunner.And("I have an unread message with", ((string)(null)), table6);
#line 67
 testRunner.And("I am viewing week schedule");
#line 68
 testRunner.When("I navigate to messages");
#line 69
 testRunner.Then("I should see a message in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Open unread message")]
        public virtual void OpenUnreadMessage()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Open unread message", ((string[])(null)));
#line 71
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 72
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table7.AddRow(new string[] {
                        "Title",
                        "New message"});
            table7.AddRow(new string[] {
                        "Message",
                        "Text in message"});
#line 73
 testRunner.And("I have an unread message with", ((string)(null)), table7);
#line 77
 testRunner.And("I am viewing week schedule");
#line 78
 testRunner.When("I navigate to messages");
#line 79
 testRunner.And("I click on the message at position \'1\' in the list");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table8.AddRow(new string[] {
                        "Title",
                        "New message"});
            table8.AddRow(new string[] {
                        "Message",
                        "Text in message"});
#line 80
 testRunner.Then("I should see the message details form with", ((string)(null)), table8);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Confirm message is read")]
        public virtual void ConfirmMessageIsRead()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Confirm message is read", ((string[])(null)));
#line 85
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 86
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table9.AddRow(new string[] {
                        "Title",
                        "New message"});
#line 87
 testRunner.And("I have an unread message with", ((string)(null)), table9);
#line 90
 testRunner.And("I am viewing week schedule");
#line 91
 testRunner.And("I navigate to messages");
#line 92
 testRunner.And("I click on the message at position \'1\' in the list");
#line 93
 testRunner.When("I click the confirm button");
#line 94
 testRunner.Then("I should not see any messages");
#line 95
 testRunner.And("I navigate to messages");
#line 96
 testRunner.And("I should see a user-friendly message explaining I dont have any messages");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sort messages in list by latest message")]
        public virtual void SortMessagesInListByLatestMessage()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sort messages in list by latest message", ((string[])(null)));
#line 98
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 99
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table10.AddRow(new string[] {
                        "Title",
                        "Message"});
            table10.AddRow(new string[] {
                        "Is oldest message",
                        "True"});
#line 100
 testRunner.And("I have an unread message with", ((string)(null)), table10);
#line hidden
            TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table11.AddRow(new string[] {
                        "Title",
                        "Latest message"});
#line 104
 testRunner.And("I have an unread message with", ((string)(null)), table11);
#line 107
 testRunner.And("I am viewing week schedule");
#line 108
 testRunner.When("I navigate to messages");
#line 109
 testRunner.Then("I should see the message with title \'Latest message\' at position \'1\' in the list");
#line 110
 testRunner.And("I should see the message with title \'Message\' at position \'2\' in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Reduce number of unread messages in message tab title")]
        public virtual void ReduceNumberOfUnreadMessagesInMessageTabTitle()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Reduce number of unread messages in message tab title", ((string[])(null)));
#line 112
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 113
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table12.AddRow(new string[] {
                        "Title",
                        "New message"});
#line 114
 testRunner.And("I have an unread message with", ((string)(null)), table12);
#line hidden
            TechTalk.SpecFlow.Table table13 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table13.AddRow(new string[] {
                        "Title",
                        "Another new message"});
#line 117
 testRunner.And("I have an unread message with", ((string)(null)), table13);
#line 120
 testRunner.And("I am viewing week schedule");
#line 121
 testRunner.And("I should be notified that I have \'2\' unread message(s)");
#line 122
 testRunner.When("I navigate to messages");
#line 123
 testRunner.And("I confirm reading the message at position \'1\' in the list");
#line 125
 testRunner.And("I navigate to messages");
#line 126
 testRunner.Then("I should be notified that I have \'1\' unread message(s)");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
