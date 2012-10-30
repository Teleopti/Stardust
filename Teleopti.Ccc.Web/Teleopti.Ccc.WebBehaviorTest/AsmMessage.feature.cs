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
#line 49
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 50
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 51
 testRunner.And("I have no unread messages");
#line 52
 testRunner.And("I am viewing week schedule");
#line 53
 testRunner.When("I navigate to messages");
#line 54
 testRunner.Then("I should not see any messages");
#line 55
 testRunner.And("I should see a user-friendly message explaining I dont have any messages");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View unread messages")]
        public virtual void ViewUnreadMessages()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View unread messages", ((string[])(null)));
#line 57
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 58
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table5.AddRow(new string[] {
                        "Title",
                        "New message"});
#line 59
 testRunner.And("I have an unread message with", ((string)(null)), table5);
#line 62
 testRunner.When("I am viewing messages");
#line 63
 testRunner.Then("I should see \'1\' message(s) in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Open unread message")]
        public virtual void OpenUnreadMessage()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Open unread message", ((string[])(null)));
#line 65
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 66
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
#line 67
 testRunner.And("I have an unread message with", ((string)(null)), table6);
#line 71
 testRunner.When("I am viewing messages");
#line 72
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
#line 73
 testRunner.Then("I should see the message details form with", ((string)(null)), table7);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Confirm message is read")]
        public virtual void ConfirmMessageIsRead()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Confirm message is read", ((string[])(null)));
#line 78
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 79
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table8.AddRow(new string[] {
                        "Title",
                        "New message"});
#line 80
 testRunner.And("I have an unread message with", ((string)(null)), table8);
#line 83
 testRunner.When("I am viewing messages");
#line 84
 testRunner.And("I click on the message at position \'1\' in the list");
#line 85
 testRunner.When("I click the confirm button");
#line 86
 testRunner.Then("I should not see any messages");
#line 87
 testRunner.And("I should see a user-friendly message explaining I dont have any messages");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Sort messages in list by latest message")]
        public virtual void SortMessagesInListByLatestMessage()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Sort messages in list by latest message", ((string[])(null)));
#line 89
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 90
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
#line 91
 testRunner.And("I have an unread message with", ((string)(null)), table9);
#line hidden
            TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table10.AddRow(new string[] {
                        "Title",
                        "Latest message"});
#line 95
 testRunner.And("I have an unread message with", ((string)(null)), table10);
#line 98
 testRunner.When("I am viewing messages");
#line 99
 testRunner.Then("I should see the message with title \'Latest message\' at position \'1\' in the list");
#line 100
 testRunner.And("I should see the message with title \'Message\' at position \'2\' in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Reduce number of unread messages in message tab title")]
        public virtual void ReduceNumberOfUnreadMessagesInMessageTabTitle()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Reduce number of unread messages in message tab title", ((string[])(null)));
#line 102
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 103
 testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table11.AddRow(new string[] {
                        "Title",
                        "New message"});
#line 104
 testRunner.And("I have an unread message with", ((string)(null)), table11);
#line hidden
            TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table12.AddRow(new string[] {
                        "Title",
                        "Another new message"});
#line 107
 testRunner.And("I have an unread message with", ((string)(null)), table12);
#line 110
 testRunner.And("I am viewing week schedule");
#line 111
 testRunner.And("I should be notified that I have \'2\' unread message(s)");
#line 112
 testRunner.When("I navigate to messages");
#line 113
 testRunner.And("I confirm reading the message at position \'1\' of \'2\' in the list");
#line 114
 testRunner.Then("I should be notified that I have \'1\' unread message(s)");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Receive a new message when viewing message page")]
        public virtual void ReceiveANewMessageWhenViewingMessagePage()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Receive a new message when viewing message page", ((string[])(null)));
#line 116
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 117
 testRunner.Given("I have the role \'Full access to mytime\'");
#line 118
 testRunner.And("I am viewing messages");
#line 119
 testRunner.When("I receive message number \'1\'");
#line 120
 testRunner.Then("I should see \'1\' message(s) in the list");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Open unread message where text reply is allowed")]
        public virtual void OpenUnreadMessageWhereTextReplyIsAllowed()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Open unread message where text reply is allowed", ((string[])(null)));
#line 122
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 123
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
#line 124
 testRunner.And("I have an unread message with", ((string)(null)), table13);
#line 129
 testRunner.And("I am viewing messages");
#line 130
 testRunner.When("I click on the message at position \'1\' in the list");
#line 131
 testRunner.Then("I should see the message details form with an editable text box");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("See reply dialogue in message text")]
        public virtual void SeeReplyDialogueInMessageText()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("See reply dialogue in message text", ((string[])(null)));
#line 133
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 134
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
#line 135
 testRunner.And("I have an unread message with", ((string)(null)), table14);
#line 142
 testRunner.And("I am viewing messages");
#line 143
 testRunner.When("I click on the message at position \'1\' in the list");
#line hidden
            TechTalk.SpecFlow.Table table15 = new TechTalk.SpecFlow.Table(new string[] {
                        "Messages"});
            table15.AddRow(new string[] {
                        "Ok if you buy me dinner?"});
            table15.AddRow(new string[] {
                        "It´s a deal!"});
#line 144
 testRunner.Then("I should see this conversation", ((string)(null)), table15);
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Do not allow empty reply")]
        public virtual void DoNotAllowEmptyReply()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Do not allow empty reply", ((string[])(null)));
#line 149
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 150
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
#line 151
 testRunner.And("I have an unread message with", ((string)(null)), table16);
#line 156
 testRunner.And("I am viewing messages");
#line 157
 testRunner.When("I click on the message at position \'1\' in the list");
#line 158
 testRunner.Then("the send button should be disabled");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Send text reply message")]
        public virtual void SendTextReplyMessage()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Send text reply message", ((string[])(null)));
#line 160
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 161
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
#line 162
 testRunner.And("I have an unread message with", ((string)(null)), table17);
#line 167
 testRunner.And("I am viewing messages");
#line 168
 testRunner.And("I click on the message at position \'1\' in the list");
#line 169
 testRunner.When("I enter the text reply \'my reply\'");
#line 170
 testRunner.And("I click the confirm button");
#line 171
 testRunner.Then("I should not see any messages");
#line 172
 testRunner.And("I should see a user-friendly message explaining I dont have any messages");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Show replyoptions for message with multiple options")]
        public virtual void ShowReplyoptionsForMessageWithMultipleOptions()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show replyoptions for message with multiple options", ((string[])(null)));
#line 174
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 175
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
                        "False"});
            table18.AddRow(new string[] {
                        "ReplyOption1",
                        "Yes"});
            table18.AddRow(new string[] {
                        "ReplyOption2",
                        "No"});
#line 176
 testRunner.And("I have an unread message with", ((string)(null)), table18);
#line 183
 testRunner.And("I am viewing messages");
#line 184
 testRunner.When("I click on the message at position \'1\' in the list");
#line 185
 testRunner.Then("I should see a radiobutton with caption \'Yes\'");
#line 186
 testRunner.And("I should see a radiobutton with caption \'No\'");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Confirm message with multiple replyoptions")]
        public virtual void ConfirmMessageWithMultipleReplyoptions()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Confirm message with multiple replyoptions", ((string[])(null)));
#line 188
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 189
testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table19 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table19.AddRow(new string[] {
                        "Title",
                        "Ashley is ill"});
            table19.AddRow(new string[] {
                        "Message",
                        "Can you work tomorrow?"});
            table19.AddRow(new string[] {
                        "ReplyOption2",
                        "Probably"});
            table19.AddRow(new string[] {
                        "ReplyOption2",
                        "Probably not"});
            table19.AddRow(new string[] {
                        "ReplyOption3",
                        "Defenitly not"});
#line 190
 testRunner.And("I have an unread message with", ((string)(null)), table19);
#line 197
 testRunner.And("I am viewing messages");
#line 198
 testRunner.When("I click on the message at position \'1\' in the list");
#line 199
 testRunner.And("I click the radiobutton with caption \'Probably not\'");
#line 200
 testRunner.And("I click the confirm button");
#line 201
 testRunner.Then("I should not see any messages");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Enable confirmbutton when user has selceted a replyoption")]
        public virtual void EnableConfirmbuttonWhenUserHasSelcetedAReplyoption()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Enable confirmbutton when user has selceted a replyoption", ((string[])(null)));
#line 203
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 204
testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table20 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table20.AddRow(new string[] {
                        "Title",
                        "New message"});
            table20.AddRow(new string[] {
                        "Message",
                        "Text in message"});
            table20.AddRow(new string[] {
                        "Text reply allowed",
                        "False"});
            table20.AddRow(new string[] {
                        "ReplyOption1",
                        "Yes"});
            table20.AddRow(new string[] {
                        "ReplyOption2",
                        "No"});
#line 205
 testRunner.And("I have an unread message with", ((string)(null)), table20);
#line 212
 testRunner.And("I am viewing messages");
#line 213
 testRunner.When("I click on the message at position \'1\' in the list");
#line 214
 testRunner.And("I click the radiobutton with caption \'No\'");
#line 215
 testRunner.Then("the send button should be enabled");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Confirmbutton should be disabled when user hasnt selected a replyoption")]
        public virtual void ConfirmbuttonShouldBeDisabledWhenUserHasntSelectedAReplyoption()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Confirmbutton should be disabled when user hasnt selected a replyoption", ((string[])(null)));
#line 217
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 218
testRunner.Given("I have the role \'Full access to mytime\'");
#line hidden
            TechTalk.SpecFlow.Table table21 = new TechTalk.SpecFlow.Table(new string[] {
                        "Field",
                        "Value"});
            table21.AddRow(new string[] {
                        "Title",
                        "New message"});
            table21.AddRow(new string[] {
                        "Message",
                        "Text in message"});
            table21.AddRow(new string[] {
                        "Text reply allowed",
                        "False"});
            table21.AddRow(new string[] {
                        "ReplyOption1",
                        "Yes"});
            table21.AddRow(new string[] {
                        "ReplyOption2",
                        "No"});
#line 219
 testRunner.And("I have an unread message with", ((string)(null)), table21);
#line 226
 testRunner.And("I am viewing messages");
#line 227
 testRunner.When("I click on the message at position \'1\' in the list");
#line 228
 testRunner.Then("the send button should be disabled");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
