using System;
using System.Globalization;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using Teleopti.Interfaces.Domain;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class AsmMessageStepDefinition
	{
		private MessagePage _page { get { return Pages.Pages.MessagePage; } }

		[Then(@"Message tab should be visible")]
		public void ThenMessageTabShouldBeVisible()
		{
			EventualAssert.That(() => _page.MessageLink.Exists, Is.True);
		}

		[Then(@"Message tab should not be visible")]
		public void ThenMessageTabShouldNotBeVisible()
		{
			EventualAssert.That(() => _page.MessageLink.Exists, Is.False);
		}

		[Given(@"I have an unread message with")]
		public void GivenIHaveAnUnreadMessageWith(Table table)
		{
			var message = table.CreateInstance<MessageConfigurable>();
			UserFactory.User().Setup(message);
		}

		[Given(@"I should be notified that I have '(.*)' unread message\(s\)")]
		[Then(@"I should be notified that I have '(.*)' unread message\(s\)")]
		public void ThenIShouldBeNotifiedThatIHaveUnreadMessageS(string unreadMessageCount)
		{
			int parseResult;
			if (int.TryParse(unreadMessageCount, out parseResult))
				EventualAssert.That(() => _page.MessageLink.ClassName.Contains("asm-new-message-indicator"), Is.True);
			if (parseResult > 0)
			{
				EventualAssert.That(() => _page.MessageLink.InnerHtml.Contains("(" + parseResult + ")"), Is.True);
			}
			else
			{
				EventualAssert.That(() => _page.MessageLink.InnerHtml.Contains("("), Is.False);
				EventualAssert.That(() => _page.MessageLink.InnerHtml.Contains(")"), Is.False);
			}
		}

		[When(@"I receive message number '(.*)' while not viewing message page")]
		public void WhenIReceiveMessageNumberWhileNotViewingMessagePage(int messageCount)
		{
			Browser.Interactions.Javascript("Teleopti.MyTimeWeb.AsmMessage.SetMessageNotificationOnTab(" + messageCount.ToString() + ");");
		}

		[When(@"I receive message number '(.*)'")]
		public void WhenIReceiveMessageNumber(int messageCount)
		{
			Browser.Interactions.AssertExists(".asmMessage-list");
			Browser.Interactions.Javascript("Teleopti.MyTimeWeb.AsmMessage.SetMessageNotificationOnTab(" + messageCount.ToString() + ");");

			var pushMessageDialogueJsonObject = new StringBuilder();
			pushMessageDialogueJsonObject.Append("var messageItem = {");
			pushMessageDialogueJsonObject.AppendFormat(CultureInfo.InvariantCulture, "MessageId: '{0}', ", Guid.NewGuid());
			pushMessageDialogueJsonObject.Append("Title: 'My title', ");
			pushMessageDialogueJsonObject.Append("Message: 'My message', ");
			pushMessageDialogueJsonObject.Append("Message: 'My message', ");
			pushMessageDialogueJsonObject.Append("Sender: 'My sender', ");
			pushMessageDialogueJsonObject.AppendFormat(CultureInfo.InvariantCulture, "Date: '{0} {1}', ", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
			pushMessageDialogueJsonObject.Append("IsRead: 'false'}; ");
			pushMessageDialogueJsonObject.Append("Teleopti.MyTimeWeb.AsmMessageList.AddNewMessageAtTop(messageItem);");

			Browser.Interactions.Javascript(pushMessageDialogueJsonObject.ToString());
		}

		[Given(@"I have no unread messages")]
		public void GivenIHaveNoUnreadMessages()
		{
			//For helping system tester to understand
		}

		[Given(@"I am viewing messages")]
		[When(@"I am viewing messages")]
		public void WhenIAmViewingMessages()
		{
			TestControllerMethods.Logon();
			Navigation.GotoMessagePage();
		}

		[Then(@"I should not see any messages")]
		public void ThenIShouldNotSeeAnyMessages()
		{
			EventualAssert.That(() => _page.MessageListItems.Count, Is.EqualTo(0));
			Browser.Current.Eval("Teleopti.MyTimeWeb.AsmMessage.SetMessageNotificationOnTab(0)");
		}

		[Then(@"I should see '(.*)' message\(s\) in the list")]
		public void ThenIShouldSeeMessageSInTheList(int messageCount)
		{
			EventualAssert.That(() => _page.MessageListItems.Count, Is.EqualTo(messageCount));
		}

		[Then(@"I should see the message details form with")]
		public void ThenIShouldSeeTheMessageDetailsFormWith(Table table)
		{
			EventualAssert.That(() => Pages.Pages.CurrentMessageReplyPage.MessageDetailSection.DisplayVisible(), Is.True);
			EventualAssert.That(() => Pages.Pages.CurrentMessageReplyPage.Title.InnerHtml.Contains(table.Rows[0][1]), Is.True);
			EventualAssert.That(() => Pages.Pages.CurrentMessageReplyPage.Message.InnerHtml.Contains(table.Rows[1][1]), Is.True);
		}

		[When(@"I click on the message at position '(.*)' in the list")]
		public void GivenIClickOnTheMessageAtPositionInTheList(int position)
		{
			EventualAssert.That(() => _page.MessageListItems.Count, Is.EqualTo(1));
			_page.MessageListItems[position-1].Click();
		}
		
		[When(@"I click the confirm button")]
		public void WhenIClickTheConfirmButton()
		{
			Pages.Pages.CurrentOkButton.OkButton.EventualClick();
		}

		[When(@"I confirm reading the message at position '(.*)' of '(.*)' in the list")]
		public void WhenIConfirmReadingTheMessageAtPositionInTheList(int listPosition, int messageCount)
		{
			Browser.Interactions.AssertExists(".asmMessage-item");

			var newMessageCount = messageCount - 1;
			EventualAssert.That(() => _page.MessageListItems.Count, Is.EqualTo(messageCount));
			_page.MessageListItems[listPosition - 1].Click();
			Pages.Pages.CurrentOkButton.OkButton.EventualClick();
			Browser.Current.Eval("Teleopti.MyTimeWeb.AsmMessage.SetMessageNotificationOnTab(" + newMessageCount.ToString() + ");");
		}

		[Then(@"I should see a user-friendly message explaining I dont have any messages")]
		public void ThenIShouldSeeAUser_FriendlyMessageExplainingIDontHaveAnyMessages()
		{
			EventualAssert.That(() => _page.FriendlyMessage.Style.GetAttributeValue("display"), Is.Not.EqualTo("none"));
		}

		[When(@"I enter the text reply '(.*)'")]
		public void WhenIEnterTheTextReply(string reply)
		{
			const string js = @"Teleopti.MyTimeWeb.AsmMessageList.AddReplyText('{0}');";
			Browser.Current.Eval(string.Format(js, reply));
		}

		[Then(@"I should see the message details form with an editable text box")]
		public void ThenIShouldSeeTheMessageDetailsFormWithAnEditableTextBox()
		{
			EventualAssert.That(() => Pages.Pages.CurrentMessageReplyPage.Reply.Style.GetAttributeValue("display"), Is.Not.EqualTo("none"));
		}

		[Then(@"I should see this conversation")]
		public void ThenIShouldSeeThisConversation(Table table)
		{
			foreach (var tableRow in table.Rows)
			{
				EventualAssert.That(() => Pages.Pages.CurrentMessageReplyPage.DialogueMessages.InnerHtml.Contains(tableRow[0]), Is.EqualTo(true));						
			}
		}

		[Then(@"the send button should be disabled")]
		public void ThenTheSendButtonShouldBeDisabled()
		{
			EventualAssert.That(() => Pages.Pages.CurrentOkButton.OkButton.Enabled, Is.False);
		}

		[Then(@"the send button should be enabled")]
		public void ThenTheSendButtonShouldBeEnabled()
		{
			EventualAssert.That(() => Pages.Pages.CurrentOkButton.OkButton.Enabled, Is.True);
		}

		[When(@"I click the radiobutton with caption '(.*)'")]
		public void WhenIClickTheRadiobuttonWithCaption(string option)
		{
			var label = Pages.Pages.CurrentMessageReplyPage.ReplyOptions.Labels.First(r => r.Text.Equals(option));
			var indexOfLabel = Pages.Pages.CurrentMessageReplyPage.ReplyOptions.Labels.ToList().IndexOf(label);
			Pages.Pages.CurrentMessageReplyPage.ReplyOptions.RadioButtons.ElementAt(indexOfLabel).EventualClick();
		}

		[Then(@"the radiobutton with caption '(.*)' should not be checked")]
		public void ThenTheRadiobuttonWithCaptionShouldNotBeChecked(string option)
		{
			var label = Pages.Pages.CurrentMessageReplyPage.ReplyOptions.Labels.First(r => r.Text.Equals(option));
			var indexOfLabel = Pages.Pages.CurrentMessageReplyPage.ReplyOptions.Labels.ToList().IndexOf(label);
			EventualAssert.That(()=>Pages.Pages.CurrentMessageReplyPage.ReplyOptions.RadioButtons.ElementAt(indexOfLabel).Checked,Is.False);
		}

		[Then(@"I should see radiobuttons with")]
		public void ThenIShouldSeeRadiobuttonsWith(Table table)
		{
			foreach (var tableRow in table.Rows)
			{
				EventualAssert.That(() => Pages.Pages.CurrentMessageReplyPage.ReplyOptions.InnerHtml.Contains(tableRow[0]), Is.EqualTo(true));
			}	
		}

		[Then(@"I should not see any options")]
		public void ThenIShouldNotSeeAnyOptions()
		{
			foreach (var radioButton in Pages.Pages.CurrentMessageReplyPage.ReplyOptions.RadioButtons)
			{
				Assert.That(IsDisplayed(radioButton), Is.False);
			}
		}

		private static bool IsDisplayed(Element element)
		{
			if(string.Equals(element.Style.Display,"none"))
			{
				return false;
			}
			if(element.Parent!=null)
			{
				return IsDisplayed(element.Parent);
			}
			return true;
		}

		[When(@"the message with title '(.*)' is deleted by the sender")]
		public void WhenTheMessageWithTitleIsDeletedByTheSender(string title)
		{
			Browser.Interactions.AssertExists(".asmMessage-list");
			Guid id;
			IPushMessageDialogue pushMessageDialogue;
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var repository = new PushMessageDialogueRepository(uow);
				pushMessageDialogue = repository.LoadAll().First(m => m.PushMessage.GetTitle(new NoFormatting()).Equals(title));
				id = (Guid)pushMessageDialogue.Id;
			}

			Browser.Interactions.Javascript("Teleopti.MyTimeWeb.AsmMessage.SetMessageNotificationOnTab(" + "1" + ");");
			var javaScript = new StringBuilder();

			javaScript.AppendFormat(CultureInfo.InvariantCulture, "Teleopti.MyTimeWeb.AsmMessageList.DeleteMessage( '{0}' );", id.ToString());

			Browser.Interactions.Javascript(javaScript.ToString());
		}
	}
}