using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ShiftTradeRequestFromRequestStepDefinitions
	{
		[Given(@"I click the shift trade request's delete button")]
		[When(@"I click the shift trade request's delete button")]
		public void GivenIClickTheShiftTradeRequestSDeleteButton()
		{
			var requestId = UserFactory.User().UserData<ExistingShiftTradeRequest>().PersonRequest.Id.Value;
			Pages.Pages.RequestsPage.RequestDeleteButtonById(requestId).EventualClick();
		}

		[Then(@"I should not see the shift trade request in the list")]
		public void ThenIShouldNotSeeTheShiftTradeRequestInTheList()
		{
			var existingShiftTradeRequest = UserFactory.User().UserData<ExistingShiftTradeRequest>();
			if (existingShiftTradeRequest != null)
			{
				var requestId = existingShiftTradeRequest.PersonRequest.Id.Value;
				EventualAssert.That(() => Pages.Pages.RequestsPage.RequestById(requestId).Exists, Is.False);
				Navigation.GotoRequests();
				EventualAssert.That(() => Pages.Pages.RequestsPage.RequestById(requestId).Exists, Is.False);
				return;
			}
			EventualAssert.That(() => Pages.Pages.RequestsPage.Requests.Count(), Is.EqualTo(0));
		}

		[Then(@"I should not see any delete button on my existing shift trade request")]
		public void ThenIShouldNotSeeAnyDeleteButtonOnMyExistingShiftTradeRequest()
		{
			var requestId = UserFactory.User().UserData<ExistingShiftTradeRequest>().PersonRequest.Id.Value;
			EventualAssert.That(() => Pages.Pages.RequestsPage.RequestDeleteButtonById(requestId).IsDisplayed(), Is.False);
		}

		[Then(@"Shift trade request with subject '(.*)' should be ok by both parts")]
		public void ThenShiftTradeRequestWithSubjectShouldBeOkByBothParts(string subject)
		{
			EventualAssert.That(() => getStatusForShiftTradeWithSubject(subject), Is.EqualTo(ShiftTradeStatus.OkByBothParts));
		}

		[Then(@"Shift trade request with subject '(.*)' should be rejected")]
		public void ThenShiftTradeRequestWithSubjectShouldBeRejected(string subject)
		{
			//TODO: Henke: probably check something else when a shifttrade is rejected...
			EventualAssert.That(() => getStatusForShiftTradeWithSubject(subject), Is.EqualTo(ShiftTradeStatus.OkByMe));
		}

		private ShiftTradeStatus getStatusForShiftTradeWithSubject(string subject)
		{
			ShiftTradeStatus status;

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var repository = new PersonRequestRepository(uow);

				IPersonRequest personRequest = repository.LoadAll().First(r => r.GetSubject(new NoFormatting()).Equals(subject));
				var shiftTrade = personRequest.Request as IShiftTradeRequest;
				status = shiftTrade.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing());

			}

			return status;
		}



	}
}
