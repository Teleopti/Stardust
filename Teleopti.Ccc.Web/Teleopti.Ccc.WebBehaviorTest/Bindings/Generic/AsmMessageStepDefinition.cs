﻿using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Teleopti.Ccc.WebBehaviorTest.Pages;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class AsmMessageStepDefinition
	{
		//private MessagePage _page { get { return Pages.Pages.MessagePage; } }

		[Then(@"Message tab should be visible")]
		public void ThenMessageTabShouldBeVisible()
		{
			var page = Browser.Current.Page<PortalPage>();
			EventualAssert.That(() => page.MessageLink.Exists, Is.True);
		}

		[Then(@"Message tab should not be visible")]
		public void ThenMessageTabShouldNotBeVisible()
		{
			var page = Browser.Current.Page<PortalPage>();
			EventualAssert.That(() => page.MessageLink.Exists, Is.False);
		}

		[Given(@"I have an unread message with")]
		public void GivenIHaveAnUnreadMessageWith(Table table)
		{
			var message = table.CreateInstance<MessageConfigurable>();
			UserFactory.User().Setup(message);
		}

		[Given(@"I should be notified that I have '(.*)' new message\(s\)")]
		[Then(@"I should be notified that I have '(.*)' new message\(s\)")]
		public void ThenIShouldBeNotifiedThatIHaveNewMessageS(int messageCount)
		{
			var page = Browser.Current.Page<PortalPage>();
			EventualAssert.That(() => page.MessageLink.ClassName.Contains("asm-new-message-indicator"), Is.True);
			EventualAssert.That(() => page.MessageLink.InnerHtml.Contains("(" + messageCount + ")"), Is.True);
		}

		[When(@"I receive a new message")]
		public void WhenIReceiveANewMessage()
		{
			Browser.Current.Eval("Teleopti.MyTimeWeb.AsmMessage.OnMessageBrokerEvent(null);");
		}
	}
}