using System.Linq;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Adherence;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Wfm;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Messages
{
	[Binding]
	public class MessageStepDefinitions
	{
		[Then(@"I should see receivers as")]
		public void ThenIShouldSeeReceiversAs(Table table)
		{
			Browser.Interactions.AssertExists(".message-send-feedback");
			var persons = table.CreateSet<RealTimeAdherenceAgentState>();
			foreach (var person in persons)
			{
				Browser.Interactions.AssertExistsUsingJQuery(".message-receivers li:contains('" + person.Name + "')");
			}
		}

		[When(@"I input the message")]
		public void WhenIInputTheMessage(Table table)
		{
			var message = table.CreateInstance<Message>();
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".message-subject", message.Subject);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".message-body", message.Body);
		}

		[When(@"I confirm to send the message")]
		public void WhenIConfirmToSendTheMessage()
		{
			Browser.Interactions.Click(".send-message:enabled");
		}

		[When(@"I send message for")]
		public void WhenISendMessageFor(Table table)
		{
			var persons = table.CreateSet<RealTimeAdherenceAgentState>();
			var ids=persons.Select(x => DataMaker.Data().Person(x.Name).Person.Id.Value);
			
			TestControllerMethods.Logon();
			Navigation.GotoMessageTool(ids);

			ThenIShouldSeeReceiversAs(table);
		}

		[When(@"I \(without signed in\) send message for")]
		public void WhenIWithoutSignedInSendMessageFor(Table table)
		{
			var persons = table.CreateSet<RealTimeAdherenceAgentState>();
			var ids = persons.Select(x => DataMaker.Data().Person(x.Name).Person.Id.Value);

			Navigation.GotoMessageTool(ids);
		}

		[Then(@"I should see send message succeeded")]
		public void ThenIShouldSeeSendMessageSucceeded()
		{
			Browser.Interactions.AssertExists(".message-send-feedback .label-success");
		}


		public class Message
		{
			public string Subject { get; set; }
			public string Body { get; set; }
		}
	}

}