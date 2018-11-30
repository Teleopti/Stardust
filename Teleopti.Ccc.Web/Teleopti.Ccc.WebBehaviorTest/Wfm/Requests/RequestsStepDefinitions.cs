using System.Collections.Generic;
using System.Threading;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using AbsenceRequestConfigurable = Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable.AbsenceRequestConfigurable;

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
			addTextRequest(userName, table);
		}

		[Given(@"'(.*)' has '(.*)' text requests with")]
		public void GivenHasTextRequestsWith(string userName, int count, Table table)
		{
			for (var i = 0; i < count; i++)
			{
				addTextRequest(userName, table, i);
			}
		}

		[Given(@"'(.*)' has an existing absence request with")]
		public void GivenHasAnExistingAbsenceRequestWith(string userName, Table table)
		{
			var absenceRequest = table.CreateInstance<AbsenceRequestConfigurable>();
			DataMaker.Person(userName).Apply(absenceRequest);
		}

		[Given(@"'(.*)' has an overtime request with")]
		public void GivenHasAnOvertimeRequestWith(string agentName, Table table)
		{
			var overtimeRequest = table.CreateInstance<OvertimeRequestConfigurable>();
			DataMaker.Person(agentName).Apply(overtimeRequest);
		}

		[Given(@"'(.*)' has a workflow control set with overtime request open periods")]
		public void GivenHasAWorkflowControlSetWithOvertimeRequestOpenPeriods(string agentName)
		{
			DataMaker.Data()
				.Apply(new WorkflowControlSetConfigurable
				{
					Name = "Published 100 days, SA open",
					SchedulePublishedToDate = "2030-12-01",
					StudentAvailabilityPeriodIsClosed = false,
					OvertimeRequestOpenPeriodRollingStart = 0,
					OvertimeRequestOpenPeriodRollingEnd = 13
				});
			DataMaker.Person(agentName).Apply(new WorkflowControlSetForUser { Name = "Published 100 days, SA open" });
		}


		[When(@"I select to go to shift trade requests view")]
		public void WhenISelectToGoToShiftTradeRequestsView()
		{
			Browser.Interactions.ClickUsingJQuery("md-tab-item:contains('Shift Trade')");
		}

		[When(@"I select to go to absence and text requests view")]
		public void WhenISelectToGoToAbsenceAndTextRequestsView()
		{
			Browser.Interactions.ClickUsingJQuery("md-tab-item:contains('Absence and Text')");
		}

		[When(@"I select to go to overtime view")]
		public void WhenISelectToGoToOvertimeView()
		{
			TestControllerMethods.Logon();
			Navigation.GoToPage("wfm/#/requests/overtime");
		}

		[When(@"I click button for search requests")]
		public void WhenIClickButtonForSearchRequests()
		{
			Browser.Interactions.ClickUsingJQuery("span.search-icon");
		}

		[When(@"I select all the team")]
		public void WhenISelectAllTheTeam()
		{
			Browser.Interactions.ClickUsingJQuery(".group-page-picker-menu md-checkbox");
		}

		[When(@"I select to load requests from '(.*)' to '(.*)'")]
		public void WhenISelectToLoadRequestsFromTo(string from, string to)
		{
			Browser.Interactions.FillWith(".request-date-range-picker .start-date-input", from);
			Browser.Interactions.FillWith(".request-date-range-picker .end-date-input", to);
		}

		[When(@"I select date range from '(.*)' to '(.*)'")]
		public void WhenISelectDateRangeFromTo(string from, string to)
		{
			Browser.Interactions.SetScopeValues(".wfm-requests", new Dictionary<string, string>
			{
				{ "vm.period", "{" + string.Format("startDate: new Date('{0}'), endDate: new Date('{1}')", from, to) + "}" }
			});
		}

		[When(@"I select date range from '(.*)' to '(.*)' after '(.*)' milliseconds")]
		public void WhenISelectDateRangeFromToAfterAWhile(string from, string to, int afterMilliseconds)
		{
			Thread.Sleep(afterMilliseconds);
			Browser.Interactions.SetScopeValues(".wfm-requests", new Dictionary<string, string>
			{
				{ "vm.period", "{" + string.Format("startDate: new Date('{0}'), endDate: new Date('{1}')", from, to) + "}" }
			});
		}

		[Then(@"I select first request in the first page")]
		public void ThenISelectFirstRequestInTheFirstPage()
		{
			Browser.Interactions.WaitScopeCondition(".ui-grid", "vm.isLoading", false,
				() =>
				{
					Browser.Interactions.ClickUsingJQuery(".ui-grid-icon-ok:first-child");
				});
		}

		[When(@"I change to the second page")]
		public void WhenIChangeToTheSecondPage()
		{
			Browser.Interactions.ClickUsingJQuery("li.pagination-item.ng-scope:last");
		}

		[When(@"I change to the first page")]
		public void WhenIChangeToTheFirstPage()
		{
			Browser.Interactions.ClickUsingJQuery("li.pagination-item.ng-scope:first");
		}

		[Then(@"I should see all requests should be selected")]
		public void ThenIShouldSeeAllRequestsShouldBeSelected()
		{
			Browser.Interactions.WaitScopeCondition(".ui-grid", "vm.isLoading", false,
				() =>
				{
					Browser.Interactions.AssertScopeValue(".ui-grid", "vm.gridApi.grid.selection.selectAll", true);
				});
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

		[Then(@"I should see a shift request from '(.*)' in the list")]
		public void ThenIShouldSeeAShiftRequestFromInTheList(string userName)
		{
			Browser.Interactions.WaitScopeCondition(".ui-grid", "vm.isLoading", false,
					() =>
					{
						Browser.Interactions.AssertAnyContains("td.ng-binding", userName);
					});
		}

		[Then(@"I should see a absence and text request from '(.*)' in the list")]
		public void ThenIShouldSeeAAbsenceAndTextRequestFromInTheList(string userName)
		{
			Thread.Sleep(1000);
			Browser.Interactions.WaitScopeCondition(".ui-grid", "vm.isLoading", false,
				() =>
				{
					Browser.Interactions.AssertAnyContains(".ui-grid .request-absence-name-cell", userName);
				});
		}

		[Then(@"I should see a overtime request from '(.*)' in the list")]
		public void ThenIShouldSeeAOvertimeRequestFromInTheList(string userName)
		{
			Browser.Interactions.WaitScopeCondition(".ui-grid", "vm.isLoading", false,
				() =>
				{
					Browser.Interactions.AssertAnyContains("div.ui-grid-cell-contents.ng-binding", userName);
				});
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
		[Then(@"I approve all requests that I see")]
		public void WhenIApproveAllRequestsThatISee()
		{
			Browser.Interactions.ClickUsingJQuery(".wfm-requests .ui-grid-header .ui-grid-selection-row-header-buttons");
			Browser.Interactions.ClickUsingJQuery(".wfm-requests requests-commands-pane .approve-requests");
		}

		[When(@"I deny all requests that I see")]
		[Then(@"I deny all requests that I see")]
		public void WhenIDenyAllRequestsThatISee()
		{
			Browser.Interactions.ClickUsingJQuery(".wfm-requests .ui-grid-header .ui-grid-selection-row-header-buttons");
			Browser.Interactions.ClickUsingJQuery(".wfm-requests requests-commands-pane .deny-requests");
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
			Browser.Interactions.AssertExistsUsingJQuery(".request-status:contains('" + approvedStatusText + "')");
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

		[Then(@"I should see a success message")]
		public void ThenIShouldSeeASuccessMessage()
		{
			Browser.Interactions.AssertExists(".notice-container .notice-success");
		}

		[Then(@"I should not see an error message")]
		public void ThenIShouldNotSeeAnErrorMessage()
		{
			Browser.Interactions.AssertNotExists(".request-body", "div.notice-container .notice-error");
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

		private static void addTextRequest(string userName, Table table, int index = 0)
		{
			var textRequest = table.CreateInstance<TextRequestConfigurable>();
			if (index > 0) textRequest.Subject += index;
			DataMaker.Person(userName).Apply(textRequest);
		}
	}
}