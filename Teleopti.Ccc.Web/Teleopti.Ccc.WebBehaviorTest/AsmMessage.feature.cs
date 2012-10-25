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
#line 27
 testRunner.And("I am viewing week schedule");
#line 28
 testRunner.When("I receive message number \'1\' while not viewing message page");
#line 29
 testRunner.Then("I should be notified that I have \'1\' unread message(s)");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Indicate another new message while logged on")]
        public virtual void IndicateAnotherNewMessageWhileLoggedOn()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Indicate another new message while logged on", ((string[])(null)));
#line 31
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 32
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table3.AddRow(new string[] {
                        "Title",
                        "New message"});
#line 33
 testRunner.And("I have an unread message with", ((string)(null)), table3);
#line 36
 testRunner.And("I am viewing week schedule");
#line 37
 testRunner.And("I should be notified that I have \'1\' unread message(s)");
#line 38
 testRunner.When("I receive message number \'2\' while not viewing message page");
#line 39
 testRunner.Then("I should be notified that I have \'2\' unread message(s)");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Indicate new message at logon")]
        public virtual void IndicateNewMessageAtLogon()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Indicate new message at logon", ((string[])(null)));
#line 41
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 42
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table4.AddRow(new string[] {
                        "Title",
                        "New message"});
#line 43
 testRunner.And("I have an unread message with", ((string)(null)), table4);
#line 46
 testRunner.When("I am viewing week schedule");
#line 47
 testRunner.Then("I should be notified that I have \'1\' unread message(s)");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Navigate to message tab")]
        public virtual void NavigateToMessageTab()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Navigate to message tab", ((string[])(null)));
#line 51
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 52
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 53
 testRunner.And("I have no unread messages");
#line 54
 testRunner.And("I am viewing week schedule");
#line 55
 testRunner.When("I navigate to messages");
#line 56
 testRunner.Then("I should not see any messages");
#line 57
 testRunner.And("I should see a user-friendly message explaining I dont have any messages");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View unread messages")]
        public virtual void ViewUnreadMessages()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View unread messages", ((string[])(null)));
#line 59
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 60
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table5.AddRow(new string[] {
                        "Title",
                        "New message"});
#line 61
 testRunner.And("I have an unread message with", ((string)(null)), table5);
#line 64
 testRunner.When("I am viewing messages");
#line 65
 testRunner.Then("I should see \'1\' message(s) in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Open unread message")]
        public virtual void OpenUnreadMessage()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Open unread message", ((string[])(null)));
#line 67
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 68
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table6.AddRow(new string[] {
                        "Title",
                        "New message"});
            table6.AddRow(new string[] {
                        "Message",
                        "Text in message"});
#line 69
 testRunner.And("I have an unread message with", ((string)(null)), table6);
#line 73
 testRunner.When("I am viewing messages");
#line 74
 testRunner.And("I click on the message at position \'1\' in the list");
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
#line 75
 testRunner.Then("I should see the message details form with", ((string)(null)), table7);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Confirm message is read")]
        public virtual void ConfirmMessageIsRead()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Confirm message is read", ((string[])(null)));
#line 80
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 81
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table8.AddRow(new string[] {
                        "Title",
                        "New message"});
#line 82
 testRunner.And("I have an unread message with", ((string)(null)), table8);
#line 85
 testRunner.When("I am viewing messages");
#line 86
 testRunner.And("I click on the message at position \'1\' in the list");
#line 87
 testRunner.When("I click the confirm button");
#line 88
 testRunner.Then("I should not see any messages");
#line 89
 testRunner.And("I should see a user-friendly message explaining I dont have any messages");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sort messages in list by latest message")]
        public virtual void SortMessagesInListByLatestMessage()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sort messages in list by latest message", ((string[])(null)));
#line 91
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 92
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table9.AddRow(new string[] {
                        "Title",
                        "Message"});
            table9.AddRow(new string[] {
                        "Is oldest message",
                        "True"});
#line 93
 testRunner.And("I have an unread message with", ((string)(null)), table9);
#line hidden
            TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table10.AddRow(new string[] {
                        "Title",
                        "Latest message"});
#line 97
 testRunner.And("I have an unread message with", ((string)(null)), table10);
#line 100
 testRunner.When("I am viewing messages");
#line 101
 testRunner.Then("I should see the message with title \'Latest message\' at position \'1\' in the list");
#line 102
 testRunner.And("I should see the message with title \'Message\' at position \'2\' in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Reduce number of unread messages in message tab title")]
        public virtual void ReduceNumberOfUnreadMessagesInMessageTabTitle()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Reduce number of unread messages in message tab title", ((string[])(null)));
#line 104
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 105
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table11.AddRow(new string[] {
                        "Title",
                        "New message"});
#line 106
 testRunner.And("I have an unread message with", ((string)(null)), table11);
#line hidden
            TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table12.AddRow(new string[] {
                        "Title",
                        "Another new message"});
#line 109
 testRunner.And("I have an unread message with", ((string)(null)), table12);
#line 112
 testRunner.And("I am viewing week schedule");
#line 113
 testRunner.And("I should be notified that I have \'2\' unread message(s)");
#line 114
 testRunner.When("I navigate to messages");
#line 115
 testRunner.And("I confirm reading the message at position \'1\' of \'2\' in the list");
#line 116
 testRunner.Then("I should be notified that I have \'1\' unread message(s)");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Receive a new message when viewing message page")]
        public virtual void ReceiveANewMessageWhenViewingMessagePage()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Receive a new message when viewing message page", ((string[])(null)));
#line 118
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 119
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 120
 testRunner.And("I am viewing messages");
#line 121
 testRunner.When("I receive message number \'1\'");
#line 122
 testRunner.Then("I should see \'1\' message(s) in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Open unread message where text reply is allowed")]
        public virtual void OpenUnreadMessageWhereTextReplyIsAllowed()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Open unread message where text reply is allowed", ((string[])(null)));
#line 126
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 127
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table13 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table13.AddRow(new string[] {
                        "Title",
                        "New message"});
            table13.AddRow(new string[] {
                        "Message",
                        "Text in message"});
            table13.AddRow(new string[] {
                        "Text reply allowed",
                        "True"});
#line 128
 testRunner.And("I have an unread message with", ((string)(null)), table13);
#line 133
 testRunner.And("I am viewing messages");
#line 134
 testRunner.When("I click on the message at position \'1\' in the list");
#line 135
 testRunner.Then("I should see the message details form with an editable text box");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("See reply dialogue in message text")]
        public virtual void SeeReplyDialogueInMessageText()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("See reply dialogue in message text", ((string[])(null)));
#line 137
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 138
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table14 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table14.AddRow(new string[] {
                        "Title",
                        "Work late"});
            table14.AddRow(new string[] {
                        "Message",
                        "Can u work late today?"});
            table14.AddRow(new string[] {
                        "Text reply allowed",
                        "True"});
            table14.AddRow(new string[] {
                        "My reply",
                        "Ok if you buy me dinner?"});
            table14.AddRow(new string[] {
                        "Senders reply",
                        "It´s a deal!"});
#line 139
 testRunner.And("I have an unread message with", ((string)(null)), table14);
#line 146
 testRunner.And("I am viewing messages");
#line 147
 testRunner.When("I click on the message at position \'1\' in the list");
#line hidden
            TechTalk.SpecFlow.Table table15 = new TechTalk.SpecFlow.Table(new string[] {
                        "Messages"});
            table15.AddRow(new string[] {
                        "Ok if you buy me dinner?"});
            table15.AddRow(new string[] {
                        "It´s a deal!"});
#line 148
 testRunner.Then("I should see this conversation", ((string)(null)), table15);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Do not allow empty reply")]
        public virtual void DoNotAllowEmptyReply()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Do not allow empty reply", ((string[])(null)));
#line 153
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 154
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table16 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table16.AddRow(new string[] {
                        "Title",
                        "New message"});
            table16.AddRow(new string[] {
                        "Message",
                        "Text in message"});
            table16.AddRow(new string[] {
                        "Text reply allowed",
                        "True"});
#line 155
 testRunner.And("I have an unread message with", ((string)(null)), table16);
#line 160
 testRunner.And("I am viewing messages");
#line 161
 testRunner.When("I click on the message at position \'1\' in the list");
#line 162
 testRunner.Then("the send button should be disabled");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Send text reply message")]
        public virtual void SendTextReplyMessage()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Send text reply message", ((string[])(null)));
#line 164
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 165
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table17 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table17.AddRow(new string[] {
                        "Title",
                        "New message"});
            table17.AddRow(new string[] {
                        "Message",
                        "Text in message"});
            table17.AddRow(new string[] {
                        "Text reply allowed",
                        "True"});
#line 166
 testRunner.And("I have an unread message with", ((string)(null)), table17);
#line 171
 testRunner.And("I am viewing messages");
#line 172
 testRunner.And("I click on the message at position \'1\' in the list");
#line 173
 testRunner.When("I enter the text reply \'my reply\'");
#line 174
 testRunner.And("I click the send button");
#line 175
 testRunner.Then("I should not see any messages");
#line 176
 testRunner.And("I should see a user-friendly message explaining I dont have any messages");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Do not allow replies that are too long")]
        public virtual void DoNotAllowRepliesThatAreTooLong()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Do not allow replies that are too long", ((string[])(null)));
#line 178
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 179
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table18 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table18.AddRow(new string[] {
                        "Title",
                        "New message"});
            table18.AddRow(new string[] {
                        "Message",
                        "Text in message"});
            table18.AddRow(new string[] {
                        "Text reply allowed",
                        "True"});
#line 180
 testRunner.And("I have an unread message with", ((string)(null)), table18);
#line 185
 testRunner.And("I am viewing messages");
#line 186
 testRunner.And("I click on the message at position \'1\' in the list");
#line 187
 testRunner.When("I write a text reply that is too long");
#line 188
 testRunner.Then("the send button should be disabled");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show remaining characters when writing text reply")]
        public virtual void ShowRemainingCharactersWhenWritingTextReply()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show remaining characters when writing text reply", ((string[])(null)));
#line 190
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 191
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table19 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table19.AddRow(new string[] {
                        "Title",
                        "New message"});
            table19.AddRow(new string[] {
                        "Message",
                        "Text in message"});
            table19.AddRow(new string[] {
                        "Text reply allowed",
                        "True"});
#line 192
 testRunner.And("I have an unread message with", ((string)(null)), table19);
#line 197
 testRunner.And("I am viewing messages");
#line 198
 testRunner.And("I click on the message at position \'1\' in the list");
#line 199
 testRunner.When("I enter the text reply \'my reply\'");
#line 200
 testRunner.Then("I should see that I have \'242\' characters left");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
