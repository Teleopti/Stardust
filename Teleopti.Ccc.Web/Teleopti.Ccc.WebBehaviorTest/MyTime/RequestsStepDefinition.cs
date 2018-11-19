using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class RequestsStepDefinition
	{
		[When(@"I click on the existing request in the list")]
		public void WhenIClickOnTheExistingRequestInTheList()
		{
			Browser.Interactions.Click(".request-body");
		}

		[Then(@"I click '(.*)' at end time drop down button")]
		public void ThenIClickAtEndTimeDropDownButton(string p0)
		{
			Browser.Interactions.ClickContaining(".row .filter-time-dropdown-button .dropdown-toggle", "Sluttid");
			Browser.Interactions.Click(".filter-time-dropdown-button .end-time-sort-order .glyphicon-arrow-up");
		}

		[When(@"I navigate to the requests page")]
		public void WhenINavigateToTheRequestsPage()
		{
			Navigation.GotoRequests();
		}

		[When(@"I navigate to messages")]
		public void WhenINavigateToMessages()
		{
			Browser.Interactions.AssertUrlContains("Schedule/Week");
			Navigation.GotoMessagePage();
		}

		[When(@"I scroll down to the bottom of the page")]
		public void WhenIScrollDownToTheBottomOfThePage()
		{
			var data = DataMaker.Data().UserData<MoreThanOnePageOfRequests>();
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".request:nth({0})", data.PageSize-1));

			var top = Browser.Interactions.Javascript_IsFlaky("return $('.bottom-of-requests-page').position().top");
			Browser.Interactions.AssertJavascriptResultContains("window.scroll(0," + top + "); return '1';", "1");
		}

		[Then(@"I should see a requests list")]
		public void ThenIShouldSeeARequestsList()
		{
			Browser.Interactions.AssertVisibleUsingJQuery(".request");
		}

		[Then(@"I should see the request of type '(.*)' in the list")]
		public void ThenIShouldSeeTheRequestOfTypeInTheList(string type)
		{
			Browser.Interactions.AssertExists(".request-body");
			Browser.Interactions.AssertFirstContains(".request-body .request-data-type", type);
		}

		[Then(@"I should see my existing text request")]
		public void ThenIShouldSeeMyExistingTextRequest()
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".request-data-subject:first:contains('{0}')", DataMaker.Data().UserData<ExistingTextRequest>().PersonRequest.GetSubject(new NoFormatting())));
		}

		[Then(@"I should see a request with date period '(.*)'")]
		public void ThenIShouldSeeARequestWithDatePeriod(string datePeriod)
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".request-data-date:first:contains('{0}')", datePeriod));
		}


		[Then(@"I should see a shift trade request in the list with subject '(.*)'")]
		public void ThenIShouldSeeAShiftTradeRequestInTheListWithSubject(string subject)
		{
			Browser.Interactions.AssertExists(".request");
			Browser.Interactions.AssertFirstContains(".request-data-subject",subject);
		}


		[Then(@"I should see my existing absence request")]
		public void ThenIShouldSeeMyExistingAbsenceRequest()
		{
			Browser.Interactions.AssertExists(".request");
			Browser.Interactions.AssertFirstContains(".request-data-subject", DataMaker.Data().UserData<ExistingAbsenceRequest>().PersonRequest.GetSubject(new NoFormatting()));
		}


		[Then(@"I should see my existing shift trade request with subject '(.*)'")]
		public void ThenIShouldSeeMyExistingShiftTradeRequestWithSubject(string subject)
		{
			Browser.Interactions.AssertExists(".request");
			Browser.Interactions.AssertFirstContains(".request-body", subject);
		}

		[Then(@"I should see my existing shift trade request with status waiting for other part")]
		public void ThenIShouldSeeMyExistingShiftTradeRequestWithStatusWaitingForOtherPart()
		{
			ThenIShouldSeeMyExistingShiftTradeRequestWithSubject(Resources.WaitingForOtherPart);
		}

		[Then(@"I should see my existing shift trade request with status waiting for your approval")]
		public void ThenIShouldSeeMyExistingShiftTradeRequestWithStatusWaitingForYourApproval()
		{
			ThenIShouldSeeMyExistingShiftTradeRequestWithSubject(Resources.WaitingForYourApproval);
		}

		[Then(@"I should be able to see requests link")]
		public void ThenIShouldBeAbleToSeeRequestsLink()
		{
			Browser.Interactions.AssertExistsUsingJQuery("a[href='#RequestsTab']");
		}

		[Then(@"I should not be able to see requests link")]
		public void ThenIShouldNotBeAbleToSeeRequestsLink()
		{
			Browser.Interactions.AssertNotExistsUsingJQuery(".teleopti-mytime-top-menu", "a[href='#RequestsTab']");
		}

		[Then(@"I should only see one page of requests")]
		public void ThenIShouldOnlySeeOnePageOfRequests()
		{
			var data = DataMaker.Data().UserData<MoreThanOnePageOfRequests>();
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".request:nth({0})", data.PageSize - 1));
		}

		[Then(@"I should see the page fill with the next page of requests")]
		public void ThenIShouldSeeThePageFillWithTheNextPageOfRequests()
		{
			var data = DataMaker.Data().UserData<MoreThanOnePageOfRequests>();
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".request:nth({0})", data.RequestCount - 1));
		}

		[Then(@"I should see that the list is sorted on changed date and time")]
		public void ThenIShouldSeeThatTheListIsSortedOnChangedDateAndTime()
		{
			var data = DataMaker.Data().UserData<TwoExistingTextRequestChangedOnDifferentTimes>();
			Browser.Interactions.AssertFirstContainsUsingJQuery(".request-subject:first", data.PersonRequest2.GetSubject(new NoFormatting()));
			Browser.Interactions.AssertFirstContainsUsingJQuery(".request-subject:last", data.PersonRequest1.GetSubject(new NoFormatting()));
		}

		[Then(@"I should see a user-friendly message explaining that no requests exists")]
		public void ThenIShouldSeeAUser_FriendlyMessageExplainingThatNoRequestsExists()
		{
			Browser.Interactions.AssertVisibleUsingJQuery("#Requests-no-requests-found");
		}

		[Then(@"I should see an indication that there are more requests")]
		public void ThenIShouldSeeAnIndicationThatThereAreMoreRequests()
		{
			Browser.Interactions.AssertVisibleUsingJQuery(".arrow-down");
		}

		[Then(@"I should not see an indication that there are more requests")]
		public void ThenIShouldNotSeeAnIndicationThatThereAreMoreRequests()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery(".arrow-down");
		}

		[When(@"I click the Approve button on the shift request")]
		public void WhenIClickTheApproveButtonOnTheShiftRequest()
		{
			Browser.Interactions.ClickUsingJQuery("#Approve-shift-trade");
		}

		[Then(@"I should not see the approve button")]
		public void ThenIShouldNotSeeTheApproveButton()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery("#Approve-shift-trade");
		}

		[Then(@"I should not see the deny button")]
		public void WhenIShouldNotSeeTheDenyButton()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery("#Deny-shift-trade");
		}

		[When(@"I click the Deny button on the shift request")]
		public void WhenIClickTheDenyButtonOnTheShiftRequest()
		{
			Browser.Interactions.AssertNotExists("#Deny-shift-trade", ".request-delete");
			Browser.Interactions.ClickUsingJQuery("#Deny-shift-trade");
		}

		[Then(@"I should not see a delete button on the request")]
		public void ThenIShouldNotSeeADeletebuttonOnTheRequest()
		{
			Browser.Interactions.AssertNotExists(".request-data-subject", ".request-delete");	
		}

		[Then(@"I should see '(.*)' as the sender of the request")]
		public void ThenIShouldSeeAsTheSenderOfTheRequest(string name)
		{
			Browser.Interactions.AssertFirstContains("#Request-shift-trade-sender", name);
		}

		[Then(@"I should see '(.*)' as the receiver of the request")]
		public void ThenIShouldSeeAsTheReceiverOfTheRequest(string name)
		{
			Browser.Interactions.AssertFirstContains("#Request-shift-trade-reciever", name);
		}

		[When(@"I click on shifttrade resend button")]
		public void WhenIClickOnShifttradeResendButton()
		{
			Browser.Interactions.ClickContaining(".btn-primary", Resources.SendAgain);
		}

		[When(@"I click on shifttrade cancel button")]
		public void WhenIClickOnShifttradeCancelButton()
		{
			Browser.Interactions.ClickUsingJQuery(".cancel-shift-trade");
		}

		[Then(@"I should not see any requests")]
		[Then(@"I should not see any request")]
		public void ThenIShouldNotSeeAnyRequests()
		{
			Browser.Interactions.AssertNotExists("#Requests-body-inner", ".request");
		}

		[Then(@"I should not see any request in current view")]
		[When(@"I should not see any request in current view")]
		public void ThenIShouldNotSeeAnyRequestInCurrentView()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery("#Requests-data-binding-area");
		}

		[Then(@"I should see that the existing request is processing")]
		public void ThenIShouldSeeThatTheExistingRequestIsProcessing()
		{
			Browser.Interactions.AssertFirstContains(".request-body .label" , Resources.ProcessingDotDotDot);			
		}

		[Then(@"I should not see resend shifttrade button for the request")]
		public void ThenIShouldNotSeeResendShifttradeButtonForTheRequest()
		{
			Browser.Interactions.AssertNotExists(".request", ".request .resend-shift-trade");
		}

		[Then(@"I should not see cancel shifttrade button for the request")]
		public void ThenIShouldNotSeeCancelShifttradeButtonForTheRequest()
		{
			Browser.Interactions.AssertNotExists(".request", ".request .cancel-shift-trade");
		}

		[Then(@"Details should be closed")]
		public void ThenDetailsShouldBeClosed()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery(".request-edit");
		}

		[Then(@"I could edit message for the reason why I approve or deny this request")]
		public void ThenICouldEditMessageForTheReasonWhyIApproveOrDenyThisRequest()
		{
			Browser.Interactions.AssertExists("#Request-detailed-shift-trade-message-input");
		}

		[When(@"I entered '(.*)' as comment")]
		public void WhenIEnteredAsComment(string message)
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-detailed-shift-trade-message-input", message);
		}

		[Then(@"I should see '(.*)' in message area")]
		public void ThenIShouldSeeInMessageArea(string message)
		{
			Browser.Interactions.Click(".request-data-message");
			Browser.Interactions.AssertAnyContains(".request-text", message);
		}
	}
}
