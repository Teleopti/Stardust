using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;
using Teleopti.Ccc.WebBehaviorTest.Pages;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class RequestsStepDefinition
	{
		private RequestsPage _page { get { return Pages.Pages.RequestsPage; } }

		[When(@"I click on the request")]
		public void WhenIClickOnTheRequest()
		{
			_page.FirstRequest.Click();
		}

		[When(@"I navigate to the requests page")]
		public void WhenINavigateToTheRequestsPage()
		{
			Navigation.GotoRequests();
		}

		[Given(@"I navigate to messages")]
		[When(@"I navigate to messages")]
        [Then(@"I navigate to messages")]
		public void WhenINavigateToMessages()
		{
			Navigation.GotoMessagePage();
		}

		[When(@"I scroll down to the bottom of the page")]
		public void WhenIScrollDownToTheBottomOfThePage()
		{
			var data = UserFactory.User().UserData<MoreThanOnePageOfRequests>();
			EventualAssert.That(() => _page.Requests.Count(), Is.EqualTo(data.PageSize));
			_page.LastRequest.ScrollIntoView();
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

		[Then(@"I should see my existing absence request")]
		public void ThenIShouldSeeMyExistingAbsenceRequest()
		{
			EventualAssert.That(() => _page.Requests.Count(), Is.GreaterThan(0));
			EventualAssert.That(() => _page.FirstRequest.InnerHtml, Is.StringContaining(UserFactory.User().UserData<ExistingAbsenceRequest>().PersonRequest.GetSubject(new NoFormatting())));
		}

		[Then(@"I should see my existing shift trade request")]
		public void ThenIShouldSeeMyExistingShiftTradeRequest()
		{
			EventualAssert.That(() => _page.Requests.Count(), Is.GreaterThan(0));
			EventualAssert.That(() => _page.FirstRequest.InnerHtml, Is.StringContaining(UserFactory.User().UserData<ExistingShiftTradeRequest>().PersonRequest.GetSubject(new NoFormatting())));
		}

		[Then(@"I should not see my existing shift trade request")]
		public void ThenIShouldNotSeeMyExistingShiftTradeRequest()
		{
			EventualAssert.That(() => _page.Requests.Count(), Is.EqualTo(0));
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

		[Then(@"I should see the shift trade request form  with subject '(.*)'")]
		public void ThenIShouldSeeTheShiftTradeRequestFormWithSubject(string subject)
		{
			EventualAssert.That(() => _page.RequestDetailSection.IsDisplayed(),Is.True, "The detailsection should be visible");
			EventualAssert.That(() => _page.IamAShiftTrade.IsDisplayed(), Is.True, "It should show a shifttrade");
			EventualAssert.That(()=>_page.RequestDetailSubjectInput.Text, Is.EqualTo(subject),"The subject should match the specific shifttrade-request");
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

	}

}
