using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;
using Teleopti.Ccc.WebBehaviorTest.Pages;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class RequestsStepDefinition
	{
		private RequestsPage _page { get { return Pages.Pages.RequestsPage; } }

		[When(@"I click on the request at position '(.*)' in the list")]
		public void WhenIClickOnTheRequestAtPositionInTheList(int position)
		{
			Browser.Interactions.Click(string.Format(".request-body:nth-child({0})", position));
		}
		
		[When(@"I navigate to the requests page")]
		public void WhenINavigateToTheRequestsPage()
		{
			Navigation.GotoRequests();
		}

		[When(@"I navigate to messages")]
		public void WhenINavigateToMessages()
		{
			Navigation.GotoMessagePage();
		}

		[When(@"I scroll down to the bottom of the page")]
		public void WhenIScrollDownToTheBottomOfThePage()
		{
			var data = UserFactory.User().UserData<MoreThanOnePageOfRequests>();
			EventualAssert.That(() => _page.Requests.Count(), Is.EqualTo(data.PageSize));
			Browser.Interactions.Javascript("$(document).scrollTop($(document).height());");
		}

		[Then(@"I should see a requests list")]
		public void ThenIShouldSeeARequestsList()
		{
			EventualAssert.That(() => _page.RequestListItems.Count(r=>r.IsDisplayed()) > 0, Is.True);
		}

		[Then(@"I should see my existing text request")]
		public void ThenIShouldSeeMyExistingTextRequest()
		{
			EventualAssert.That(() => _page.Requests.Count(), Is.GreaterThan(0));
			EventualAssert.That(() => _page.FirstRequest.InnerHtml, Is.StringContaining(UserFactory.User().UserData<ExistingTextRequest>().PersonRequest.GetSubject(new NoFormatting())));
		}

		[Then(@"I should see a shift trade request in the list with subject '(.*)'")]
		public void ThenIShouldSeeAShiftTradeRequestInTheListWithSubject(string subject)
		{
			EventualAssert.That(() => _page.Requests.Count(), Is.GreaterThan(0));
			EventualAssert.That(() => _page.FirstRequest.InnerHtml, Is.StringContaining(subject));
		}


		[Then(@"I should see my existing absence request")]
		public void ThenIShouldSeeMyExistingAbsenceRequest()
		{
			EventualAssert.That(() => _page.Requests.Count(), Is.GreaterThan(0));
			EventualAssert.That(() => _page.FirstRequest.InnerHtml, Is.StringContaining(UserFactory.User().UserData<ExistingAbsenceRequest>().PersonRequest.GetSubject(new NoFormatting())));
		}


		[Then(@"I should see my existing shift trade request with subject '(.*)'")]
		public void ThenIShouldSeeMyExistingShiftTradeRequestWithSubject(string subject)
		{
			EventualAssert.That(() => _page.Requests.Count(), Is.GreaterThan(0));
			EventualAssert.That(() => _page.FirstRequest.InnerHtml, Is.StringContaining(subject));
		}

		[Then(@"I should not see any request")]
		public void ThenIShouldNotSeeAnyRequest()
		{
			EventualAssert.That(() => _page.Requests.Count(), Is.EqualTo(0));
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
			var page = Browser.Current.Page<PortalPage>();
			EventualAssert.That(() => page.RequestsLink.Exists, Is.True);
		}

		[Then(@"I should not be able to see requests link")]
		public void ThenIShouldNotBeAbleToSeeRequestsLink()
		{
			var page = Browser.Current.Page<PortalPage>();
			EventualAssert.That(() => page.Menu.Exists, Is.True);
			EventualAssert.That(() => page.RequestsLink.Exists, Is.False);
		}

		[Then(@"I should only see one page of requests")]
		public void ThenIShouldOnlySeeOnePageOfRequests()
		{
			var data = UserFactory.User().UserData<MoreThanOnePageOfRequests>();
			EventualAssert.That(() => _page.Requests.Count(), Is.EqualTo(data.PageSize));
		}

		[Then(@"I should see the page fill with the next page of requests")]
		public void ThenIShouldSeeThePageFillWithTheNextPageOfRequests()
		{
			var data = UserFactory.User().UserData<MoreThanOnePageOfRequests>();
			EventualAssert.That(() => _page.Requests.Count(), Is.EqualTo(data.RequestCount));
		}

		[Then(@"I should see that the list is sorted on changed date and time")]
		public void ThenIShouldSeeThatTheListIsSortedOnChangedDateAndTime()
		{
			var data = UserFactory.User().UserData<TwoExistingTextRequestChangedOnDifferentTimes>();
			EventualAssert.That(() => _page.FirstRequest.InnerHtml, Is.StringContaining(data.PersonRequest2.GetSubject(new NoFormatting())));
			EventualAssert.That(() => _page.LastRequest.InnerHtml, Is.StringContaining(data.PersonRequest1.GetSubject(new NoFormatting())));
		}

		[Then(@"I should see a user-friendly message explaining that no requests exists")]
		public void ThenIShouldSeeAUser_FriendlyMessageExplainingThatNoRequestsExists()
		{
			EventualAssert.That(()=>_page.NoRequestsFound.IsDisplayed(),Is.True);
		}

		[Then(@"I should see an indication that there are more requests")]
		public void ThenIShouldSeeAnIndicationThatThereAreMoreRequests()
		{
			EventualAssert.That(() => _page.MoreToLoadArrow.IsDisplayed(), Is.True);
		}

		[Then(@"I should not see an indication that there are more requests")]
		public void ThenIShouldNotSeeAnIndicationThatThereAreMoreRequests()
		{
			EventualAssert.That(() => _page.MoreToLoadArrow.IsDisplayed(), Is.False);
		}

		[When(@"I click the Approve button on the shift request")]
		public void WhenIClickTheApproveButtonOnTheShiftRequest()
		{
			_page.ApproveShiftTradeButton.EventualClick();
		}

		[Then(@"I should not see the approve button")]
		public void WhenIShouldNotSeeTheApproveButton()
		{
			EventualAssert.That(()=>_page.ApproveShiftTradeButton.IsDisplayed(),Is.False);
		}

		[Then(@"I should not see the deny button")]
		public void WhenIShouldNotSeeTheDenyButton()
		{
			EventualAssert.That(() => _page.DenyShiftTradeButton.IsDisplayed(), Is.False);
		}

		[When(@"I click the Deny button on the shift request")]
		public void WhenIClickTheDenyButtonOnTheShiftRequest()
		{
			EventualAssert.That(() => _page.RequestsDeleteButton().IsDisplayed(), Is.False);
			_page.DenyShiftTradeButton.EventualClick();
		}

		[Then(@"I should not see a delete button on the request")]
		public void ThenIShouldNotSeeADeletebuttonOnTheRequest()
		{
			Browser.Interactions.AssertNotExists(".request-data-subject", ".close");	
		}

		[Then(@"I should see '(.*)' as the sender of the request")]
		public void ThenIShouldSeeAsTheSenderOfTheRequest(string name)
		{
			EventualAssert.That(() => _page.ShiftTradeSender.Text, Is.EqualTo(name));
		}

		[Then(@"I should see '(.*)' as the receiver of the request")]
		public void ThenIShouldSeeAsTheReceiverOfTheRequest(string name)
		{
			EventualAssert.That(() => _page.ShiftTradeReciever.Text, Is.EqualTo(name));
		}

		[When(@"I click on shifttrade resend button")]
		public void WhenIClickOnShifttradeResendButton()
		{
			Browser.Interactions.ClickContaining(".btn-primary", Resources.SendAgain);
		}

		[When(@"I click on shifttrade cancel button")]
		public void WhenIClickOnShifttradeCancelButton()
		{
			Browser.Interactions.ClickContaining(".btn-danger", Resources.Cancel);
		}

		[Then(@"I should not see any requests")]
		public void ThenIShouldNotSeeAnyRequests()
		{
			EventualAssert.That(()=>_page.Requests,Is.Empty);
		}

		[Then(@"I should see that request at position '(.*)' is processing")]
		public void ThenIShouldSeeThatRequestAtPositionIsProcessing(int position)
		{
			Browser.Interactions.AssertFirstContains(".request-body:nth-child("+position+") .label" , Resources.ProcessingDotDotDot);			
		}

		[Then(@"I should not see resend shifttrade button for request at position '(.*)'")]
		public void ThenIShouldNotSeeResendShifttradeButtonForRequestAtPosition(int position)
		{
			Browser.Interactions.AssertNotExists(".request:nth-child(" + position + ")", ".request:nth-child(" + position + ") .btn[data-bind*='reSend']");

		}

	}
}
