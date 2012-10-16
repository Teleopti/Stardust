using System;
using System.Globalization;
using System.Text;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Teleopti.Ccc.WebBehaviorTest.Pages;
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
            Browser.Current.Eval("Teleopti.MyTimeWeb.AsmMessage.SetMessageNotificationOnTab(" + messageCount.ToString() + ");");
        }

        [When(@"I receive message number '(.*)'")]
        public void WhenIReceiveMessageNumber(int messageCount)
        {
			Browser.Current.Eval("Teleopti.MyTimeWeb.AsmMessage.SetMessageNotificationOnTab(" + messageCount.ToString() + ");");

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

			Browser.Current.Eval(pushMessageDialogueJsonObject.ToString());
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

        [Given(@"message tab indicates '(.*)' new message\(s\)")]
        [Then(@"message tab indicates '(.*)' new message\(s\)")]
        public void ThenMessageTabIndicatesNewMessageS(string unreadMessageCount)
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

        [Given(@"I should see the message details form with")]
        [Then(@"I should see the message details form with")]
        public void ThenIShouldSeeTheMessageDetailsFormWith(Table table)
        {
            EventualAssert.That(() => Pages.Pages.CurrentMessageReplyPage.MessageDetailSection.DisplayVisible(), Is.True);
			EventualAssert.That(() => Pages.Pages.CurrentMessageReplyPage.Title.InnerHtml.Contains(table.Rows[0][1]), Is.True);
			EventualAssert.That(() => Pages.Pages.CurrentMessageReplyPage.Message.InnerHtml.Contains(table.Rows[1][1]), Is.True);
        }

        [Then(@"I should see the message with title '(.*)' at position '(.*)' in the list")]
        public void ThenIShouldSeeTheMessageWithTitleAtPositionInTheList(string title, int listPosition)
        {
            EventualAssert.That(() => _page.MessageListItems[listPosition-1].InnerHtml.Contains(title), Is.True);
        }

        [Given(@"I click on the message at position '(.*)' in the list")]
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
	}
}