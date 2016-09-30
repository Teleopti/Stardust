﻿using System.Collections.Generic;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.Requests
{
	[Binding]
	public class RequestsStepDefinitions
	{
		private const string approvedStatusText = "Approved";
		private const string deniedStatusText = "Denied";
		private const string cancelledStatusText = "Cancelled";

		[Given(@"'(.*)' has an existing text request with")]
		public void GivenHasAnExistingTextRequestWith(string userName, Table table)
		{
			var textRequest = table.CreateInstance<TextRequestConfigurable>();
			DataMaker.Person(userName).Apply(textRequest);
		}

		[Given(@"'(.*)' has an existing absence request with")]
		public void GivenHasAnExistingAbsenceRequestWith(string userName, Table table)
		{
			var absenceRequest = table.CreateInstance<AbsenceRequestConfigurable>();
			DataMaker.Person(userName).Apply(absenceRequest);
		}

		[When(@"I select to go to shift trade requests view")]
		public void WhenISelectToGoToShiftTradeRequestsView()
		{
			Browser.Interactions.AssertScopeValue("requests-table-container", "requestsOverview.loaded", true);
			Browser.Interactions.AssertExistsUsingJQuery("md-tab-item:contains('Shift Trade')");
			Browser.Interactions.ClickUsingJQuery("md-tab-item:contains('Shift Trade')");
		}

		[When(@"I select to load requests from '(.*)' to '(.*)'")]
		public void WhenISelectToLoadRequestsFromTo(string from, string to)
		{
			Browser.Interactions.FillWith(".request-date-range-picker .start-date-input", from);
			Browser.Interactions.FillWith(".request-date-range-picker .end-date-input", to);
		}

		[When(@"I select to load requests in status '(.*)'")]
		[Then(@"I select to load requests in status '(.*)'")]
		public void WhenISelectToLoadRequestInStatus(string status)
		{
			Browser.Interactions.ClickUsingJQuery(".test-status-selector");
			Browser.Interactions.ClickUsingJQuery("md-select-menu md-option:contains(\'" + status + "\')");
			Browser.Interactions.AssertExists(".md-click-catcher");
			Browser.Interactions.ClickUsingJQuery(".md-click-catcher");
		}

		[Then(@"I should see a request from '(.*)' in the list")]
		public void ThenIShouldSeeARequestFromInTheList(string userName)
		{
			Browser.Interactions.AssertScopeValue("requests-table-container", "requestsOverview.loaded", true);
			Browser.Interactions.AssertAnyContains(".request-agent-name .ui-grid-cell-contents", userName);
		}

		[Then(@"I should see a shift request from '(.*)' in the list")]
		public void ThenIShouldSeeAShiftRequestFromInTheList(string userName)
		{
			Browser.Interactions.AssertScopeValue("requests-table-container", "requestsOverview.loaded", true);
			Browser.Interactions.AssertAnyContains(".ui-grid-cell [class=\"ng-binding\"]", userName);
		}

		[When(@"I sort the request list by descending agent name")]
		public void WhenISortTheRequestListByDescendingAgentName()
		{
			Browser.Interactions.AssertScopeValue("requests-table-container", "requestsOverview.loaded", true);
			Browser.Interactions.ClickUsingJQuery(".request-agent-name-header .ui-grid-column-menu-button");
			Browser.Interactions.ClickUsingJQuery(".ui-grid-menu-items li:nth(1) button");
		}

		[Then(@"I should see the request from '(.*)' before the request from '(.*)' in the list")]
		public void ThenIShouldSeeTheRequestFromBeforeTheRequestFromInTheList(string agentName1, string agentName2)
		{
			Browser.Interactions.AssertExistsUsingJQuery(".ui-grid-row:contains('{0}') + .ui-grid-row:contains('{1}')",
				agentName1, agentName2);
		}

		[When(@"I approve all requests that I see")]
		public void WhenIApproveAllRequestsThatISee()
		{
			Browser.Interactions.AssertScopeValue("requests-table-container", "requestsOverview.loaded", true);
			Browser.Interactions.ClickUsingJQuery(".ui-grid-row .ui-grid-selection-row-header-buttons");
			Browser.Interactions.ClickUsingJQuery("requests-commands-pane .approve-requests");
		}

		[When(@"I deny all requests that I see")]
		public void WhenIDenyAllRequestsThatISee()
		{
			Browser.Interactions.AssertScopeValue("requests-table-container", "requestsOverview.loaded", true);
			Browser.Interactions.ClickUsingJQuery(".ui-grid-row .ui-grid-selection-row-header-buttons");
			Browser.Interactions.ClickUsingJQuery("requests-commands-pane .deny-requests");
		}

		[Then(@"I should see request for '(.*)' approved")]
		public void ThenIShouldSeeRequestForApproved(string agentName)
		{
			Browser.Interactions.AssertExistsUsingJQuery($".ui-grid-row:contains('{agentName}') .request-status:contains('" +
														 approvedStatusText + "')");
		}

		[Then(@"I should see request for '(.*)' denied")]
		public void ThenIShouldSeeRequestForDenied(string agentName)
		{
			Browser.Interactions.AssertExistsUsingJQuery($".ui-grid-row:contains('{agentName}') .request-status:contains('" +
														 deniedStatusText + "')");
		}

		[Then(@"I should see request for '(.*)' cancelled")]
		public void ThenIShouldSeeRequestForCanceled(string agentName)
		{
			Browser.Interactions.AssertExistsUsingJQuery($".ui-grid-row:contains('{agentName}') .request-status:contains('" +
														 cancelledStatusText + "')");
		}

		[Then(@"I should see all requests approved")]
		public void ThenIShouldSeeAllRequestsApproved()
		{
			Browser.Interactions.AssertExistsUsingJQuery(".request-status:contains('" + approvedStatusText+ "')");
		}

		[Then(@"I should see all requests denied")]
		public void ThenIShouldSeeAllRequestsDenied()
		{
			Browser.Interactions.AssertExistsUsingJQuery(".request-status:contains('" + deniedStatusText + "')");
		}

		[When(@"I click the shift trade schedule day")]
		public void ClickShiftTradeScheduleDay()
		{
			Browser.Interactions.ClickUsingJQuery(".ui-grid-canvas div[style*='pointer']:first");
		}

		[Then(@"I should see schedule detail")]
		public void ShouldSeeScheduleDetail()
		{
			Browser.Interactions.AssertExistsUsingJQuery(".schedule-container:visible");
		}

		[Then(@"I reply message '(.*)'")]
		public void ThenIReplyMessage(string message)
		{
			fillMessage(message);
			Browser.Interactions.ClickUsingJQuery("requests-reply-message #btnReply");
		}

		[Then(@"I reply and approve with message '(.*)'")]
		public void ThenIReplyAndApproveWithMessage(string message)
		{
			fillMessage(message);
			Browser.Interactions.ClickUsingJQuery("requests-reply-message #btnReplyAndApprove");
		}

		[Then(@"I reply and deny with message '(.*)'")]
		public void ThenIReplyAndDenyWithMessage(string message)
		{
			fillMessage(message);
			Browser.Interactions.ClickUsingJQuery("requests-reply-message #btnReplyAndDeny");
		}

		[Then(@"I reply and cancel with message '(.*)'")]
		public void ThenIReplyAndCancelWithMessage(string message)
		{
			fillMessage(message);
			Browser.Interactions.ClickUsingJQuery("requests-reply-message #btnReplyAndCancel");
		}

		[Then(@"I should see replied messge '(.*)' in message column")]
		public void ThenIShouldSeeRepliedMessgeInMessageColumn(string message)
		{
			Browser.Interactions.ClickUsingJQuery(".ui-grid-icon-menu");
			Browser.Interactions.ClickUsingJQuery(".ui-grid-menu-button button:contains('Message'):first");
			Browser.Interactions.AssertExistsUsingJQuery($".request-message:contains('{message}')");
			Browser.Interactions.ClickUsingJQuery(".ui-grid-icon-menu");
		}

		private void fillMessage(string message)
		{
			displayReplyDialog();

			Browser.Interactions.FillWith("requests-reply-message #replyMessage", message);
			Browser.Interactions.SetScopeValues("requests-reply-message", new Dictionary<string, string>
			{
				{"requestsReplyMessage.replyMessage", message}
			});
		}

		private void displayReplyDialog()
		{
			Browser.Interactions.AssertScopeValue("requests-table-container", "requestsOverview.loaded", true);
			Browser.Interactions.ClickUsingJQuery(".ui-grid-row .ui-grid-selection-row-header-buttons");
			Browser.Interactions.ClickUsingJQuery("requests-commands-pane .reply-requests");
			Browser.Interactions.AssertExistsUsingJQuery(".request-reply-dialog:visible");
		}
	}
}