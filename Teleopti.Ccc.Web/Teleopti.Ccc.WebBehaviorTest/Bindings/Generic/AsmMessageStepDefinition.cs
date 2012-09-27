﻿using System;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Teleopti.Ccc.WebBehaviorTest.Pages;
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

		[When(@"I receive a new message")]
		public void WhenIReceiveANewMessage()
		{
			Browser.Current.Eval("Teleopti.MyTimeWeb.AsmMessage.OnMessageBrokerEvent(null);");
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
            EventualAssert.That(() => _page.MessageList.Exists, Is.False);
        }

        [Then(@"I should see a message in the list")]
        public void ThenIShouldSeeAMessageInTheList()
        {
            EventualAssert.That(() => _page.MessageList.Exists, Is.True);
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

        [Then(@"I should see the message details form with")]
        public void ThenIShouldSeeTheMessageDetailsFormWith(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I should see the message with title '(.*)' at position '(.*)' in the list")]
        public void ThenIShouldSeeTheMessageWithTitleAtPositionInTheList(string title, int listPosition)
        {
            _page.MessageList.Elements.Count.Should().Be.EqualTo(listPosition);
            EventualAssert.That(() => _page.FirstMessage.InnerHtml.Contains(title), Is.True);
        }

        [Given(@"I click on the message at position '(.*)' in the list")]
        public void GivenIClickOnTheMessageAtPositionInTheList(int position)
        {
            _page.FirstMessage.Click();
        }

        [When(@"I click the confirm button")]
        public void WhenIClickTheConfirmButton()
        {
            Pages.Pages.CurrentOkButton.OkButton.EventualClick();
        }

        [When(@"I confirm reading the message at position '(.*)' in the list")]
        public void WhenIConfirmReadingTheMessageAtPositionInTheList(int p0)
        {
            _page.FirstMessage.Click();
            Pages.Pages.CurrentOkButton.OkButton.EventualClick();
        }

        [Then(@"I should see a user-friendly message explaining I dont have any messages")]
        public void ThenIShouldSeeAUser_FriendlyMessageExplainingIDontHaveAnyMessages()
        {
            EventualAssert.That(() => _page.FriendlyMessage.Exists, Is.True);
        }
	}
}