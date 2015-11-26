﻿using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Interfaces.Domain;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using BrowserInteractionsJQueryExtensions = Teleopti.Ccc.TestCommon.Web.StartWeb.BrowserDriver.BrowserInteractionsJQueryExtensions;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class AsmMessageStepDefinition
	{
		[Then(@"Message tab should be visible")]
		public void ThenMessageTabShouldBeVisible()
		{
			Browser.Interactions.AssertExists("[href*='#MessageTab']");
		}

		[Then(@"Message tab should not be visible")]
		public void ThenMessageTabShouldNotBeVisible()
		{
			Browser.Interactions.AssertNotExists(".container", "[href*='#MessageTab']");
		}

		[Given(@"I have an unread message with")]
		public void GivenIHaveAnUnreadMessageWith(Table table)
		{
			var message = table.CreateInstance<MessageConfigurable>();
			DataMaker.Data().Apply(message);
		}

		[Given(@"I should be notified that I have '(.*)' unread message\(s\)")]
		[Then(@"I should be notified that I have '(.*)' unread message\(s\)")]
		public void ThenIShouldBeNotifiedThatIHaveUnreadMessageS(int unreadMessageCount)
		{
			if (unreadMessageCount == 0)
				Browser.Interactions.AssertNotExists(".container", "[href*='#MessageTab'] span.badge");
			else
				Browser.Interactions.AssertFirstContains("[href*='#MessageTab'] span.badge", unreadMessageCount.ToString(CultureInfo.InvariantCulture));
		}

		[When(@"I receive message number '(.*)' while not viewing message page")]
		public void WhenIReceiveMessageNumberWhileNotViewingMessagePage(int messageCount)
		{
			Browser.Interactions.Javascript("Teleopti.MyTimeWeb.AsmMessage.SetMessageNotificationOnTab(" + messageCount.ToString(CultureInfo.InvariantCulture) + ");");
		}

		[When(@"I receive message number '(.*)'")]
		public void WhenIReceiveMessageNumber(int messageCount)
		{
			Browser.Interactions.AssertExists(".message-list");
			Browser.Interactions.Javascript("Teleopti.MyTimeWeb.AsmMessage.SetMessageNotificationOnTab(" + messageCount.ToString(CultureInfo.InvariantCulture) + ");");

			var pushMessageDialogueJsonObject = new StringBuilder();
			pushMessageDialogueJsonObject.Append("var messageItem = {");
			pushMessageDialogueJsonObject.AppendFormat(CultureInfo.InvariantCulture, "MessageId: '{0}', ", Guid.NewGuid());
			pushMessageDialogueJsonObject.Append("Title: 'My title', ");
			pushMessageDialogueJsonObject.Append("Message: 'My message', ");
			pushMessageDialogueJsonObject.Append("Message: 'My message', ");
			pushMessageDialogueJsonObject.Append("Sender: 'My sender', ");
            pushMessageDialogueJsonObject.AppendFormat(CultureInfo.InvariantCulture, "Date: '{0} {1}', ", DateOnlyForBehaviorTests.TestToday.ToShortDateString(), DateTime.Now.ToShortTimeString());
			pushMessageDialogueJsonObject.Append("IsRead: 'false'}; ");
			pushMessageDialogueJsonObject.Append("Teleopti.MyTimeWeb.AsmMessageList.AddNewMessageAtTop(messageItem);");

			Browser.Interactions.Javascript(pushMessageDialogueJsonObject.ToString());
		}

		[Then(@"I should not see any messages")]
		public void ThenIShouldNotSeeAnyMessages()
		{
			Browser.Interactions.AssertNotExists(".container", ".header");
			Browser.Interactions.Javascript("Teleopti.MyTimeWeb.AsmMessage.SetMessageNotificationOnTab(0)");
		}

		[Then(@"I should see '(.*)' message\(s\) in the list")]
		public void ThenIShouldSeeMessageSInTheList(int messageCount)
		{
			Browser.Interactions.AssertJavascriptResultContains("return $(\".header\").length;", messageCount.ToString(CultureInfo.InvariantCulture));
		}

		[Then(@"I should see the message detail form with the message '(.*)'")]
		public void ThenIShouldSeeTheMessageDetailFormWithTheMessage(string messageText)
		{
			BrowserInteractionsJQueryExtensions.AssertVisibleUsingJQuery(Browser.Interactions, ".detail-form");
			BrowserInteractionsJQueryExtensions.AssertFirstContainsUsingJQuery(Browser.Interactions, ".detail-form .message-text", messageText);
		}

		[When(@"I click on the message with the title '(.*)'")]
		public void WhenIClickOnTheMessageWithTheTitle(string messsageTitle)
		{
			BrowserInteractionsJQueryExtensions.ClickUsingJQuery(Browser.Interactions, string.Format(CultureInfo.InvariantCulture, ".message .title:contains('{0}')", messsageTitle));
		}


		[When(@"I confirm reading the message with the title '(.*)'")]
		public void WhenIConfirmReadingTheMessageWithTheTitle(string messageTitle)
		{
			BrowserInteractionsJQueryExtensions.ClickUsingJQuery(Browser.Interactions, string.Format(CultureInfo.InvariantCulture,
			                                         ".message:has(.title:contains('{0}')) .confirm-read", messageTitle));
		}

		[Then(@"I should see a user-friendly message explaining I dont have any messages")]
		public void ThenIShouldSeeAUser_FriendlyMessageExplainingIDontHaveAnyMessages()
		{
			BrowserInteractionsJQueryExtensions.AssertVisibleUsingJQuery(Browser.Interactions, ".no-messages");
		}

		[When(@"I enter the text reply '(.*)'")]
		public void WhenIEnterTheTextReply(string reply)
		{
			const string js = @"Teleopti.MyTimeWeb.AsmMessageList.AddReplyText('{0}');";
			Browser.Interactions.Javascript(string.Format(js, reply));
		}

		[Then(@"I should be able to write a text reply for the message with the title '(.*)'")]
		public void ThenIShouldBeAbleToWriteATextReplyForTheMessageWithTheTitle(string messageTitle)
		{
			BrowserInteractionsJQueryExtensions.AssertVisibleUsingJQuery(Browser.Interactions, string.Format(CultureInfo.InvariantCulture,
																		".message:has(.title:contains('{0}')) .text-reply",
			                                                            messageTitle));
		}

		[Then(@"I can see a conversation for the message with the title '(.*)' with")]
		public void ThenICanSeeAConversationForTheMessageWithTheTitleWith(string messageTitle, Table table)
		{
			var dialogue1 = table.Rows[0][0];
			var dialogue2 = table.Rows[1][0];
			const string selector = ".message:has(.title:contains('{0}')) .dialogue > span:contains('{1}')";
			BrowserInteractionsJQueryExtensions.AssertExistsUsingJQuery(Browser.Interactions, string.Format(CultureInfo.InvariantCulture, selector, messageTitle, dialogue1));
			BrowserInteractionsJQueryExtensions.AssertExistsUsingJQuery(Browser.Interactions, string.Format(CultureInfo.InvariantCulture, selector, messageTitle, dialogue2));
		}

		[Then(@"I should not be able to send response for the message with the title '(.*)'")]
		public void ThenTheSendButtonShouldBeDisabledForTheMessageWithTheTitle(string messageTitle)
		{
			BrowserInteractionsJQueryExtensions.AssertExistsUsingJQuery(Browser.Interactions, string.Format(CultureInfo.InvariantCulture,
																		".message:has(.title:contains('{0}')) .confirm-read:disabled",
																		messageTitle));
		}

		[Then(@"I should be able to send response for the message with the title '(.*)'")]
		public void ThenTheSendButtonShouldBeEnabledForTheMessageWithTheTitle(string messageTitle)
		{
			BrowserInteractionsJQueryExtensions.AssertExistsUsingJQuery(Browser.Interactions, string.Format(CultureInfo.InvariantCulture,
																		".message:has(.title:contains('{0}')) .confirm-read:not(disabled)",
																		messageTitle));
		}

		[When(@"I choose reply option '(.*)' for the message with the title '(.*)'")]
		public void WhenIChooseReplyOptionForTheMessageWithTheTitle(string replyOption, string messageTitle)
		{
			string selector = string.Format(CultureInfo.InvariantCulture, ".message:has(.title:contains('{0}')) .message-option:contains('{1}')", messageTitle, replyOption);
			BrowserInteractionsJQueryExtensions.ClickUsingJQuery(Browser.Interactions, selector);
		}

		[Then(@"I should be able to select one of the following options for the message with the title '(.*)'")]
		public void ThenIShouldBeAbleToSelectOneOfTheFollowingOptionsForTheMessageWithTheTitle(string messageTitle, Table table)
		{
			for (int i = 0; i < table.RowCount; i++)
			{
				BrowserInteractionsJQueryExtensions.AssertFirstContainsUsingJQuery(Browser.Interactions, string.Format(CultureInfo.InvariantCulture,
																	   ".message:has(.title:contains('{0}')) .message-option:nth-child({1})",
																	   messageTitle, i + 1),
																	   table.Rows[i][0]);
			}
		}

		[Then(@"I should not see any options for the message with the title '(.*)'")]
		public void ThenIShouldNotSeeAnyOptionsForTheMessageWithTheTitle(string messageTitle)
		{
			BrowserInteractionsJQueryExtensions.AssertNotVisibleUsingJQuery(Browser.Interactions, string.Format(CultureInfo.InvariantCulture,
																	   ".message:has(.title:contains('{0}')) .message-option",
																	   messageTitle));
		}
		
		[When(@"the message with title '(.*)' is deleted by the sender")]
		public void WhenTheMessageWithTitleIsDeletedByTheSender(string title)
		{
			Browser.Interactions.AssertExists(".message-list");
			Guid id;
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var repository = new PushMessageDialogueRepository(uow);
				IPushMessageDialogue pushMessageDialogue = repository.LoadAll().First(m => m.PushMessage.GetTitle(new NoFormatting()).Equals(title));
				id = pushMessageDialogue.Id.GetValueOrDefault();
			}

			Browser.Interactions.Javascript("Teleopti.MyTimeWeb.AsmMessage.SetMessageNotificationOnTab(" + "1" + ");");
			
			Thread.Sleep(400); //wait for making sure the message has been added
			var javaScript = new StringBuilder();
			javaScript.AppendFormat(CultureInfo.InvariantCulture, "Teleopti.MyTimeWeb.AsmMessageList.DeleteMessage( '{0}' );", id.ToString());

			Browser.Interactions.Javascript(javaScript.ToString());
		}

		[Then(@"the reply option '(.*)' should not be selected for the message with the title '(.*)'")]
		public void ThenTheReplyOptionShouldNotBeSelectedForTheMessageWithTheTitle(string replyOption, string messageTitle)
		{
			BrowserInteractionsJQueryExtensions.AssertNotExistsUsingJQuery(Browser.Interactions, string.Format(CultureInfo.InvariantCulture, ".message:has(.title:contains('{0}')) .message-option:contains('{1}')",
				              messageTitle, replyOption),
				string.Format(CultureInfo.InvariantCulture, ".message:has(.title:contains('{0}')) .message-option.active:contains('{1}')",
				              messageTitle, replyOption));
		}
	}
}