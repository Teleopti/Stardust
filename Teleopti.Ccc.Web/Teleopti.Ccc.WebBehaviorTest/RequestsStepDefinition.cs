using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User;
using Teleopti.Ccc.WebBehaviorTest.Pages;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class RequestsStepDefinition
	{
		private RequestsPage _page;

		[Given(@"I am viewing requests")]
		[When(@"I am viewing requests")]
		[When(@"I view requests")]
		public void GivenIAmViewingRequests()
		{
			var userName = UserFactory.User().MakeUser();
			Navigation.GotoGlobalSignInPage();
			var page = Browser.Current.Page<SignInPage>();
			page.SignInApplication(userName, TestData.CommonPassword);
			Navigation.GotoRequests();
			_page = Browser.Current.Page<RequestsPage>();
		}

		[When(@"I click on the request")]
		public void WhenIClickOnTheRequest()
		{
			_page.FirstRequest.Click();
		}

		[When(@"I navigate to the requests page")]
		public void WhenINavigateToTheRequestsPage() { Navigation.GotoRequests(); }

		[When(@"I scroll down to the bottom of the page")]
		public void WhenIScrollDownToTheBottomOfThePage()
		{
			var data = UserFactory.User().UserData<MoreThanOnePageOfRequests>();
			EventualAssert.That(() => _page.Requests.Count(), Is.EqualTo(data.PageSize));
			_page.LastRequest.ScrollIntoView();
		}

		[Then(@"I should see a requests list")]
		public void ThenIShouldSeeARequestsList() { EventualAssert.That(() => _page.RequestsList.Exists, Is.True); }

		[Then(@"I should see my existing text request")]
		public void ThenIShouldSeeMyExistingTextRequest()
		{
			EventualAssert.That(() => _page.Requests.Count(), Is.GreaterThan(0));
			EventualAssert.That(() => _page.FirstRequest.InnerHtml, Is.StringContaining(UserFactory.User().UserData<ExistingTextRequest>().PersonRequest.Subject));
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
			EventualAssert.That(() => _page.FirstRequest.InnerHtml, Is.StringContaining(data.PersonRequest2.Subject));
			EventualAssert.That(() => _page.LastRequest.InnerHtml, Is.StringContaining(data.PersonRequest1.Subject));
		}

	}

}
