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
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
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
			DataMaker.Data().Apply(message);
		}

		[Given(@"I should be notified that I have '(.*)' unread message\(s\)")]
		[Then(@"I should be notified that I have '(.*)' unread message\(s\)")]
		public void ThenIShouldBeNotifiedThatIHaveUnreadMessageS(string unreadMessageCount)
		{
			var messageCount = parseInt(unreadMessageCount);

			if (messageCount == 0)
				EventualAssert.That(() => Pages.Pages.CurrentPortalPage.MessageLink.Span(Find.BySelector(".badge")).Exists, Is.False);
			else
				EventualAssert.That(() => Pages.Pages.CurrentPortalPage.MessageLink.Span(Find.BySelector(".badge")).InnerHtml, Is.EqualTo(unreadMessageCount));
		}

		private int parseInt(string stringInt)
		{
			int parseResult;
			if (!int.TryParse(stringInt, NumberStyles.Number, CultureInfo.InvariantCulture, out parseResult))
				return 0;

			return parseResult;
		}

		[When(@"I receive message number '(.*)' while not viewing message page")]
		public void WhenIReceiveMessageNumberWhileNotViewingMessagePage(int messageCount)
		{
			Browser.Interactions.Javascript("Teleopti.MyTimeWeb.AsmMessage.SetMessageNotificationOnTab(" + messageCount.ToString() + ");");
		}

		[When(@"I receive message number '(.*)'")]
		public void WhenIReceiveMessageNumber(int messageCount)
		{
			Browser.Interactions.AssertExists("#AsmMessages-list");
			Browser.Interactions.Javascript("Teleopti.MyTimeWeb.AsmMessage.SetMessageNotificationOnTab(" + messageCount.ToString() + ");");

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
			EventualAssert.That(() => _page.MessageBodyDivs.Count, Is.EqualTo(0));
			Browser.Current.Eval("Teleopti.MyTimeWeb.AsmMessage.SetMessageNotificationOnTab(0)");
		}

		[Then(@"I should see '(.*)' message\(s\) in the list")]
		public void ThenIShouldSeeMessageSInTheList(int messageCount)
		{
			EventualAssert.That(() => _page.MessageBodyDivs.Count, Is.EqualTo(messageCount));
		}

		[Then(@"I should see the message details form with on the message at position '(.*)' in the list")]
		public void ThenIShouldSeeTheMessageDetailsFormWithOnTheMessageAtPositionInTheList(int position, Table table)
		{
			EventualAssert.That(() => Pages.Pages.CurrentMessageReplyPage.MessageDetailSection(position).DisplayVisible(), Is.True);
			EventualAssert.That(() => Pages.Pages.CurrentMessageReplyPage.Message(position).InnerHtml.Contains(table.Rows[0][1]), Is.True);
		}
		
		[When(@"I click on the message at position '(.*)' in the list")]
		public void GivenIClickOnTheMessageAtPositionInTheList(int position)
		{
			EventualAssert.That(() => _page.MessageBodyDivs.Count, Is.AtLeast(position));
			var messageBodyDiv = _page.MessageBodyDivs[position - 1].EventualGet();
			messageBodyDiv.EventualClick();
		}

		[When(@"I click the confirm button on the message at position '(.*)' in the list")]
		public void WhenIClickTheConfirmButtonOnTheMessageAtPositionInTheList(int position)
		{
			Pages.Pages.MessagePage.ConfirmButton(position).EventualClick();
		}

		[When(@"I confirm reading the message at position '(.*)' of '(.*)' in the list")]
		public void WhenIConfirmReadingTheMessageAtPositionInTheList(int listPosition, int messageCount)
		{
			Browser.Interactions.AssertExists("#AsmMessages-list");

			var newMessageCount = messageCount - 1;
			EventualAssert.That(() => _page.MessageBodyDivs.Count, Is.EqualTo(messageCount));
			_page.MessageBodyDivs[listPosition - 1].Click();
			Pages.Pages.MessagePage.ConfirmButton(listPosition).EventualClick();
			Browser.Current.Eval(string.Format(CultureInfo.InvariantCulture,
			                                   "Teleopti.MyTimeWeb.AsmMessage.SetMessageNotificationOnTab({0});", newMessageCount));
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

		[Then(@"I should see the message details form with an editable text box on the message at position '(.*)' in the list")]
		public void ThenIShouldSeeTheMessageDetailsFormWithAnEditableTextBoxOnTheMessageAtPositionInTheList(int position)
		{
			EventualAssert.That(() => Pages.Pages.CurrentMessageReplyPage.Reply(position).DisplayVisible(), Is.True);
		}

		[Then(@"I should see this conversation on the message at position '(.*)' in the list")]
		public void ThenIShouldSeeThisConversationOnTheMessageAtPositionInTheList(int position, Table table)
		{
			foreach (var tableRow in table.Rows)
			{
				EventualAssert.That(
					() => Pages.Pages.CurrentMessageReplyPage.DialogueMessages(position).InnerHtml.Contains(tableRow[0]),
					Is.EqualTo(true));
			}
		}

		[Then(@"the send button should be disabled on the message at position '(.*)' in the list")]
		public void ThenTheSendButtonShouldBeDisabledOnTheMessageAtPositionInTheList(int position)
		{
			EventualAssert.That(() => Pages.Pages.MessagePage.ConfirmButton(position).Enabled, Is.False);
		}

		[Then(@"the send button should be enabled on the message at position '(.*)' in the list")]
		public void ThenTheSendButtonShouldBeEnabled(int position)
		{
			EventualAssert.That(() => Pages.Pages.MessagePage.ConfirmButton(position).Enabled, Is.True);
		}

		[When(@"I click the radiobutton with caption '(.*)' on the message at position '(.*)' in the list")]
		public void WhenIClickTheRadiobuttonWithCaptionOnTheMessageAtPositionInTheList(string option, int position)
		{
			var btn = Pages.Pages.CurrentMessageReplyPage.ReplyOptionsDiv(position).Buttons.First(r => r.InnerHtml.Equals(option)).EventualGet();
			btn.EventualClick();
		}

		[Then(@"the radiobutton with caption '(.*)' should not be checked on the message at position '(.*)' in the list")]
		public void ThenTheRadiobuttonWithCaptionShouldNotBeCheckedOnTheMessageAtPositionInTheList(string option, int position)
		{
			var btn = Pages.Pages.CurrentMessageReplyPage.ReplyOptionsDiv(position).Buttons.First(r => r.InnerHtml.Equals(option)).EventualGet();
			Assert.That(btn.ClassName.Contains("active"),Is.False);
		}

		[Then(@"I should see radiobuttons on the message at position '(.*)' in the list with")]
		public void ThenIShouldSeeRadiobuttonsOnTheMessageAtPositionInTheListWith(int position, Table table)
		{
			var messageDetailDiv = Pages.Pages.CurrentMessageReplyPage.ReplyOptionsDiv(position).EventualGet();
			foreach (var tableRow in table.Rows)
			{
				EventualAssert.That(() => messageDetailDiv.InnerHtml.Contains(tableRow[0]), Is.EqualTo(true));
			}
		}

		[Then(@"I should not see any options on the message at position '(.*)' in the list")]
		public void ThenIShouldNotSeeAnyOptionsOnTheMessageAtPositionInTheList(int position)
		{
			var messageDetailDiv = Pages.Pages.CurrentMessageReplyPage.ReplyOptionsDiv(position).EventualGet();
			foreach (var radioButton in messageDetailDiv.RadioButtons)
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
			Browser.Interactions.AssertExists("#AsmMessages-list");
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